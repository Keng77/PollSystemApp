using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Polls;
using PollSystemApp.Infrastructure.Common.Persistence;

namespace PollSystemApp.Infrastructure.Polls.Persistence
{
    public class PollResultRepository(AppDbContext appDbContext) :
    RepositoryBase<PollResult>(appDbContext), IPollResultRepository
    {
        public async Task<PollResult?> GetByIdAsync(Guid id, bool trackChanges = false) =>
        await FindByCondition(p => p.Id.Equals(id), trackChanges)
        .SingleOrDefaultAsync();

        public async Task<IEnumerable<PollResult>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges = false) =>
        await FindByCondition(p => ids.Contains(p.Id), trackChanges)
        .ToListAsync();

        public async Task<PollResult?> GetLatestPollResultAsync(Guid pollId, bool trackChanges, CancellationToken cancellationToken)
        {
            IQueryable<PollResult> query = _dbSet.Where(pr => pr.PollId == pollId);
            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }
            return await query
                .Include(pr => pr.Options)
                .OrderByDescending(pr => pr.CalculatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
