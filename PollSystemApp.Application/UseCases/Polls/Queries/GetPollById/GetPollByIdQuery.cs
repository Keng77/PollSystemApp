using MediatR;
using PollSystemApp.Application.Common.Dto.PollDtos;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollById
{
    public class GetPollByIdQuery : IRequest<PollDto>
    {
        public Guid Id { get; set; }
    }
}