using System;
using System.Collections.Generic;
using PollSystemApp.Application.Common.Dto.OptionDtos;

namespace PollSystemApp.Application.Common.Dto.PollDtos
{
    public class PollForCreationDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsAnonymous { get; set; }
        public bool IsMultipleChoice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<OptionForCreationDto> Options { get; set; } = new List<OptionForCreationDto>();
        public List<string> Tags { get; set; } = new List<string>();
    }
}