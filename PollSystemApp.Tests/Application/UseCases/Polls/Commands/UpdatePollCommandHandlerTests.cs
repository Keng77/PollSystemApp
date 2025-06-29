using AutoMapper;
using FluentAssertions;
using Moq;
using PollSystemApp.Application.Common.Dto.PollDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Mappings;
using PollSystemApp.Application.UseCases.Polls.Commands.UpdatePoll;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Tests.Application.UseCases.Polls.Commands
{
    public class UpdatePollCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly UpdatePollCommandHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;

        public UpdatePollCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _pollRepositoryMock = new Mock<IPollRepository>();
            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);

            _currentUserServiceMock = new Mock<ICurrentUserService>();

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = mapperConfig.CreateMapper();

            _handler = new UpdatePollCommandHandler(
                _repositoryManagerMock.Object,
                _mapper,
                _currentUserServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldUpdatePoll_WhenUserIsOwner()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var command = new UpdatePollCommand
            {
                Id = pollId,
                PollData = new PollForUpdateDto { Title = "Updated Title" }
            };
            var existingPoll = new Poll { Id = pollId, CreatedBy = userId, Title = "Old Title" };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, true)).ReturnsAsync(existingPoll);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            existingPoll.Title.Should().Be("Updated Title");
            _repositoryManagerMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsNotOwnerOrAdmin()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var command = new UpdatePollCommand { Id = pollId, PollData = new PollForUpdateDto() };
            var existingPoll = new Poll { Id = pollId, CreatedBy = ownerId };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(otherUserId);
            _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(false);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, true)).ReturnsAsync(existingPoll);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenAccessException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenPollDoesNotExist()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var command = new UpdatePollCommand { Id = pollId, PollData = new PollForUpdateDto() };

            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, true)).ReturnsAsync((Poll)null);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}