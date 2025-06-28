using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IOptionVoteSummaryRepository : IRepositoryBase<OptionVoteSummary>
    {
        Task<IEnumerable<OptionVoteSummary>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<OptionVoteSummary?> GetByIdAsync(Guid id, bool trackChanges);
    }
}
