namespace PollSystemApp.Domain.Polls
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Poll> Polls { get; set; } = [];
    }
}
