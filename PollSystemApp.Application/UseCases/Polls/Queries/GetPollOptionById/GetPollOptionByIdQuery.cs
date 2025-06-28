using MediatR;
using PollSystemApp.Application.Common.Dto.OptionDtos;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollOptionById
{
    public class GetPollOptionByIdQuery : IRequest<OptionDto>
    {
        public Guid PollId { get; set; }
        public Guid OptionId { get; set; }
    }
}