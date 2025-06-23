using MediatR;
using PollSystemApp.Application.Common.Responses;
using System;

namespace PollSystemApp.Application.UseCases.Votes.Queries.CheckUserVote
{
    public class CheckUserVoteQuery : IRequest<ApiBaseResponse>
    {
        public Guid PollId { get; set; }
    }
}