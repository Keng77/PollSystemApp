using MediatR;

namespace PollSystemApp.Application.UseCases.Polls.Commands.DeletePollOption
{
    public class DeletePollOptionCommand : IRequest<Unit>
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