using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IVoteRepository : IRepositoryBase<Vote>
    {
        Task<IEnumerable<Vote>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<Vote?> GetByIdAsync(Guid id, bool trackChanges);
        Task<List<Guid>> GetUserVoteOptionIdsAsync(Guid pollId, Guid userId, bool trackChanges, CancellationToken cancellationToken);
        Task<List<Vote>> GetVotesByPollIdAsync(Guid pollId, bool trackChanges, CancellationToken cancellationToken);
    }
}
