using MediatR;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.UseCases.Polls.Commands.DeletePollOption
{
    public class DeletePollOptionCommandHandler : IRequestHandler<DeletePollOptionCommand, Unit>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ICurrentUserService _currentUserService;

        public DeletePollOptionCommandHandler(IRepositoryManager repositoryManager, ICurrentUserService currentUserService)
        {
            _repositoryManager = repositoryManager;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(DeletePollOptionCommand request, CancellationToken cancellationToken)
        {
            var poll = await _repositoryManager.Polls.GetByIdAsync(request.PollId, trackChanges: false);
            if (poll == null)
            {
                throw new NotFoundException(nameof(Poll), request.PollId);
            }

            var currentUserId = _currentUserService.UserId;
            if (poll.CreatedBy != currentUserId && !_currentUserService.IsInRole("Admin"))
            {
                throw new ForbiddenAccessException("You are not authorized to delete options from this poll.");
            }

            var option = await _repositoryManager.Options.FirstOrDefaultAsync(
                o => o.Id == request.OptionId && o.PollId == request.PollId,
                trackChanges: false);

            if (option == null)
            {
                throw new NotFoundException(nameof(Option), request.OptionId);
            }

            var remainingOptionsCount = await _repositoryManager.Options.CountAsync(o => o.PollId == request.PollId && o.Id != request.OptionId);
            if (remainingOptionsCount < 2)
            {
                throw new BadRequestException("A poll must have at least two options. Cannot delete this option.");
            }

            var votesForOption = await _repositoryManager.Votes.CountAsync(v => v.OptionId == request.OptionId);
            if (votesForOption > 0)
            {
                throw new BadRequestException("Cannot delete an option that has votes. Please remove votes first or archive the poll.");
            }

            var summariesForOption = await _repositoryManager.OptionVoteSummaries.CountAsync(ovs => ovs.OptionId == request.OptionId);
            if (summariesForOption > 0)
            {

                throw new BadRequestException("Cannot delete an option that is part of a calculated poll result.");
            }

            _repositoryManager.Options.Delete(option);
            await _repositoryManager.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}