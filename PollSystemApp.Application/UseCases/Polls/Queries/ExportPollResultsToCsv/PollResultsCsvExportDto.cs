namespace PollSystemApp.Application.UseCases.Polls.Queries.ExportPollResultsToCsv
{
    public class PollResultsCsvExportDto
    {
        public byte[] FileContents { get; set; } = [];
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "text/csv";
    }
}