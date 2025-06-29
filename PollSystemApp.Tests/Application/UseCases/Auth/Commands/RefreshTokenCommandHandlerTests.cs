using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using PollSystemApp.Application.Common.Dto.UserDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Interfaces.Authentication;
using PollSystemApp.Application.Common.Settings;
using PollSystemApp.Application.UseCases.Auth.Commands.RefreshToken;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Users;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PollSystemApp.Tests.Application.UseCases.Auth.Commands
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly Mock<ITokenValidator> _tokenValidatorMock;
        private readonly IOptions<JwtSettings> _jwtSettingsOptions;
        private readonly RefreshTokenCommandHandler _handler;

        private readonly Mock<IUserRepository> _userRepositoryMock;

        public RefreshTokenCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _repositoryManagerMock.Setup(r => r.Users).Returns(_userRepositoryMock.Object);

            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
            _tokenValidatorMock = new Mock<ITokenValidator>();

            var jwtSettings = new JwtSettings { ExpiryMinutes = 15 };
            _jwtSettingsOptions = Options.Create(jwtSettings);

            _handler = new RefreshTokenCommandHandler(
                _repositoryManagerMock.Object,
                _jwtTokenGeneratorMock.Object,
                _tokenValidatorMock.Object,
                _jwtSettingsOptions);
        }

        [Fact]
        public async Task Handle_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userName = "testuser";
            var command = new RefreshTokenCommand { AccessToken = "expired_token", RefreshToken = "valid_refresh_token" };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userName)
            };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));

            var user = new User { Id = userId, UserName = userName, RefreshToken = "valid_refresh_token", RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1) };

            _tokenValidatorMock.Setup(v => v.GetPrincipalFromExpiredToken(command.AccessToken))
                               .Returns(claimsPrincipal);

            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, true))
                               .ReturnsAsync(user);

            _userRepositoryMock.Setup(r => r.GetRolesAsync(user))
                               .ReturnsAsync(new List<string> { "User" });

            _userRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<User>()))
                               .ReturnsAsync(IdentityResult.Success);

            _jwtTokenGeneratorMock.Setup(g => g.GenerateToken(user, It.IsAny<IList<string>>(), null))
                                  .Returns("new_access_token");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().Be("new_access_token");
            result.RefreshToken.Should().NotBe("valid_refresh_token"); 

            _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Id == userId)), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenRefreshTokenIsInvalid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new RefreshTokenCommand { AccessToken = "expired_token", RefreshToken = "invalid_refresh_token" };
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }));
            var user = new User { Id = userId, RefreshToken = "different_valid_token", RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1) };

            _tokenValidatorMock.Setup(v => v.GetPrincipalFromExpiredToken(command.AccessToken)).Returns(claimsPrincipal);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, true)).ReturnsAsync(user);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }
    }
}