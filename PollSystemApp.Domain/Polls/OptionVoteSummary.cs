namespace PollSystemApp.Domain.Polls
{
    public class OptionVoteSummary
    {
        public Guid OptionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Votes { get; set; }
        public double Percentage { get; set; }

    }
}