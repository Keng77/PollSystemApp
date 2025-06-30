using AutoMapper;
using MediatR;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using PollSystemApp.Domain.Users;

namespace PollSystemApp.Application.UseCases.Polls.Commands.UpdatePollOption
{
    public class UpdatePollOptionCommandHandler : IRequestHandler<UpdatePollOptionCommand, Unit>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UpdatePollOptionCommandHandler(IRepositoryManager repositoryManager, IMapper mapper, ICurrentUserService currentUserService)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(UpdatePollOptionCommand request, CancellationToken cancellationToken)
        {
            var poll = await _repositoryManager.Polls.GetByIdAsync(request.PollId, false)
             ?? throw new NotFoundException(nameof(Poll), request.PollId);

            var currentUserId = _currentUserService.UserId;
            if (poll.CreatedBy != currentUserId && !_currentUserService.IsInRole(UserRoles.Admin))
            {
                throw new ForbiddenAccessException("You are not authorized to update options for this poll.");
            }

            var option = await _repositoryManager.Options.FirstOrDefaultAsync(
            o => o.Id == request.OptionId && o.PollId == request.PollId,
            trackChanges: true)
            ?? throw new NotFoundException(nameof(Option), request.OptionId);

            if (option.Order != request.OptionData.Order)
            {
                bool orderExists = await _repositoryManager.Options.ExistsAsync(
                    o => o.PollId == request.PollId &&
                         o.Order == request.OptionData.Order &&
                         o.Id != request.OptionId,
                    cancellationToken);

                if (orderExists)
                {
                    throw new BadRequestException($"An option with order '{request.OptionData.Order}' already exists in this poll.");
                }
            }

            _mapper.Map(request.OptionData, option);
            await _repositoryManager.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}