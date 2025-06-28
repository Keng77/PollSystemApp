namespace PollSystemApp.Application.Common.Dto.PollResultDtos
{
    public class OptionVoteSummaryDto
    {
        public Guid OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public int Votes { get; set; }
        public double Percentage { get; set; }
    }
}