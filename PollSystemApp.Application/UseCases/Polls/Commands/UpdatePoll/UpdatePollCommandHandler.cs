using AutoMapper;
using MediatR;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Responses;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PollSystemApp.Application.UseCases.Polls.Commands.UpdatePoll
{
    public class UpdatePollCommandHandler : IRequestHandler<UpdatePollCommand, ApiBaseResponse>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UpdatePollCommandHandler(IRepositoryManager repositoryManager, IMapper mapper, ICurrentUserService currentUserService)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<ApiBaseResponse> Handle(UpdatePollCommand request, CancellationToken cancellationToken)
        {
            var poll = await _repositoryManager.Polls.GetByIdAsync(request.Id, trackChanges: true);

            if (poll == null)
            {
                throw new NotFoundException(nameof(Poll), request.Id);
            }

            var currentUserId = _currentUserService.UserId;
            if (poll.CreatedBy != currentUserId && !_currentUserService.IsInRole("Admin"))
            {
                throw new ForbiddenAccessException("You are not authorized to update this poll.");
            }

            _mapper.Map(request.PollData, poll);
            await _repositoryManager.CommitAsync(cancellationToken);

            return new ApiOkResponse();
        }
    }
}