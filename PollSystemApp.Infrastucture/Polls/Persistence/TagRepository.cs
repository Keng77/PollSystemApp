﻿using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Polls;
using PollSystemApp.Infrastructure.Common.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollSystemApp.Infrastructure.Polls.Persistence
{
    public class TagRepository(AppDbContext appDbContext) :
    RepositoryBase<Tag>(appDbContext), ITagRepository
    {
        public async Task<Tag?> GetByIdAsync(Guid id, bool trackChanges = false) =>
        await FindByCondition(p => p.Id.Equals(id), trackChanges)
        .SingleOrDefaultAsync();

        public async Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, bool trackChanges = false) =>
        await FindByCondition(p => ids.Contains(p.Id), trackChanges)
        .ToListAsync();

    }
}
