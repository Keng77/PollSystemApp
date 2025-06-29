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
            var pagedPolls = await _repositoryManager.Polls.GetPollsAsync(request, false, cancellationToken);

            if (!pagedPolls.Any())
            {
                return new PagedResponse<PollDto>(new List<PollDto>(), pagedPolls.MetaData);
            }

            var pollIds = pagedPolls.Select(p => p.Id).ToList();

            var allOptions = await _repositoryManager.Options.GetOptionsByPollIdsAsync(pollIds, false, cancellationToken);

            var optionsByPollId = allOptions
                .GroupBy(o => o.PollId)
                .ToDictionary(g => g.Key, g => g.OrderBy(o => o.Order).ToList());

            var pollDtos = new List<PollDto>();
            foreach (var poll in pagedPolls)
            {
                var pollDto = _mapper.Map<PollDto>(poll);

                if (optionsByPollId.TryGetValue(poll.Id, out var pollOptions))
                {
                    pollDto.Options = _mapper.Map<List<OptionDto>>(pollOptions);
                }

                pollDtos.Add(pollDto);
            }

            return new PagedResponse<PollDto>(pollDtos, pagedPolls.MetaData);
        }
    }
}
