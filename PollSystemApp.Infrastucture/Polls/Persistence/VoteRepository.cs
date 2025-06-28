using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Polls;
using PollSystemApp.Infrastructure.Common.Persistence;

namespace PollSystemApp.Infrastructure.Polls.Persistence
{
    public class VoteRepository(AppDbContext appDbContext) :
    RepositoryBase<Vote>(appDbContext), IVoteRepository
    {
        public async Task<Vote?> GetByIdAsync(Guid id, bool trackChanges = false) =>
        await FindByCondition(p => p.Id.Equals(id), trackChanges)
        .SingleOrDefaultAsync();

        public async Task<IEnumerable<Vote>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges = false) =>
        await FindByCondition(p => ids.Contains(p.Id), trackChanges)
        .ToListAsync();

        public async Task<List<Guid>> GetUserVoteOptionIdsAsync(Guid pollId, Guid userId, bool trackChanges, CancellationToken cancellationToken)
        {
            IQueryable<Vote> query = _dbSet.Where(v => v.PollId == pollId && v.UserId == userId);
            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }
            return await query.Select(v => v.OptionId).ToListAsync(cancellationToken);
        }

        public async Task<List<Vote>> GetVotesByPollIdAsync(Guid pollId, bool trackChanges, CancellationToken cancellationToken)
        {
            IQueryable<Vote> query = _dbSet.Where(v => v.PollId == pollId);
            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync(cancellationToken);
        }
    }
}
