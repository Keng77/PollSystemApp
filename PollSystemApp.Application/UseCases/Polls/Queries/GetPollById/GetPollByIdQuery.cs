using MediatR;
using PollSystemApp.Application.Common.Dto.PollDtos;
using System;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollById
{
    public class GetPollByIdQuery : IRequest<PollDto?>
    {
        public Guid Id { get; set; }
    }
}