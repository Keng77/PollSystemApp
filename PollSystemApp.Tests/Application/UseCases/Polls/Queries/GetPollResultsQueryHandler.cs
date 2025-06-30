using AutoMapper;
using FluentAssertions;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Mappings;
using PollSystemApp.Application.UseCases.Polls.Queries.GetPollResults;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Tests.Application.UseCases.Polls.Queries
{
    public class GetPollResultsQueryHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly IMapper _mapper;
        private readonly Mock<IPollResultsCalculator> _resultsCalculatorMock;
        private readonly GetPollResultsQueryHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;
        private readonly Mock<IPollResultRepository> _pollResultRepositoryMock;
        private readonly Mock<IOptionRepository> _optionRepositoryMock;

        public GetPollResultsQueryHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
            _resultsCalculatorMock = new Mock<IPollResultsCalculator>();

            _pollRepositoryMock = new Mock<IPollRepository>();
            _pollResultRepositoryMock = new Mock<IPollResultRepository>();
            _optionRepositoryMock = new Mock<IOptionRepository>();

            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.PollResults).Returns(_pollResultRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Options).Returns(_optionRepositoryMock.Object);

            _handler = new GetPollResultsQueryHandler(_repositoryManagerMock.Object, _mapper, _resultsCalculatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnExistingResult_WhenItIsFresh()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var query = new GetPollResultsQuery { PollId = pollId };
            var poll = new Poll { Id = pollId, Title = "Test", EndDate = DateTime.UtcNow.AddDays(-2) };
            var existingResult = new PollResult { PollId = pollId, CalculatedAt = DateTime.UtcNow.AddDays(-1), Options = new List<OptionVoteSummary>() };

            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _pollResultRepositoryMock.Setup(r => r.GetLatestPollResultAsync(pollId, false, It.IsAny<CancellationToken>()))
                                     .ReturnsAsync(existingResult);
            _optionRepositoryMock.Setup(r => r.GetOptionTextsByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new Dictionary<Guid, string>());

            // Act
            var resultDto = await _handler.Handle(query, CancellationToken.None);

            // Assert
            resultDto.Should().NotBeNull();
            _resultsCalculatorMock.Verify(c => c.CalculateResultsAsync(It.IsAny<Poll>(), It.IsAny<CancellationToken>()), Times.Never);
            _pollResultRepositoryMock.Verify(r => r.Create(It.IsAny<PollResult>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldCalculateNewResult_WhenNoExistingResult()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var query = new GetPollResultsQuery { PollId = pollId };

            var poll = new Poll { Id = pollId, Title = "Test", EndDate = DateTime.UtcNow.AddDays(-1) };
            var calculatedResult = new PollResult { PollId = pollId, Options = new List<OptionVoteSummary>() };

            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _pollResultRepositoryMock.Setup(r => r.GetLatestPollResultAsync(pollId, false, It.IsAny<CancellationToken>()))
                                     .ReturnsAsync((PollResult?)null);
            _resultsCalculatorMock.Setup(c => c.CalculateResultsAsync(poll, It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(calculatedResult);
            _optionRepositoryMock.Setup(r => r.GetOptionTextsByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new Dictionary<Guid, string>());

            // Act
            var resultDto = await _handler.Handle(query, CancellationToken.None);

            // Assert
            resultDto.Should().NotBeNull();
            _resultsCalculatorMock.Verify(c => c.CalculateResultsAsync(poll, It.IsAny<CancellationToken>()), Times.Once);
            _pollResultRepositoryMock.Verify(r => r.Create(calculatedResult), Times.Once);
            _repositoryManagerMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}