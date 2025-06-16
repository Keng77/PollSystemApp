using MediatR;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Responses;
using System;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollById
{
    public class GetPollByIdQuery : IRequest<ApiBaseResponse>
    {
        public Guid Id { get; set; }
    }
}