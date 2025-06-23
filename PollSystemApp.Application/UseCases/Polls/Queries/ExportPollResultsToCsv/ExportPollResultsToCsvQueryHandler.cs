using MediatR;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace PollSystemApp.Application.UseCases.Polls.Queries.ExportPollResultsToCsv
{
    public class ExportPollResultsToCsvQueryHandler : IRequestHandler<ExportPollResultsToCsvQuery, PollResultsCsvExportDto>
    {
        private readonly IRepositoryManager _repositoryManager;

        public ExportPollResultsToCsvQueryHandler(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        public async Task<PollResultsCsvExportDto> Handle(ExportPollResultsToCsvQuery request, CancellationToken cancellationToken)
        {
            var poll = await _repositoryManager.Polls.GetByIdAsync(request.PollId, trackChanges: false);
            if (poll == null)
            {
                throw new NotFoundException(nameof(Poll), request.PollId);
            }

            if (DateTime.UtcNow <= poll.EndDate && !(poll.EndDate == poll.StartDate))
            {
                throw new BadRequestException($"Results for this poll (and CSV export) will be available after {poll.EndDate:o}.");
            }

            PollResult? pollResult = await _repositoryManager.PollResults
                .FindByCondition(pr => pr.PollId == request.PollId, trackChanges: false)
                .Include(pr => pr.Options)
                .OrderByDescending(pr => pr.CalculatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var optionsForPoll = await _repositoryManager.Options
                    .FindByCondition(o => o.PollId == request.PollId, trackChanges: false)
                    .OrderBy(o => o.Order)
                    .ToListAsync(cancellationToken);


            bool recalculate = false;
            if (pollResult == null)
            {
                recalculate = true;
            }
            else if (poll.EndDate > pollResult.CalculatedAt && !(poll.EndDate == poll.StartDate))
            {
                recalculate = true;
            }


            if (recalculate)
            {
                var votes = await _repositoryManager.Votes
                    .FindByCondition(v => v.PollId == request.PollId, trackChanges: false)
                    .ToListAsync(cancellationToken);

                int totalVotesInPoll = 0;
                if (poll.IsAnonymous)
                {
                    totalVotesInPoll = votes.Count;
                }
                else
                {
                    totalVotesInPoll = votes.GroupBy(v => v.UserId)
                                            .Count(g => g.Key.HasValue && g.Key.Value != Guid.Empty);
                }

                pollResult = new PollResult
                {
                    PollId = poll.Id,
                    CalculatedAt = DateTime.UtcNow, 
                    TotalVotes = totalVotesInPoll,
                    Options = new List<OptionVoteSummary>()
                };

                foreach (var option in optionsForPoll)
                {
                    var votesForOptionCount = votes.Count(v => v.OptionId == option.Id);
                    double percentage = (totalVotesInPoll > 0) ? Math.Round(((double)votesForOptionCount / totalVotesInPoll) * 100, 2) : 0;
                    pollResult.Options.Add(new OptionVoteSummary
                    {
                        OptionId = option.Id,
                        Votes = votesForOptionCount,
                        Percentage = percentage
                    });
                }
            }

            if (pollResult == null || !pollResult.Options.Any())
            {
                if (!optionsForPoll.Any())
                {
                    throw new BadRequestException("Poll has no options to generate CSV from.");
                }
            }


            var sb = new StringBuilder();

            sb.AppendLine("PollTitle,CalculatedAt,TotalVotesInPoll");
            sb.AppendLine($"{EscapeCsvField(poll.Title)},{pollResult.CalculatedAt:o},{pollResult.TotalVotes}");
            sb.AppendLine();

            sb.AppendLine("OptionText,Votes,Percentage");

            var optionTexts = optionsForPoll.ToDictionary(o => o.Id, o => o.Text);

            foreach (var summary in pollResult.Options.OrderBy(s => optionsForPoll.FirstOrDefault(opt => opt.Id == s.OptionId)?.Order ?? int.MaxValue))
            {
                string optionText = optionTexts.TryGetValue(summary.OptionId, out var text) ? text : "Unknown Option";
                sb.AppendLine($"{EscapeCsvField(optionText)},{summary.Votes},{summary.Percentage.ToString(CultureInfo.InvariantCulture)}");
            }

            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());

            string safeTitle = new string(poll.Title.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray()).Replace(" ", "_");
            if (string.IsNullOrWhiteSpace(safeTitle)) safeTitle = "PollResults";
            string fileName = $"{safeTitle}_{poll.Id.ToString("N").Substring(0, 8)}_{DateTime.UtcNow:yyyyMMdd}.csv";

            return new PollResultsCsvExportDto
            {
                FileContents = csvBytes,
                FileName = fileName,
                ContentType = "text/csv"
            };
        }

        private string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}