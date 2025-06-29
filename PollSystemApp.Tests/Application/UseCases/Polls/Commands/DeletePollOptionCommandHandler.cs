using FluentAssertions;
using MediatR;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.UseCases.Polls.Commands.DeletePollOption;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using System.Linq.Expressions;


namespace PollSystemApp.Tests.Application.UseCases.Polls.Commands
{
    public class DeletePollOptionCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly DeletePollOptionCommandHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;
        private readonly Mock<IOptionRepository> _optionRepositoryMock;
        private readonly Mock<IVoteRepository> _voteRepositoryMock;
        private readonly Mock<IOptionVoteSummaryRepository> _optionVoteSummaryRepositoryMock;

        public DeletePollOptionCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _pollRepositoryMock = new Mock<IPollRepository>();
            _optionRepositoryMock = new Mock<IOptionRepository>();
            _voteRepositoryMock = new Mock<IVoteRepository>();
            _optionVoteSummaryRepositoryMock = new Mock<IOptionVoteSummaryRepository>();

            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Options).Returns(_optionRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Votes).Returns(_voteRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.OptionVoteSummaries).Returns(_optionVoteSummaryRepositoryMock.Object);

            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _handler = new DeletePollOptionCommandHandler(_repositoryManagerMock.Object, _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldDeleteOption_WhenRequestIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var optionIdToDelete = Guid.NewGuid();
            var command = new DeletePollOptionCommand(pollId, optionIdToDelete);

            var poll = new Poll { Id = pollId, CreatedBy = userId };
            var option = new Option { Id = optionIdToDelete, PollId = pollId };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _optionRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Option, bool>>>(), false, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(option);
            _optionRepositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Option, bool>>>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(2);
            _voteRepositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Vote, bool>>>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(0);

            _optionVoteSummaryRepositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<OptionVoteSummary, bool>>>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            _optionRepositoryMock.Verify(r => r.Delete(option), Times.Once);
            _repositoryManagerMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenLessThanTwoOptionsWouldRemain()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var optionIdToDelete = Guid.NewGuid();
            var command = new DeletePollOptionCommand(pollId, optionIdToDelete);

            var poll = new Poll { Id = pollId, CreatedBy = userId };
            var option = new Option { Id = optionIdToDelete, PollId = pollId };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _optionRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Option, bool>>>(), false, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(option);
            _optionRepositoryMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Option, bool>>>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(1);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                     .WithMessage("A poll must have at least two options. Cannot delete this option.");
        }
    }
}