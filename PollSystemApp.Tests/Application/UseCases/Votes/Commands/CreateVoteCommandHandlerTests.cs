using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.UseCases.Votes.Commands.CreateVote;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using System.Linq.Expressions;

namespace PollSystemApp.Tests.Application.UseCases.Votes.Commands
{
    public class CreateVoteCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly CreateVoteCommandHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;
        private readonly Mock<IOptionRepository> _optionRepositoryMock;
        private readonly Mock<IVoteRepository> _voteRepositoryMock;

        public CreateVoteCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            _pollRepositoryMock = new Mock<IPollRepository>();
            _optionRepositoryMock = new Mock<IOptionRepository>();
            _voteRepositoryMock = new Mock<IVoteRepository>();

            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Options).Returns(_optionRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Votes).Returns(_voteRepositoryMock.Object);

            _handler = new CreateVoteCommandHandler(
                _repositoryManagerMock.Object,
                _currentUserServiceMock.Object,
                _httpContextAccessorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateVote_WhenRequestIsValidForNonAnonymousPoll()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var optionId = Guid.NewGuid();
            var command = new CreateVoteCommand { PollId = pollId, OptionIds = new List<Guid> { optionId } };

            var poll = new Poll { Id = pollId, IsAnonymous = false, IsMultipleChoice = false, StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1) };
            var options = new List<Option> { new Option { Id = optionId, PollId = pollId } };

            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _optionRepositoryMock.Setup(r => r.GetOptionsByIdsAndPollIdAsync(pollId, It.IsAny<IEnumerable<Guid>>(), false, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(options);
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _voteRepositoryMock.Setup(r => r.ExistsAsync(v => v.PollId == pollId && v.UserId == userId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(false); 

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _voteRepositoryMock.Verify(r => r.CreateRangeAsync(It.Is<List<Vote>>(votes => votes.Count == 1 && votes[0].UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
            _repositoryManagerMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenUserHasAlreadyVoted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var optionId = Guid.NewGuid();
            var command = new CreateVoteCommand { PollId = pollId, OptionIds = new List<Guid> { optionId } };

            var poll = new Poll { Id = pollId, IsAnonymous = false, StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1) };
            var options = new List<Option> { new Option { Id = optionId, PollId = pollId } };

            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _optionRepositoryMock.Setup(r => r.GetOptionsByIdsAndPollIdAsync(pollId, It.IsAny<IEnumerable<Guid>>(), false, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(options);
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _voteRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Vote, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("You have already voted in this poll.");
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenPollIsNotActive()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var command = new CreateVoteCommand { PollId = pollId, OptionIds = new List<Guid> { Guid.NewGuid() } };
            var poll = new Poll { Id = pollId, StartDate = DateTime.UtcNow.AddDays(-2), EndDate = DateTime.UtcNow.AddDays(-1) };

            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }
    }
}