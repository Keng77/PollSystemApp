using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Pagination;
using PollSystemApp.Application.UseCases.Polls.Queries.GetAllPolls;
using PollSystemApp.Domain.Polls;
using PollSystemApp.Infrastructure.Common.Persistence;

namespace PollSystemApp.Infrastructure.Polls.Persistence
{
    public class PollRepository(AppDbContext appDbContext) :
    RepositoryBase<Poll>(appDbContext), IPollRepository
    {
        public async Task<Poll?> GetByIdAsync(Guid id, bool trackChanges = false) =>
            await FirstOrDefaultAsync(p => p.Id.Equals(id), trackChanges); 

        public async Task<Poll?> GetPollWithDetailsAsync(Guid id, bool trackChanges, CancellationToken cancellationToken)
        {
            IQueryable<Poll> query = _dbSet;
            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }            
            return await query
                .Include(p => p.Tags)
                .Include(p => p.Options)
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<PagedList<Poll>> GetPollsAsync(GetAllPollsQuery parameters, bool trackChanges, CancellationToken cancellationToken)
        {
            IQueryable<Poll> pollsQueryable = _dbSet;
            if (!trackChanges)
            {
                pollsQueryable = pollsQueryable.AsNoTracking();
            }

            pollsQueryable = pollsQueryable.Include(p => p.Tags);

            if (!string.IsNullOrWhiteSpace(parameters.TitleSearch))
            {
                pollsQueryable = pollsQueryable.Where(p => p.Title.Contains(parameters.TitleSearch));
            }
            if (!string.IsNullOrWhiteSpace(parameters.TagSearch))
            {
                var normalizedTagSearch = parameters.TagSearch.ToLower();
                pollsQueryable = pollsQueryable.Where(p =>
                    p.Tags.Any(t => t.Name.ToLower().Contains(normalizedTagSearch)));
            }
            if (parameters.CreatedAfter.HasValue)
            {
                pollsQueryable = pollsQueryable.Where(p => p.CreatedAt >= parameters.CreatedAfter.Value);
            }
            if (parameters.CreatedBefore.HasValue)
            {
                pollsQueryable = pollsQueryable.Where(p => p.CreatedAt < parameters.CreatedBefore.Value.Date.AddDays(1));
            }
            if (parameters.StartsAfter.HasValue)
            {
                pollsQueryable = pollsQueryable.Where(p => p.StartDate >= parameters.StartsAfter.Value);
            }
            if (parameters.EndsBefore.HasValue)
            {
                pollsQueryable = pollsQueryable.Where(p => p.EndDate < parameters.EndsBefore.Value.Date.AddDays(1));
            }
            if (parameters.IsActive.HasValue)
            {
                var now = DateTime.UtcNow;
                if (parameters.IsActive.Value)
                {
                    pollsQueryable = pollsQueryable.Where(p => p.StartDate <= now && p.EndDate >= now);
                }
                else
                {
                    pollsQueryable = pollsQueryable.Where(p => p.StartDate > now || p.EndDate < now);
                }
            }

            pollsQueryable = pollsQueryable.OrderByDescending(p => p.CreatedAt);

            return await PagedList<Poll>.CreateAsync(
                pollsQueryable,
                parameters.PageNumber,
                parameters.PageSize,
                cancellationToken);
        }
    }
}