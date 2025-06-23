using MediatR;
using PollSystemApp.Application.Common.Responses;
using System;

namespace PollSystemApp.Application.UseCases.Polls.Commands.DeletePollOption
{
    public class DeletePollOptionCommand : IRequest<ApiBaseResponse>
    {
        public Guid PollId { get; }
        public Guid OptionId { get; }

        public DeletePollOptionCommand(Guid pollId, Guid optionId)
        {
            PollId = pollId;
            OptionId = optionId;
        }
    }
}