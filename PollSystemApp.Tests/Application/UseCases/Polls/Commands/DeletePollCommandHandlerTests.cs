using FluentAssertions;
using MediatR;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.UseCases.Polls.Commands.DeletePoll;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Tests.Application.UseCases.Polls.Commands
{
    public class DeletePollCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly DeletePollCommandHandler _handler;
        private readonly Mock<IPollRepository> _pollRepositoryMock;

        public DeletePollCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _pollRepositoryMock = new Mock<IPollRepository>();
            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);

            _currentUserServiceMock = new Mock<ICurrentUserService>();

            _handler = new DeletePollCommandHandler(
                _repositoryManagerMock.Object,
                _currentUserServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldDeletePoll_WhenUserIsOwner()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var command = new DeletePollCommand(pollId);
            var existingPoll = new Poll { Id = pollId, CreatedBy = userId };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(existingPoll);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _pollRepositoryMock.Verify(r => r.Delete(existingPoll), Times.Once);
            _repositoryManagerMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldDeletePoll_WhenUserIsAdmin()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var command = new DeletePollCommand(pollId);
            var existingPoll = new Poll { Id = pollId, CreatedBy = ownerId };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(adminId);
            _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(true);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(existingPoll);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            _pollRepositoryMock.Verify(r => r.Delete(existingPoll), Times.Once);
            _repositoryManagerMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsNotOwnerOrAdmin()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var command = new DeletePollCommand(pollId);
            var existingPoll = new Poll { Id = pollId, CreatedBy = ownerId };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(otherUserId);
            _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(false);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(existingPoll);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenAccessException>();
        }
    }
}