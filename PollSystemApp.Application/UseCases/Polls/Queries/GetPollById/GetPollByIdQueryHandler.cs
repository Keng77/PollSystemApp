using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<GetPollByIdQueryHandler> _logger;

        public GetPollByIdQueryHandler(IRepositoryManager repositoryManager, IMapper mapper, ILogger<GetPollByIdQueryHandler> logger)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PollDto> Handle(GetPollByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching poll with ID: {PollId} from repository.", request.Id);
            var poll = await _repositoryManager.Polls.GetPollWithDetailsAsync(request.Id, false, cancellationToken);

            if (poll == null)
            {
                _logger.LogWarning("Poll with ID: {PollId} not found.", request.Id);
                throw new NotFoundException(nameof(Poll), request.Id);
            }
            var pollDto = _mapper.Map<PollDto>(poll);
            pollDto.Options = pollDto.Options.OrderBy(o => o.Order).ToList();

            return pollDto;
        }
    }
}