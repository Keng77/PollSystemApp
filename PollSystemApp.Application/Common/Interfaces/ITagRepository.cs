using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface ITagRepository : IRepositoryBase<Tag>
    {
        Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<Tag?> GetByIdAsync(Guid id, bool trackChanges);
    }
}
