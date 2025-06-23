using MediatR;
using PollSystemApp.Application.Common.Dto.OptionDtos; 
using PollSystemApp.Application.Common.Responses;
using System;
using System.Text.Json.Serialization; 

namespace PollSystemApp.Application.UseCases.Polls.Commands.AddOptionToPoll
{
    public class AddOptionToPollCommand : IRequest<ApiBaseResponse>
    {
        [JsonIgnore]
        public Guid PollId { get; set; }
        public OptionForCreationDto OptionData { get; set; } = null!;
    }
}