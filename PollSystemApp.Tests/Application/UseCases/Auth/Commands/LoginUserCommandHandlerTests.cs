using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using PollSystemApp.Application.Common.Dto.UserDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Interfaces.Authentication;
using PollSystemApp.Application.Common.Settings;
using PollSystemApp.Application.UseCases.Auth.Commands.LoginUser;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Users;

namespace PollSystemApp.Tests.Application.UseCases.Auth.Commands
{
    public class LoginUserCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly IOptions<JwtSettings> _jwtSettingsOptions;
        private readonly LoginUserCommandHandler _handler;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public LoginUserCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _repositoryManagerMock.Setup(r => r.Users).Returns(_userRepositoryMock.Object);

            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();

            var jwtSettings = new JwtSettings { ExpiryMinutes = 15 };
            _jwtSettingsOptions = Options.Create(jwtSettings);

            _handler = new LoginUserCommandHandler(
                _repositoryManagerMock.Object,
                _jwtTokenGeneratorMock.Object,
                _jwtSettingsOptions);
        }

        [Fact]
        public async Task Handle_ShouldReturnAuthResponseDto_WhenCredentialsAreValid()
        {
            // Arrange
            var command = new LoginUserCommand { UserNameOrEmail = "testuser", Password = "Password123!" };
            var user = new User { UserName = "testuser", Email = "test@test.com", IsActive = true };

            _userRepositoryMock.Setup(r => r.GetByUserNameAsync(command.UserNameOrEmail)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.CheckPasswordAsync(user, command.Password)).ReturnsAsync(true);
            _userRepositoryMock.Setup(r => r.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            _userRepositoryMock.Setup(r => r.UpdateAsync(user)).ReturnsAsync(Microsoft.AspNetCore.Identity.IdentityResult.Success);
            _jwtTokenGeneratorMock.Setup(g => g.GenerateToken(It.IsAny<User>(), It.IsAny<IList<string>>(), null)).Returns("valid_jwt_token");

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeOfType<AuthResponseDto>();
            result.Token.Should().Be("valid_jwt_token");
            user.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenCredentialsAreInvalid()
        {
            // Arrange
            var command = new LoginUserCommand { UserNameOrEmail = "testuser", Password = "WrongPassword" };
            _userRepositoryMock.Setup(r => r.GetByUserNameAsync(command.UserNameOrEmail)).ReturnsAsync((User)null);
            _userRepositoryMock.Setup(r => r.GetByEmailAsync(command.UserNameOrEmail)).ReturnsAsync((User)null);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Invalid username/email or password.");
        }
    }
}