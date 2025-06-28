using MediatR;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.UseCases.Polls.Commands.EndPollEarly
{
    public class EndPollEarlyCommandHandler : IRequestHandler<EndPollEarlyCommand, Unit>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ICurrentUserService _currentUserService;

        public EndPollEarlyCommandHandler(IRepositoryManager repositoryManager, ICurrentUserService currentUserService)
        {
            _repositoryManager = repositoryManager;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(EndPollEarlyCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsInRole("Admin"))
            {
                throw new ForbiddenAccessException("Only administrators can end a poll early.");
            }

            var poll = await _repositoryManager.Polls.GetByIdAsync(request.PollId, trackChanges: true);
            if (poll == null)
            {
                throw new NotFoundException(nameof(Poll), request.PollId);
            }

            var now = DateTime.UtcNow;
            if (now >= poll.EndDate)
            {
                throw new BadRequestException("This poll has already ended.");
            }


            if (now < poll.StartDate)
            {
                poll.EndDate = poll.StartDate;
                poll.EndDate = now;
            }
            else
            {
                poll.EndDate = now;
            }

            await _repositoryManager.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}