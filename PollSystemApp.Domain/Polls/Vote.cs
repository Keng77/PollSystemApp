using System;

namespace PollSystemApp.Domain.Polls
{
    public class Vote
    {
        public Guid Id { get; set; }
        public Guid PollId { get; set; }
        public Guid OptionId { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IpAddress { get; set; } = string.Empty;

        
    }
}