using AutoMapper;
using FluentAssertions;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Mappings;
using PollSystemApp.Application.Common.Pagination;
using PollSystemApp.Application.UseCases.Polls.Queries.GetAllPolls;
using PollSystemApp.Domain.Polls;


namespace PollSystemApp.Tests.Application.UseCases.Polls.Queries
{
    public class GetAllPollsQueryHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly IMapper _mapper;
        private readonly GetAllPollsQueryHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;
        private readonly Mock<IOptionRepository> _optionRepositoryMock;

        public GetAllPollsQueryHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

            _pollRepositoryMock = new Mock<IPollRepository>();
            _optionRepositoryMock = new Mock<IOptionRepository>();
            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Options).Returns(_optionRepositoryMock.Object);

            _handler = new GetAllPollsQueryHandler(_repositoryManagerMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnPagedResponseOfPolls_WhenPollsExist()
        {
            // Arrange
            var query = new GetAllPollsQuery { PageNumber = 1, PageSize = 10 };
            var pollId1 = Guid.NewGuid();
            var pollId2 = Guid.NewGuid();

            var pollsFromDb = new List<Poll>
            {
                new Poll { Id = pollId1, Title = "Poll 1" },
                new Poll { Id = pollId2, Title = "Poll 2" }
            };

            var pagedList = new PagedList<Poll>(pollsFromDb, 2, 1, 10);

            var optionsFromDb = new List<Option>
            {
                new Option { PollId = pollId1, Text = "Opt A" },
                new Option { PollId = pollId2, Text = "Opt B" }
            };

            _pollRepositoryMock.Setup(r => r.GetPollsAsync(query, false, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(pagedList);
            _optionRepositoryMock.Setup(r => r.GetOptionsByPollIdsAsync(It.Is<List<Guid>>(ids => ids.Contains(pollId1) && ids.Contains(pollId2)), false, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(optionsFromDb);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.MetaData.TotalCount.Should().Be(2);

            result.Items.First(p => p.Id == pollId1).Options.Should().HaveCount(1);
            result.Items.First(p => p.Id == pollId1).Options.First().Text.Should().Be("Opt A");

            result.Items.First(p => p.Id == pollId2).Options.Should().HaveCount(1);
            result.Items.First(p => p.Id == pollId2).Options.First().Text.Should().Be("Opt B");
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyPagedResponse_WhenNoPollsExist()
        {
            // Arrange
            var query = new GetAllPollsQuery { PageNumber = 1, PageSize = 10 };
            var pagedList = new PagedList<Poll>(new List<Poll>(), 0, 1, 10);

            _pollRepositoryMock.Setup(r => r.GetPollsAsync(query, false, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(pagedList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEmpty();
            result.MetaData.TotalCount.Should().Be(0);

            _optionRepositoryMock.Verify(r => r.GetOptionsByPollIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}