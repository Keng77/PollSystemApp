using MediatR;
using PollSystemApp.Application.Common.Responses;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PollSystemApp.Application.UseCases.Votes.Commands.CreateVote
{
    public class CreateVoteCommand : IRequest<ApiBaseResponse> 
    {
        [JsonIgnore]
        public Guid PollId { get; set; } 
        public List<Guid> OptionIds { get; set; } = new List<Guid>();
        [JsonIgnore]
        public string IpAddress { get; set; } = string.Empty;
    }
}