using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Pagination;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetAllPolls
{
    public class GetAllPollsQueryHandler : IRequestHandler<GetAllPollsQuery, PagedResponse<PollDto>>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public GetAllPollsQueryHandler(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }

        public async Task<PagedResponse<PollDto>> Handle(GetAllPollsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Poll> pollsQueryable = _repositoryManager.Polls.FindAll(trackChanges: false);
            pollsQueryable = pollsQueryable.Include(p => p.Tags);

            if (!string.IsNullOrWhiteSpace(request.TitleSearch))
            {
                pollsQueryable = pollsQueryable.Where(p => p.Title.Contains(request.TitleSearch));
            }
            if (request.CreatedAfter.HasValue)
            {
                pollsQueryable = pollsQueryable.Where(p => p.CreatedAt >= request.CreatedAfter.Value);
            }
            if (request.CreatedBefore.HasValue)
            {
                pollsQueryable = pollsQueryable.Where(p => p.CreatedAt < request.CreatedBefore.Value.Date.AddDays(1));
            }
            if (request.StartsAfter.HasValue)
            {
                pollsQueryable = pollsQueryable.Where(p => p.StartDate >= request.StartsAfter.Value);
            }
            if (request.EndsBefore.HasValue)
            {
                pollsQueryable = pollsQueryable.Where(p => p.EndDate < request.EndsBefore.Value.Date.AddDays(1));
            }
            if (request.IsActive.HasValue)
            {
                var now = DateTime.UtcNow;
                if (request.IsActive.Value)
                {
                    pollsQueryable = pollsQueryable.Where(p => p.StartDate <= now && p.EndDate >= now);
                }
                else
                {
                    pollsQueryable = pollsQueryable.Where(p => p.StartDate > now || p.EndDate < now);
                }
            }

            pollsQueryable = pollsQueryable.OrderByDescending(p => p.CreatedAt);

            var pagedPolls = await PagedList<Poll>.CreateAsync(
                pollsQueryable,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var pollDtos = new List<PollDto>();
            foreach (var poll in pagedPolls)
            {
                var pollDto = _mapper.Map<PollDto>(poll);
                var options = await _repositoryManager.Options
                                        .FindByCondition(o => o.PollId == poll.Id, trackChanges: false)
                                        .OrderBy(o => o.Order)
                                        .ToListAsync(cancellationToken);
                pollDto.Options = _mapper.Map<List<OptionDto>>(options);
                pollDtos.Add(pollDto);
            }

            return new PagedResponse<PollDto>(pollDtos, pagedPolls.MetaData);
        }
    }
}