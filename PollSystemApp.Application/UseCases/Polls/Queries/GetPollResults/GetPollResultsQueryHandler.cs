using AutoMapper;
using MediatR;
using PollSystemApp.Application.Common.Dto.PollResultDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Responses;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollResults
{
    public class GetPollResultsQueryHandler : IRequestHandler<GetPollResultsQuery, ApiBaseResponse>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public GetPollResultsQueryHandler(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }

        public async Task<ApiBaseResponse> Handle(GetPollResultsQuery request, CancellationToken cancellationToken)
        {
            var poll = await _repositoryManager.Polls.GetByIdAsync(request.PollId, trackChanges: false);
            if (poll == null)
            {
                throw new NotFoundException(nameof(Poll), request.PollId);
            }

            if (DateTime.UtcNow <= poll.EndDate && !(poll.EndDate == poll.StartDate))
            {
                throw new BadRequestException($"Results for this poll will be available after {poll.EndDate:o}.");
            }

            var existingPollResult = await _repositoryManager.PollResults
                .FindByCondition(pr => pr.PollId == request.PollId, trackChanges: false)
                .Include(pr => pr.Options) 
                .OrderByDescending(pr => pr.CalculatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingPollResult != null)
            {
                if (poll.EndDate < existingPollResult.CalculatedAt || poll.EndDate == poll.StartDate)
                {
                    var resultDto = await MapPollResultToDtoAsync(existingPollResult, poll.Title, cancellationToken);
                    return new ApiOkResponse<PollResultDto>(resultDto);
                }
            }

            var options = await _repositoryManager.Options
                .FindByCondition(o => o.PollId == request.PollId, trackChanges: false)
                .OrderBy(o => o.Order)
                .ToListAsync(cancellationToken);

            if (!options.Any())
            {
                var emptyResultDto = new PollResultDto
                {
                    PollId = poll.Id,
                    PollTitle = poll.Title,
                    TotalVotes = 0,
                    Options = new List<OptionVoteSummaryDto>(),
                    CalculatedAt = DateTime.UtcNow
                };
                return new ApiOkResponse<PollResultDto>(emptyResultDto);
            }

            var votes = await _repositoryManager.Votes
                .FindByCondition(v => v.PollId == request.PollId, trackChanges: false)
                .ToListAsync(cancellationToken);

            int totalVotesInPoll = votes.GroupBy(v => v.UserId)
                                        .Count(g => g.Key.HasValue && g.Key.Value != Guid.Empty);

            if (poll.IsAnonymous) 
            {
                totalVotesInPoll = votes.Count;
                if (poll.IsMultipleChoice && totalVotesInPoll > 0 && options.Count > 0)
                {
                    totalVotesInPoll = votes.Count;
                }
            }


            var newPollResult = new PollResult
            {
                Id = Guid.NewGuid(),
                PollId = poll.Id,
                CalculatedAt = DateTime.UtcNow,
                Options = new List<OptionVoteSummary>()
            };

            foreach (var option in options)
            {
                var votesForOption = votes.Count(v => v.OptionId == option.Id);
                double percentage = (totalVotesInPoll > 0) ? Math.Round(((double)votesForOption / totalVotesInPoll) * 100, 2) : 0;

                var summary = new OptionVoteSummary
                {
                    Id = Guid.NewGuid(),
                    OptionId = option.Id,
                    Votes = votesForOption,
                    Percentage = percentage,
                };
                newPollResult.Options.Add(summary);
            }
            newPollResult.TotalVotes = totalVotesInPoll;

            _repositoryManager.PollResults.Create(newPollResult);
            await _repositoryManager.CommitAsync(cancellationToken);

            var calculatedResultDto = await MapPollResultToDtoAsync(newPollResult, poll.Title, cancellationToken);
            return new ApiOkResponse<PollResultDto>(calculatedResultDto);
        }

        private async Task<PollResultDto> MapPollResultToDtoAsync(PollResult pollResult, string pollTitle, CancellationToken cancellationToken)
        {
            var resultDto = _mapper.Map<PollResultDto>(pollResult);
            resultDto.PollTitle = pollTitle;

            var optionIds = pollResult.Options.Select(ovs => ovs.OptionId).Distinct().ToList();
            if (optionIds.Any())
            {
                var optionsFromDb = await _repositoryManager.Options
                    .FindByCondition(o => optionIds.Contains(o.Id), trackChanges: false)
                    .ToDictionaryAsync(o => o.Id, o => o.Text, cancellationToken);

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
}