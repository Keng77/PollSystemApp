using System;
using System.Collections.Generic; 

namespace PollSystemApp.Application.Common.Dto.VoteDtos
{
    public class VoteForCreationDto 
    {
        public Guid PollId { get; set; }
        public List<Guid> OptionIds { get; set; } = new List<Guid>();
    }
}