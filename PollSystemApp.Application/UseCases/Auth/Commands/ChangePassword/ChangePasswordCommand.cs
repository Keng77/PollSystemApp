using MediatR;

namespace PollSystemApp.Application.UseCases.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommand : IRequest<Unit>
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}