using FluentAssertions;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.UseCases.Votes.Queries.CheckUserVote;
using PollSystemApp.Domain.Polls;


namespace PollSystemApp.Tests.Application.UseCases.Votes.Queries
{
    public class CheckUserVoteQueryHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly CheckUserVoteQueryHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;
        private readonly Mock<IVoteRepository> _voteRepositoryMock;

        public CheckUserVoteQueryHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            _pollRepositoryMock = new Mock<IPollRepository>();
            _voteRepositoryMock = new Mock<IVoteRepository>();

            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Votes).Returns(_voteRepositoryMock.Object);

            _handler = new CheckUserVoteQueryHandler(_repositoryManagerMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnHasVotedTrue_WhenUserHasVotedInNonAnonymousPoll()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var votedOptionId = Guid.NewGuid();
            var query = new CheckUserVoteQuery { PollId = pollId };

            var poll = new Poll { Id = pollId, IsAnonymous = false };

            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _voteRepositoryMock.Setup(r => r.GetUserVoteOptionIdsAsync(pollId, userId, false, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new List<Guid> { votedOptionId });

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.HasVoted.Should().BeTrue();
            result.VotedOptionIds.Should().Contain(votedOptionId);
        }

        [Fact]
        public async Task Handle_ShouldReturnHasVotedFalse_WhenUserHasNotVoted()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var query = new CheckUserVoteQuery { PollId = pollId };

            var poll = new Poll { Id = pollId, IsAnonymous = false };

            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _voteRepositoryMock.Setup(r => r.GetUserVoteOptionIdsAsync(pollId, userId, false, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new List<Guid>());

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.HasVoted.Should().BeFalse();
            result.VotedOptionIds.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ShouldReturnHasVotedFalse_ForAnonymousPoll()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var query = new CheckUserVoteQuery { PollId = pollId };
            var poll = new Poll { Id = pollId, IsAnonymous = true };

            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.HasVoted.Should().BeFalse();
        }
    }
}