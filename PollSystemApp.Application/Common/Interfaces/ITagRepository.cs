using PollSystemApp.Domain.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface ITagRepository : IRepositoryBase<Tag>
    {
        Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<Tag?> GetByIdAsync(Guid id, bool trackChanges);
    }
}
