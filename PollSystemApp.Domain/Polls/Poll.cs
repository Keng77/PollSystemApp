using System;
using System.Collections.Generic;

namespace PollSystemApp.Domain.Polls
{
    public class Poll
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; } 

        public bool IsAnonymous { get; set; }

        public bool IsMultipleChoice { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<Tag> Tags { get; set; } = [];

    }
}