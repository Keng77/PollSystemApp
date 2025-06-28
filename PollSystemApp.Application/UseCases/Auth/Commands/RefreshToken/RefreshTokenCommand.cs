using MediatR;
using PollSystemApp.Application.Common.Dto.UserDtos;

namespace PollSystemApp.Application.UseCases.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : RefreshTokenRequestDto, IRequest<AuthResponseDto>
    {
    }
}