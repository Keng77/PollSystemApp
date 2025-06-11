using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges);
        Task<T?> GetByIdAsync(Guid id, bool trackChanges);
        void Create(T evnt);
        void Update(T evnt);
        void Delete(T evnt);
        Task SaveAsync();
    }

}
