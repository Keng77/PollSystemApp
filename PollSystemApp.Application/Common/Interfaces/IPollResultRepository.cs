using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IPollResultRepository : IRepositoryBase<PollResult>
    {
        Task<IEnumerable<PollResult>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<PollResult?> GetByIdAsync(Guid id, bool trackChanges);
        Task<PollResult?> GetLatestPollResultAsync(Guid pollId, bool trackChanges, CancellationToken cancellationToken);
    }
}
