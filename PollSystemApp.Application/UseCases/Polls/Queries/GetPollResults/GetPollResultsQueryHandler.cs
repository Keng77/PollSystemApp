using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Dto.PollResultDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollResults;

public class GetPollResultsQueryHandler : IRequestHandler<GetPollResultsQuery, PollResultDto>
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IPollResultsCalculator _resultsCalculator;

    public GetPollResultsQueryHandler(IRepositoryManager repositoryManager, IMapper mapper, IPollResultsCalculator resultsCalculator)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _resultsCalculator = resultsCalculator;
    }

    public async Task<PollResultDto> Handle(GetPollResultsQuery request, CancellationToken cancellationToken)
    {
        var poll = await _repositoryManager.Polls.GetByIdAsync(request.PollId, false)
            ?? throw new NotFoundException(nameof(Poll), request.PollId);

        if (DateTime.UtcNow <= poll.EndDate && !(poll.EndDate == poll.StartDate))
        {
            throw new BadRequestException($"Results for this poll will be available after {poll.EndDate:o}.");
        }

        var existingPollResult = await _repositoryManager.PollResults.GetLatestPollResultAsync(request.PollId, false, cancellationToken);

        if (existingPollResult != null && (poll.EndDate < existingPollResult.CalculatedAt || poll.EndDate == poll.StartDate))
        {
            return await MapPollResultToDtoAsync(existingPollResult, poll.Title, cancellationToken);
        }

        var calculatedResult = await _resultsCalculator.CalculateResultsAsync(poll, cancellationToken);

        _repositoryManager.PollResults.Create(calculatedResult);
        await _repositoryManager.CommitAsync(cancellationToken);

        return await MapPollResultToDtoAsync(calculatedResult, poll.Title, cancellationToken);
    }

    private async Task<PollResultDto> MapPollResultToDtoAsync(PollResult pollResult, string pollTitle, CancellationToken cancellationToken)
    {
        var resultDto = _mapper.Map<PollResultDto>(pollResult);
        resultDto.PollTitle = pollTitle;
        var optionIds = pollResult.Options.Select(ovs => ovs.OptionId).Distinct().ToList();
        if (optionIds.Any())
        {
            var optionsFromDb = await _repositoryManager.Options.GetOptionTextsByIdsAsync(optionIds, cancellationToken);
            foreach (var summaryDto in resultDto.Options)
            {
                if (optionsFromDb.TryGetValue(summaryDto.OptionId, out var text))
                {
                    summaryDto.OptionText = text;
                }
            }
        }
        return resultDto;
    }
}