namespace PollSystemApp.Domain.Polls
{
    public class PollResult
    {
        public Guid Id { get; set; }
        public Guid PollId { get; set; }
        public int TotalVotes { get; set; }
        public List<OptionVoteSummary> Options { get; set; } = [];
        public DateTime CalculatedAt { get; set; }

    }
}