namespace PollSystemApp.Application.Common.Dto.PollResultDtos
{
    public class PollResultDto
    {
        public Guid PollId { get; set; }
        public string PollTitle { get; set; } = string.Empty;
        public int TotalVotes { get; set; }
        public List<OptionVoteSummaryDto> Options { get; set; } = new List<OptionVoteSummaryDto>();
        public DateTime CalculatedAt { get; set; }
    }
}