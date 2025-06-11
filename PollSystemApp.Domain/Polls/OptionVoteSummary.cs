namespace PollSystemApp.Domain.Polls
{
    public class OptionVoteSummary
    {
        public Guid Id { get; set; }
        public Guid OptionId { get; set; }
        public int Votes { get; set; }
        public double Percentage { get; set; }

    }
}