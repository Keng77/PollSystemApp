using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IPollRepository : IRepositoryBase<Poll>
    {
        Task<IEnumerable<Poll>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<Poll?> GetByIdAsync(Guid id, bool trackChanges);
    }
}
