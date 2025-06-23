using MediatR;
using PollSystemApp.Application.Common.Dto.UserDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Interfaces.Authentication;
using PollSystemApp.Application.Common.Responses;
using PollSystemApp.Application.Common.Settings;
using PollSystemApp.Domain.Common.Exceptions;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace PollSystemApp.Application.UseCases.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiBaseResponse>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ITokenValidator _tokenValidator;
        private readonly JwtSettings _jwtSettings;

        public RefreshTokenCommandHandler(
            IRepositoryManager repositoryManager,
            IJwtTokenGenerator jwtTokenGenerator,
            ITokenValidator tokenValidator,
            IOptions<JwtSettings> jwtOptions)
        {
            _repositoryManager = repositoryManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _tokenValidator = tokenValidator;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<ApiBaseResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _tokenValidator.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal?.Identity?.Name == null) 
            {
                throw new BadRequestException("Invalid access token or refresh token.");
            }

            var userIdString = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out var userId))
            {
                throw new BadRequestException("Invalid access token: User ID not found or invalid.");
            }

            var user = await _repositoryManager.Users.GetByIdAsync(userId, trackChanges: true); 

            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {              
                throw new BadRequestException("Invalid refresh token or refresh token expired.");
            }


            var roles = await _repositoryManager.Users.GetRolesAsync(user);
            var newAccessToken = _jwtTokenGenerator.GenerateToken(user, roles);
            var newRefreshToken = Guid.NewGuid().ToString("N");

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes * 2);

            var updateResult = await _repositoryManager.Users.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new Exception("Failed to update user's refresh token."); 
            }

            var authResponse = new AuthResponseDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
            };

            return new ApiOkResponse<AuthResponseDto>(authResponse);
        }
    }
}