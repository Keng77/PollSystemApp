using PollSystemApp.Application.Common.Pagination;
using PollSystemApp.Application.UseCases.Polls.Queries.GetAllPolls;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IPollRepository : IRepositoryBase<Poll>
    {
        Task<Poll?> GetByIdAsync(Guid id, bool trackChanges);
        Task<Poll?> GetPollWithDetailsAsync(Guid id, bool trackChanges, CancellationToken cancellationToken); 
        Task<PagedList<Poll>> GetPollsAsync(GetAllPollsQuery parameters, bool trackChanges, CancellationToken cancellationToken); 
    }
}
