using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Mappings;
using PollSystemApp.Application.UseCases.Auth.Commands.RegisterUser;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Users;

namespace PollSystemApp.Tests.Application.UseCases.Auth.Commands
{
    public class RegisterUserCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly IMapper _mapper;
        private readonly RegisterUserCommandHandler _handler;
        private readonly Mock<IUserRepository> _userRepositoryMock;

        public RegisterUserCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _repositoryManagerMock.Setup(r => r.Users).Returns(_userRepositoryMock.Object);

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = mapperConfig.CreateMapper();

            _handler = new RegisterUserCommandHandler(_repositoryManagerMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnUnitValue_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var command = new RegisterUserCommand { UserName = "newuser", Email = "new@test.com", Password = "Password123!" };
            _userRepositoryMock.Setup(r => r.RegisterAsync(It.IsAny<User>(), command.Password))
                               .ReturnsAsync(IdentityResult.Success);
            _userRepositoryMock.Setup(r => r.AddRolesToUserAsync(It.IsAny<User>(), It.IsAny<ICollection<string>>()))
                               .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _userRepositoryMock.Verify(r => r.RegisterAsync(It.Is<User>(u => u.UserName == command.UserName), command.Password), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowValidationAppException_WhenRegistrationFails()
        {
            // Arrange
            var command = new RegisterUserCommand { UserName = "newuser", Email = "new@test.com", Password = "Password123!" };
            var errors = new List<IdentityError> { new IdentityError { Code = "DuplicateUserName", Description = "Username is already taken." } };
            _userRepositoryMock.Setup(r => r.RegisterAsync(It.IsAny<User>(), command.Password))
                               .ReturnsAsync(IdentityResult.Failed(errors.ToArray()));

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ValidationAppException>()
                     .Where(ex => ex.Errors.ContainsKey("UserName") && ex.Errors["UserName"].Contains("Username is already taken."));
        }
    }
}