using MediatR;
using PollSystemApp.Application.Common.Dto.UserDtos;
using PollSystemApp.Application.Common.Responses;    

namespace PollSystemApp.Application.UseCases.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : RefreshTokenRequestDto, IRequest<ApiBaseResponse>
    {
    }
}