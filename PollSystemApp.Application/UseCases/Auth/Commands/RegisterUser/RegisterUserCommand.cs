using MediatR;
using PollSystemApp.Application.Common.Responses;
using PollSystemApp.Application.Common.Dto.UserDtos;

namespace PollSystemApp.Application.UseCases.Auth.Commands.RegisterUser;
public class RegisterUserCommand : UserForRegistrationDto, IRequest<ApiBaseResponse>
{
}