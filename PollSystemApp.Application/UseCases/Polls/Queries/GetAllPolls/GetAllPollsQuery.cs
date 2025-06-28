using MediatR;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Pagination;


namespace PollSystemApp.Application.UseCases.Polls.Queries.GetAllPolls
{
    public class GetAllPollsQuery : IRequest<PagedResponse<PollDto>>
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value < 1 ? 1 : value);
        }

        public string? TitleSearch { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public DateTime? StartsAfter { get; set; }
        public DateTime? EndsBefore { get; set; }
        public bool? IsActive { get; set; }
    }
}