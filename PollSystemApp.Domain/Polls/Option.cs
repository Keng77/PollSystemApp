using System;

namespace PollSystemApp.Domain.Polls
{
    public class Option
    {
        public Guid Id { get; set; }
        public Guid PollId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}