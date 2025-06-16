namespace PollSystemApp.Application.Common.Dto.UserDtos
{
    public class UserForLoginDto
    {
        public string UserNameOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}