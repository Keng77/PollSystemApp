using MediatR;
using PollSystemApp.Application.Common.Dto.PollDtos;

namespace PollSystemApp.Application.UseCases.Polls.Commands.UpdatePoll
{
    public class UpdatePollCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public PollForUpdateDto PollData { get; set; } = null!;
    }
}
