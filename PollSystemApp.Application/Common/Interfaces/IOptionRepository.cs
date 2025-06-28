using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IOptionRepository : IRepositoryBase<Option>
    {
        Task<IEnumerable<Option>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<Option?> GetByIdAsync(Guid id, bool trackChanges);
        Task<List<Option>> GetOptionsByPollIdAsync(Guid pollId, bool trackChanges, CancellationToken cancellationToken);
        Task<Dictionary<Guid, string>> GetOptionTextsByIdsAsync(IEnumerable<Guid> optionIds, CancellationToken cancellationToken);
        Task<Option?> GetLastOptionByPollIdAsync(Guid pollId, bool trackChanges, CancellationToken cancellationToken);
        Task<List<Option>> GetOptionsByIdsAndPollIdAsync(Guid pollId, IEnumerable<Guid> optionIds, bool trackChanges, CancellationToken cancellationToken);
        Task<List<Option>> GetOptionsByPollIdsAsync(IEnumerable<Guid> pollIds, bool trackChanges, CancellationToken cancellationToken);
    }
}
