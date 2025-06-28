using MediatR;
using PollSystemApp.Application.Common.Dto.UserDtos;

namespace PollSystemApp.Application.UseCases.Auth.Commands.LoginUser;
public class LoginUserCommand : UserForLoginDto, IRequest<AuthResponseDto>
{
}