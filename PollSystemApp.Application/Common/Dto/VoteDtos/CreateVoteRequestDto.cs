namespace PollSystemApp.Application.Common.Dto.VoteDtos
{
    public class CreateVoteRequestDto
    {
        public List<Guid> OptionIds { get; set; } = new List<Guid>();
    }
}