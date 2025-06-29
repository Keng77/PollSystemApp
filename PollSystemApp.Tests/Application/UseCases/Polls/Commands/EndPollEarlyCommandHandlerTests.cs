using FluentAssertions;
using MediatR;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.UseCases.Polls.Commands.EndPollEarly;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Tests.Application.UseCases.Polls.Commands
{
    public class EndPollEarlyCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly EndPollEarlyCommandHandler _handler;
        private readonly Mock<IPollRepository> _pollRepositoryMock;

        public EndPollEarlyCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _pollRepositoryMock = new Mock<IPollRepository>();
            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);

            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _handler = new EndPollEarlyCommandHandler(_repositoryManagerMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldSetEndDateToNow_WhenPollIsActiveAndUserIsAdmin()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var command = new EndPollEarlyCommand(pollId);
            var poll = new Poll { Id = pollId, EndDate = DateTime.UtcNow.AddDays(1) };

            _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(true);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, true)).ReturnsAsync(poll);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            poll.EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            _repositoryManagerMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsNotAdmin()
        {
            // Arrange
            var command = new EndPollEarlyCommand(Guid.NewGuid());
            _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(false);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenAccessException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenPollHasAlreadyEnded()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var command = new EndPollEarlyCommand(pollId);
            var poll = new Poll { Id = pollId, EndDate = DateTime.UtcNow.AddDays(-1) };

            _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(true);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, true)).ReturnsAsync(poll);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("This poll has already ended.");
        }
    }
}