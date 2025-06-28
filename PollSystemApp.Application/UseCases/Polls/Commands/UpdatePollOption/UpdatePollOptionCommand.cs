using MediatR;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using System.Text.Json.Serialization;

namespace PollSystemApp.Application.UseCases.Polls.Commands.UpdatePollOption
{
    public class UpdatePollOptionCommand : IRequest<Unit>
    {
        [JsonIgnore]
        public Guid PollId { get; set; }
        [JsonIgnore]
        public Guid OptionId { get; set; }

        public OptionForUpdateDto OptionData { get; set; } = null!;
    }
}