using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.UseCases.Auth.Commands.ChangePassword;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Users;

namespace PollSystemApp.Tests.Application.UseCases.Auth.Commands
{
    public class ChangePasswordCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly ChangePasswordCommandHandler _handler;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public ChangePasswordCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            _userRepositoryMock = new Mock<IUserRepository>();
            _repositoryManagerMock.Setup(r => r.Users).Returns(_userRepositoryMock.Object);

            _handler = new ChangePasswordCommandHandler(
                _repositoryManagerMock.Object,
                _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnUnitValue_WhenPasswordIsChangedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new ChangePasswordCommand
            {
                CurrentPassword = "oldPassword123!",
                NewPassword = "newPassword456!",
                ConfirmNewPassword = "newPassword456!"
            };
            var user = new User { Id = userId };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
                               .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _userRepositoryMock.Verify(r => r.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationAppException_WhenCurrentPasswordIsIncorrect()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new ChangePasswordCommand { CurrentPassword = "wrongOldPassword", NewPassword = "newPassword456!" };
            var user = new User { Id = userId };
            var identityError = new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password." };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId, false)).ReturnsAsync(user);
            _userRepositoryMock.Setup(r => r.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
                               .ReturnsAsync(IdentityResult.Failed(identityError));

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationAppException>()
                     .Where(ex => ex.Errors.ContainsKey("CurrentPassword"));
        }

        [Fact]
        public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var command = new ChangePasswordCommand();
            _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenAccessException>();
        }
    }
}