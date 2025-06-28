using MediatR;
using PollSystemApp.Application.Common.Dto.PollResultDtos;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollResults
{
    public class GetPollResultsQuery : IRequest<PollResultDto>
    {
        public Guid PollId { get; set; }
    }
}