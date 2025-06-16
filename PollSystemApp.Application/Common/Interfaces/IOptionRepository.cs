using PollSystemApp.Domain.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IOptionRepository : IRepositoryBase<Option>
    {
        Task<IEnumerable<Option>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<Option?> GetByIdAsync(Guid id, bool trackChanges);
    }
}
