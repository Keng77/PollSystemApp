using MediatR;
using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Dto.VoteDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.UseCases.Votes.Queries.CheckUserVote
{
    public class CheckUserVoteQueryHandler : IRequestHandler<CheckUserVoteQuery, UserVoteStatusDto>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ICurrentUserService _currentUserService;

        public CheckUserVoteQueryHandler(IRepositoryManager repositoryManager, ICurrentUserService currentUserService)
        {
            _repositoryManager = repositoryManager;
            _currentUserService = currentUserService;
        }

        public async Task<UserVoteStatusDto> Handle(CheckUserVoteQuery request, CancellationToken cancellationToken)
        {
            var poll = await _repositoryManager.Polls.GetByIdAsync(request.PollId, trackChanges: false);
            if (poll == null)
            {
                throw new NotFoundException(nameof(Poll), request.PollId);
            }

            var responseDto = new UserVoteStatusDto { HasVoted = false };

            if (!poll.IsAnonymous)
            {
                if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                {
                    return responseDto;
                }

                var userId = _currentUserService.UserId.Value;

                var userVotes = await _repositoryManager.Votes
                    .FindByCondition(v => v.PollId == request.PollId && v.UserId == userId, trackChanges: false)
                    .Select(v => v.OptionId)
                    .ToListAsync(cancellationToken);

                if (userVotes.Any())
                {
                    responseDto.HasVoted = true;
                    responseDto.VotedOptionIds = userVotes;
                }
            }
            return responseDto;
        }
    }
}