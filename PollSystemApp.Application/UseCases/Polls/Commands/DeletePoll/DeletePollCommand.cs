using MediatR;

namespace PollSystemApp.Application.UseCases.Polls.Commands.DeletePoll
{
    public class DeletePollCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }

        public DeletePollCommand(Guid id)
        {
            Id = id;
        }
    }
}
