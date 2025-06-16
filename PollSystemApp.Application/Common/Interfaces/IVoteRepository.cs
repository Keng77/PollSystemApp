using PollSystemApp.Domain.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IVoteRepository : IRepositoryBase<Vote>
    {
        Task<IEnumerable<Vote>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<Vote?> GetByIdAsync(Guid id, bool trackChanges);
    }
}
