using MediatR;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Responses;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PollSystemApp.Application.UseCases.Polls.Commands.DeletePoll
{
    public class DeletePollCommandHandler : IRequestHandler<DeletePollCommand, ApiBaseResponse?> 
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ICurrentUserService _currentUserService;

        public DeletePollCommandHandler(IRepositoryManager repositoryManager, ICurrentUserService currentUserService)
        {
            _repositoryManager = repositoryManager;
            _currentUserService = currentUserService;
        }

        public async Task<ApiBaseResponse?> Handle(DeletePollCommand request, CancellationToken cancellationToken)
        {
            var poll = await _repositoryManager.Polls.GetByIdAsync(request.Id, trackChanges: false);

            if (poll == null)
            {
                throw new NotFoundException(nameof(Poll), request.Id);
            }

            var currentUserId = _currentUserService.UserId;
            if (poll.CreatedBy != currentUserId && !_currentUserService.IsInRole("Admin"))
            {
                throw new ForbiddenAccessException("You are not authorized to delete this poll.");
            }

            _repositoryManager.Polls.Delete(poll);
            await _repositoryManager.CommitAsync(cancellationToken);

            return new ApiOkResponse();
        }
    }
}