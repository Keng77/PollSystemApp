using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Polls;
using PollSystemApp.Infrastructure.Common.Persistence;

namespace PollSystemApp.Infrastructure.Polls.Persistence;

public class OptionRepository(AppDbContext appDbContext) :
RepositoryBase<Option>(appDbContext), IOptionRepository
{
    public async Task<Option?> GetByIdAsync(Guid id, bool trackChanges = false) =>
    await FindByCondition(p => p.Id.Equals(id), trackChanges)
    .SingleOrDefaultAsync();

    public async Task<IEnumerable<Option>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges = false) =>
    await FindByCondition(p => ids.Contains(p.Id), trackChanges)
    .ToListAsync();

    public async Task<List<Option>> GetOptionsByPollIdAsync(Guid pollId, bool trackChanges, CancellationToken cancellationToken)
    {
        IQueryable<Option> query = _dbSet.Where(o => o.PollId == pollId);
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }
        return await query.OrderBy(o => o.Order).ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, string>> GetOptionTextsByIdsAsync(IEnumerable<Guid> optionIds, CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(o => optionIds.Contains(o.Id))
            .ToDictionaryAsync(o => o.Id, o => o.Text, cancellationToken);
    }

    public async Task<Option?> GetLastOptionByPollIdAsync
        (Guid pollId, bool trackChanges, CancellationToken cancellationToken)
    {
        IQueryable<Option> query = _dbSet.Where(o => o.PollId == pollId);
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }
        return await query.OrderByDescending(o => o.Order).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<Option>> GetOptionsByIdsAndPollIdAsync
        (Guid pollId, IEnumerable<Guid> optionIds, bool trackChanges, CancellationToken cancellationToken)
    {
        IQueryable<Option> query = _dbSet.Where(o => o.PollId == pollId && optionIds.Contains(o.Id));
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<Option>> GetOptionsByPollIdsAsync(IEnumerable<Guid> pollIds, bool trackChanges, CancellationToken cancellationToken)
    {
        IQueryable<Option> query = _dbSet.Where(o => pollIds.Contains(o.PollId));
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }
        return await query.ToListAsync(cancellationToken);
    }
}
