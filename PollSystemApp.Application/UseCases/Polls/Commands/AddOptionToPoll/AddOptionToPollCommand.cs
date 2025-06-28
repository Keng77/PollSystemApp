using MediatR;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using System.Text.Json.Serialization;

namespace PollSystemApp.Application.UseCases.Polls.Commands.AddOptionToPoll
{
    public class AddOptionToPollCommand : IRequest<OptionDto>
    {
        [JsonIgnore]
        public Guid PollId { get; set; }
        public OptionForCreationDto OptionData { get; set; } = null!;
    }
}