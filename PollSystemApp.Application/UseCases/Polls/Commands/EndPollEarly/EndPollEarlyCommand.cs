using MediatR;

namespace PollSystemApp.Application.UseCases.Polls.Commands.EndPollEarly
{
    public class EndPollEarlyCommand : IRequest<Unit>
    {
        public Guid PollId { get; set; }

        public EndPollEarlyCommand(Guid pollId)
        {
            PollId = pollId;
        }
    }
}
