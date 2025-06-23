using MediatR;
using PollSystemApp.Application.Common.Responses;
using System;

namespace PollSystemApp.Application.UseCases.Polls.Commands.EndPollEarly
{
    public class EndPollEarlyCommand : IRequest<ApiBaseResponse>
    {
        public Guid PollId { get; set; }

        public EndPollEarlyCommand(Guid pollId)
        {
            PollId = pollId;
        }
    }
}
