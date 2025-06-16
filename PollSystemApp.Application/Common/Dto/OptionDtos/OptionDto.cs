using System;

namespace PollSystemApp.Application.Common.Dto.OptionDtos
{
    public class OptionDto
    {
        public Guid Id { get; set; }
        public Guid PollId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}