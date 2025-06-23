using AutoMapper;
using MediatR;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Responses;
using Microsoft.EntityFrameworkCore; // Для Include и ToListAsync
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetAllPolls
{
    public class GetAllPollsQueryHandler : IRequestHandler<GetAllPollsQuery, ApiBaseResponse>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public GetAllPollsQueryHandler(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }

        public async Task<ApiBaseResponse> Handle(GetAllPollsQuery request, CancellationToken cancellationToken)
        {
            var pollsQuery = _repositoryManager.Polls
                                .FindAll(trackChanges: false)
                                .Include(p => p.Tags) 
                                .OrderByDescending(p => p.CreatedAt);

            var polls = await pollsQuery.ToListAsync(cancellationToken);
            var pollDtos = new List<PollDto>();

            foreach (var poll in polls)
            {
                var pollDto = _mapper.Map<PollDto>(poll);
                var options = await _repositoryManager.Options
                                        .FindByCondition(o => o.PollId == poll.Id, trackChanges: false)
                                        .OrderBy(o => o.Order)
                                        .ToListAsync(cancellationToken);
                pollDto.Options = _mapper.Map<List<OptionDto>>(options);
                pollDtos.Add(pollDto);
            }

            return new ApiOkResponse<List<PollDto>>(pollDtos);
        }
    }
}