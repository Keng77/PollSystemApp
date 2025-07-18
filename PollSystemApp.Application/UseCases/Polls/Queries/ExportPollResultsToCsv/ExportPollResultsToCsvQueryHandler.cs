using MediatR;
using Microsoft.AspNetCore.Http;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using System.Globalization;
using System.Text;

namespace PollSystemApp.Application.UseCases.Polls.Queries.ExportPollResultsToCsv;

public class ExportPollResultsToCsvQueryHandler : IRequestHandler<ExportPollResultsToCsvQuery, PollResultsCsvExportDto>
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IPollResultsCalculator _resultsCalculator;

    public ExportPollResultsToCsvQueryHandler(IRepositoryManager repositoryManager, IPollResultsCalculator resultsCalculator)
    {
        _repositoryManager = repositoryManager;
        _resultsCalculator = resultsCalculator;
    }

    public async Task<PollResultsCsvExportDto> Handle(ExportPollResultsToCsvQuery request, CancellationToken cancellationToken)
    {
        var poll = await _repositoryManager.Polls.GetByIdAsync(request.PollId, false)
            ?? throw new NotFoundException(nameof(Poll), request.PollId);

        if (DateTime.UtcNow <= poll.EndDate && !(poll.EndDate == poll.StartDate))
        {
            throw new BadRequestException($"Results for this poll (and CSV export) will be available after {poll.EndDate:o}.");
        }

        PollResult? pollResult = await _repositoryManager.PollResults.GetLatestPollResultAsync(request.PollId, false, cancellationToken);

        bool shouldRecalculate = pollResult == null || (poll.EndDate > pollResult.CalculatedAt && poll.EndDate != poll.StartDate);

        if (shouldRecalculate)
        {
            pollResult = await _resultsCalculator.CalculateResultsAsync(poll, cancellationToken);
        }

        var optionsForPoll = await _repositoryManager.Options.GetOptionsByPollIdAsync(poll.Id, false, cancellationToken);

        if (pollResult == null || !pollResult.Options.Any())
        {
            if (!optionsForPoll.Any())
            {
                throw new BadRequestException("Poll has no options to generate CSV from.");
            }
            pollResult ??= new PollResult { CalculatedAt = DateTime.UtcNow, TotalVotes = 0, Options = new List<OptionVoteSummary>() };
        }

        var sb = new StringBuilder();

        sb.AppendLine("PollTitle,CalculatedAt,TotalVotesInPoll");
        sb.AppendLine($"{EscapeCsvField(poll.Title)},{pollResult.CalculatedAt:o},{pollResult.TotalVotes}");
        sb.AppendLine();
        sb.AppendLine("OptionText,Votes,Percentage");

        var optionTexts = optionsForPoll.ToDictionary(o => o.Id, o => o.Text);

        var sortedSummaries = pollResult.Options
            .OrderBy(s => optionsForPoll.FirstOrDefault(opt => opt.Id == s.OptionId)?.Order ?? int.MaxValue);

        foreach (var summary in sortedSummaries)
        {
            string optionText = optionTexts.TryGetValue(summary.OptionId, out var text) ? text : "Unknown Option";
            sb.AppendLine($"{EscapeCsvField(optionText)},{summary.Votes},{summary.Percentage.ToString(CultureInfo.InvariantCulture)}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());

        string safeTitle = new string(poll.Title.Where(c => !Path.GetInvalidFileNameChars().Contains(c)).ToArray()).Replace(" ", "_");
        if (string.IsNullOrWhiteSpace(safeTitle)) safeTitle = "PollResults";
        string fileName = $"{safeTitle}_{poll.Id.ToString("N").Substring(0, 8)}_{DateTime.UtcNow:yyyyMMdd}.csv";

        if (csvBytes == null || csvBytes.Length == 0)
        {
            throw new NotFoundException("No data to export or error generating CSV.");
        }

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