using MediatR;
using System.Text.Json.Serialization;

namespace PollSystemApp.Application.UseCases.Votes.Commands.CreateVote
{
    public class CreateVoteCommand : IRequest<Unit>
    {
        [JsonIgnore]
        public Guid PollId { get; set; }
        public List<Guid> OptionIds { get; set; } = new List<Guid>();
        [JsonIgnore]
        public string IpAddress { get; set; } = string.Empty;
    }
}