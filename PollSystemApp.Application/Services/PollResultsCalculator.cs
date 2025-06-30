using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PollSystemApp.Application.Services
{
    public class PollResultsCalculator : IPollResultsCalculator
    {
        private readonly IRepositoryManager _repositoryManager;

        public PollResultsCalculator(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<PollResult> CalculateResultsAsync(Poll poll, CancellationToken cancellationToken)
        {
            var options = await _repositoryManager.Options.GetOptionsByPollIdAsync(poll.Id, false, cancellationToken);
            if (!options.Any())
            {
                return new PollResult
                {
                    PollId = poll.Id,
                    TotalVotes = 0,
                    Options = new List<OptionVoteSummary>(),
                    CalculatedAt = DateTime.UtcNow
                };
            }

            var votes = await _repositoryManager.Votes.GetVotesByPollIdAsync(poll.Id, false, cancellationToken);

            int totalVotesInPoll;
            if (poll.IsMultipleChoice || poll.IsAnonymous)
            {
                totalVotesInPoll = votes.Count;
            }
            else 
            {
                totalVotesInPoll = votes.GroupBy(v => v.UserId)
                                        .Count(g => g.Key.HasValue && g.Key.Value != Guid.Empty);
            }

            var newPollResult = new PollResult
            {
                Id = Guid.NewGuid(),
                PollId = poll.Id,
                CalculatedAt = DateTime.UtcNow,
                TotalVotes = totalVotesInPoll,
                Options = new List<OptionVoteSummary>()
            };

            foreach (var option in options)
            {
                var votesForOption = votes.Count(v => v.OptionId == option.Id);
                double percentage = (totalVotesInPoll > 0) ? Math.Round((double)votesForOption / totalVotesInPoll * 100, 2) : 0;

                var summary = new OptionVoteSummary
                {
                    Id = Guid.NewGuid(),
                    OptionId = option.Id,
                    Votes = votesForOption,
                    Percentage = percentage,
                };
                newPollResult.Options.Add(summary);
            }

            return newPollResult;
        }
    }
}