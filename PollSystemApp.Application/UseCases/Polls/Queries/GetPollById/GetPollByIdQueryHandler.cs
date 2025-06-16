using AutoMapper;
using MediatR;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Responses; 
using PollSystemApp.Domain.Common.Exceptions;    
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollById
{
    public class GetPollByIdQueryHandler : IRequestHandler<GetPollByIdQuery, ApiBaseResponse>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public GetPollByIdQueryHandler(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }

        public async Task<ApiBaseResponse> Handle(GetPollByIdQuery request, CancellationToken cancellationToken)
        {
            var poll = await _repositoryManager.Polls
                                .FindByCondition(p => p.Id == request.Id, trackChanges: false)
                                .Include(p => p.Tags)
                                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException(nameof(Poll), request.Id);
            var pollDto = _mapper.Map<PollDto>(poll);

            var options = await _repositoryManager.Options
                                    .FindByCondition(o => o.PollId == poll.Id, trackChanges: false)
                                    .OrderBy(o => o.Order)
                                    .ToListAsync(cancellationToken);

            pollDto.Options = _mapper.Map<List<OptionDto>>(options);

            return new ApiOkResponse<PollDto>(pollDto);
        }
    }
}