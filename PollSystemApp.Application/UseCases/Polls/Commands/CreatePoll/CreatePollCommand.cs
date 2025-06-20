﻿using MediatR;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Responses;
using System;
using System.Collections.Generic;

namespace PollSystemApp.Application.UseCases.Polls.Commands.CreatePoll
{
    public class CreatePollCommand : IRequest<ApiBaseResponse> 
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsAnonymous { get; set; }
        public bool IsMultipleChoice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<OptionForCreationDto> Options { get; set; } = [];
        public List<string> Tags { get; set; } = [];

    }
}