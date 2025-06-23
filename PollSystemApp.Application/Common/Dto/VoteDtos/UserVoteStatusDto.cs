using System;
using System.Collections.Generic;

namespace PollSystemApp.Application.Common.Dto.VoteDtos
{
    public class UserVoteStatusDto
    {
        public bool HasVoted { get; set; }
        public List<Guid>? VotedOptionIds { get; set; }
    }
}