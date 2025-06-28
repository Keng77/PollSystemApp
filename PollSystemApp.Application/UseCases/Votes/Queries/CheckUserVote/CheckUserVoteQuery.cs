using MediatR;
using PollSystemApp.Application.Common.Dto.VoteDtos;

namespace PollSystemApp.Application.UseCases.Votes.Queries.CheckUserVote
{
    public class CheckUserVoteQuery : IRequest<UserVoteStatusDto>
    {
        public Guid PollId { get; set; }
    }
}