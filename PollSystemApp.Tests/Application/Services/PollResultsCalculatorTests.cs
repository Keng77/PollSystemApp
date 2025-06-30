using FluentAssertions;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Services;
using PollSystemApp.Domain.Polls;


namespace PollSystemApp.Tests.Application.Services
{
    public class PollResultsCalculatorTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly PollResultsCalculator _calculator;

        private readonly Mock<IOptionRepository> _optionRepositoryMock;
        private readonly Mock<IVoteRepository> _voteRepositoryMock;

        public PollResultsCalculatorTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _optionRepositoryMock = new Mock<IOptionRepository>();
            _voteRepositoryMock = new Mock<IVoteRepository>();

            _repositoryManagerMock.Setup(r => r.Options).Returns(_optionRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Votes).Returns(_voteRepositoryMock.Object);

            _calculator = new PollResultsCalculator(_repositoryManagerMock.Object);
        }

        [Fact]
        public async Task CalculateResultsAsync_ShouldCalculateCorrectly_ForNonAnonymousPoll()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var option1Id = Guid.NewGuid();
            var option2Id = Guid.NewGuid();
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            var user3Id = Guid.NewGuid();

            var poll = new Poll { Id = pollId, IsAnonymous = false };

            var options = new List<Option>
            {
                new Option { Id = option1Id, PollId = pollId, Text = "Opt 1" },
                new Option { Id = option2Id, PollId = pollId, Text = "Opt 2" }
            };


            var votes = new List<Vote>
            {
                new Vote { PollId = pollId, OptionId = option1Id, UserId = user1Id },
                new Vote { PollId = pollId, OptionId = option1Id, UserId = user2Id }, 
                new Vote { PollId = pollId, OptionId = option2Id, UserId = user3Id }, 
            };

            _optionRepositoryMock.Setup(r => r.GetOptionsByPollIdAsync(pollId, false, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(options);
            _voteRepositoryMock.Setup(r => r.GetVotesByPollIdAsync(pollId, false, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(votes);

            // Act
            var result = await _calculator.CalculateResultsAsync(poll, CancellationToken.None);

            // Assert
            result.TotalVotes.Should().Be(3);

            var option1Result = result.Options.FirstOrDefault(o => o.OptionId == option1Id);
            option1Result.Should().NotBeNull();
            option1Result.Votes.Should().Be(2);
            option1Result.Percentage.Should().BeApproximately(66.67, 0.01);

            var option2Result = result.Options.FirstOrDefault(o => o.OptionId == option2Id);
            option2Result.Should().NotBeNull();
            option2Result.Votes.Should().Be(1);
            option2Result.Percentage.Should().BeApproximately(33.33, 0.01);
        }

        [Fact]
        public async Task CalculateResultsAsync_ShouldCalculateCorrectly_ForAnonymousPoll()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var option1Id = Guid.NewGuid();
            var option2Id = Guid.NewGuid();

            var poll = new Poll { Id = pollId, IsAnonymous = true };

            var options = new List<Option>
            {
                new Option { Id = option1Id, PollId = pollId, Text = "Opt 1" },
                new Option { Id = option2Id, PollId = pollId, Text = "Opt 2" }
            };

            var votes = new List<Vote>
            {
                new Vote { PollId = pollId, OptionId = option1Id }, 
                new Vote { PollId = pollId, OptionId = option1Id }, 
                new Vote { PollId = pollId, OptionId = option1Id }, 
                new Vote { PollId = pollId, OptionId = option2Id }  
            };

            _optionRepositoryMock.Setup(r => r.GetOptionsByPollIdAsync(pollId, false, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(options);
            _voteRepositoryMock.Setup(r => r.GetVotesByPollIdAsync(pollId, false, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(votes);

            // Act
            var result = await _calculator.CalculateResultsAsync(poll, CancellationToken.None);

            // Assert
            result.TotalVotes.Should().Be(4); 

            var option1Result = result.Options.FirstOrDefault(o => o.OptionId == option1Id);
            option1Result!.Votes.Should().Be(3);
            option1Result.Percentage.Should().Be(75.00);

            var option2Result = result.Options.FirstOrDefault(o => o.OptionId == option2Id);
            option2Result!.Votes.Should().Be(1);
            option2Result.Percentage.Should().Be(25.00);
        }
    }
}