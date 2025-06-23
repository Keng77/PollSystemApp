using MediatR;
using PollSystemApp.Application.Common.Dto.UserDtos; 
using PollSystemApp.Application.Common.Interfaces;    
using PollSystemApp.Application.Common.Interfaces.Authentication; 
using PollSystemApp.Application.Common.Responses;
using PollSystemApp.Domain.Common.Exceptions; 
using PollSystemApp.Domain.Users;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options; 
using PollSystemApp.Application.Common.Settings;

namespace PollSystemApp.Application.UseCases.Auth.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ApiBaseResponse>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly JwtSettings _jwtSettings;

        public LoginUserCommandHandler(
            IRepositoryManager repositoryManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IOptions<JwtSettings> jwtOptions)
        {
            _repositoryManager = repositoryManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _jwtSettings = jwtOptions.Value;
        }

        public async Task<ApiBaseResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repositoryManager.Users.GetByUserNameAsync(request.UserNameOrEmail)
                       ?? await _repositoryManager.Users.GetByEmailAsync(request.UserNameOrEmail);

            if (user == null || !await _repositoryManager.Users.CheckPasswordAsync(user, request.Password))
            {
                throw new BadRequestException("Invalid username/email or password.");
            }

            if (!user.IsActive)
            {
                throw new BadRequestException("User account is inactive.");
            }

            var roles = await _repositoryManager.Users.GetRolesAsync(user);
            var accessToken = _jwtTokenGenerator.GenerateToken(user, roles);

            var refreshToken = Guid.NewGuid().ToString("N"); 
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes * 2); 

            var updateResult = await _repositoryManager.Users.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                throw new Exception($"User login succeeded but failed to save refresh token: {errors}");
            }

            var authResponse = new AuthResponseDto
            {
                Token = accessToken,
                RefreshToken = refreshToken, 
                Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
            };

            return new ApiOkResponse<AuthResponseDto>(authResponse);
        }
    }
}