using FluentValidation;
using PollSystemApp.Application.Common.Dto.UserDtos;

namespace PollSystemApp.Application.Common.Validation.UserDtos
{
    public class UserForLoginDtoValidator : AbstractValidator<UserForLoginDto>
    {
        public UserForLoginDtoValidator()
        {
            RuleFor(u => u.UserNameOrEmail)
                .NotEmpty().WithMessage("Username or Email is required.");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}