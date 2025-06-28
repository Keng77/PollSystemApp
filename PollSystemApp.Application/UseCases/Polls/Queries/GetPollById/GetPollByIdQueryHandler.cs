using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollById
{
    public class GetPollByIdQueryHandler : IRequestHandler<GetPollByIdQuery, PollDto>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public GetPollByIdQueryHandler(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }

        public async Task<PollDto> Handle(GetPollByIdQuery request, CancellationToken cancellationToken)
        {
            var poll = await _repositoryManager.Polls.GetPollWithDetailsAsync(request.Id, false, cancellationToken)
                ?? throw new NotFoundException(nameof(Poll), request.Id);

            var pollDto = _mapper.Map<PollDto>(poll);
            pollDto.Options = pollDto.Options.OrderBy(o => o.Order).ToList();

            return pollDto;
        }
    }
}