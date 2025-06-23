using MediatR;
using PollSystemApp.Application.Common.Dto.OptionDtos; 
using PollSystemApp.Application.Common.Responses;   
using System;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollOptionById
{
    public class GetPollOptionByIdQuery : IRequest<ApiBaseResponse>
    {
        public Guid PollId { get; set; }
        public Guid OptionId { get; set; }
    }
}