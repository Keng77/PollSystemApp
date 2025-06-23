using MediatR;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Responses;
using System;

namespace PollSystemApp.Application.UseCases.Polls.Commands.UpdatePoll
{
    public class UpdatePollCommand : IRequest<ApiBaseResponse>
    {
        public Guid Id { get; set; } 
        public PollForUpdateDto PollData { get; set; } = null!;
    }
}
