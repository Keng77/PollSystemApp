using PollSystemApp.Domain.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IOptionVoteSummaryRepository : IRepositoryBase<OptionVoteSummary>
    {
        Task<IEnumerable<OptionVoteSummary>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<OptionVoteSummary?> GetByIdAsync(Guid id, bool trackChanges);
    }
}
