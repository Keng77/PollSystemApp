using FluentAssertions;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.UseCases.Polls.Queries.ExportPollResultsToCsv;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Tests.Application.UseCases.Polls.Queries
{
    public class ExportPollResultsToCsvQueryHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly Mock<IPollResultsCalculator> _resultsCalculatorMock;
        private readonly ExportPollResultsToCsvQueryHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;
        private readonly Mock<IPollResultRepository> _pollResultRepositoryMock;
        private readonly Mock<IOptionRepository> _optionRepositoryMock;

        public ExportPollResultsToCsvQueryHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _resultsCalculatorMock = new Mock<IPollResultsCalculator>();

            _pollRepositoryMock = new Mock<IPollRepository>();
            _pollResultRepositoryMock = new Mock<IPollResultRepository>();
            _optionRepositoryMock = new Mock<IOptionRepository>();

            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.PollResults).Returns(_pollResultRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Options).Returns(_optionRepositoryMock.Object);

            _handler = new ExportPollResultsToCsvQueryHandler(_repositoryManagerMock.Object, _resultsCalculatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnCsvFile_WithCalculatedResults()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var query = new ExportPollResultsToCsvQuery { PollId = pollId };

            var poll = new Poll { Id = pollId, Title = "CSV Test Poll", EndDate = DateTime.UtcNow.AddDays(-1) };
            var options = new List<Option>
            {
                new Option { Id = Guid.NewGuid(), PollId = pollId, Text = "CSV Option 1", Order = 1 }
            };
            var calculatedResult = new PollResult
            {
                PollId = pollId,
                TotalVotes = 10,
                Options = new List<OptionVoteSummary>
                {
                    new OptionVoteSummary { OptionId = options[0].Id, Votes = 10, Percentage = 100 }
                }
            };

            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _pollResultRepositoryMock.Setup(r => r.GetLatestPollResultAsync(pollId, false, It.IsAny<CancellationToken>()))
                                     .ReturnsAsync((PollResult?)null);
            _resultsCalculatorMock.Setup(c => c.CalculateResultsAsync(poll, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(calculatedResult);
            _optionRepositoryMock.Setup(r => r.GetOptionsByPollIdAsync(pollId, false, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(options);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.FileContents.Should().NotBeEmpty();
            result.ContentType.Should().Be("text/csv");
            result.FileName.Should().Contain("CSV_Test_Poll");
            result.FileName.Should().EndWith(".csv");

            _resultsCalculatorMock.Verify(c => c.CalculateResultsAsync(poll, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenPollIsStillActive()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var query = new ExportPollResultsToCsvQuery { PollId = pollId };
            var poll = new Poll { Id = pollId, Title = "Active Poll", EndDate = DateTime.UtcNow.AddDays(1) };

            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);

            // Act
            Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }
    }
}