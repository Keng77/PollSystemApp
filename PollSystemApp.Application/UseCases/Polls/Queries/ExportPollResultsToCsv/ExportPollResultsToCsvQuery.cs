using MediatR;

namespace PollSystemApp.Application.UseCases.Polls.Queries.ExportPollResultsToCsv
{
    public class ExportPollResultsToCsvQuery : IRequest<PollResultsCsvExportDto>
    {
        public Guid PollId { get; set; }
    }
}