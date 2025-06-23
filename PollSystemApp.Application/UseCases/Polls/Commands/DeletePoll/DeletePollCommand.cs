using MediatR;
using PollSystemApp.Application.Common.Responses;
using System;

namespace PollSystemApp.Application.UseCases.Polls.Commands.DeletePoll
{
    public class DeletePollCommand : IRequest<ApiBaseResponse>
    {
        public Guid Id { get; set; }

        public DeletePollCommand(Guid id)
        {
            Id = id;
        }
    }
}
