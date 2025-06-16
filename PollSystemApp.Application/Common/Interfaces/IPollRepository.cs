using PollSystemApp.Domain.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IPollRepository : IRepositoryBase<Poll>
    {
        Task<IEnumerable<Poll>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<Poll?> GetByIdAsync(Guid id, bool trackChanges);
    }
}
