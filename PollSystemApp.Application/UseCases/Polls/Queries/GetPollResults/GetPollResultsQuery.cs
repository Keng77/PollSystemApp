using MediatR;
using PollSystemApp.Application.Common.Responses;
using System;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollResults
{
    public class GetPollResultsQuery : IRequest<ApiBaseResponse>
    {
        public Guid PollId { get; set; }
    }
}