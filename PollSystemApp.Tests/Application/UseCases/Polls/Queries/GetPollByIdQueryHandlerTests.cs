using AutoMapper;
using FluentAssertions;
using Moq;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Mappings;
using PollSystemApp.Application.UseCases.Polls.Queries.GetPollById;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using Microsoft.Extensions.Logging;
using PollSystemApp.Application.Common.Dto.PollDtos;

namespace PollSystemApp.Tests.Application.UseCases.Polls.Queries
{
    public class GetPollByIdQueryHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<GetPollByIdQueryHandler>> _loggerMock;
        private readonly GetPollByIdQueryHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;

        public GetPollByIdQueryHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _pollRepositoryMock = new Mock<IPollRepository>();
            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);

            _loggerMock = new Mock<ILogger<GetPollByIdQueryHandler>>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _handler = new GetPollByIdQueryHandler(
                _repositoryManagerMock.Object,
                _mapper,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnPollDto_WhenPollExists()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var query = new GetPollByIdQuery { Id = pollId };
            var pollFromDb = new Poll
            {
                Id = pollId,
                Title = "Test Poll",
                Options = new List<Option>
                {
                    new Option { Id = Guid.NewGuid(), PollId = pollId, Text = "Option B", Order = 2 },
                    new Option { Id = Guid.NewGuid(), PollId = pollId, Text = "Option A", Order = 1 }
                }
            };

            _pollRepositoryMock.Setup(r => r.GetPollWithDetailsAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(pollFromDb);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PollDto>();
            result.Id.Should().Be(pollId);
            result.Title.Should().Be("Test Poll");
            result.Options.Should().HaveCount(2);
            result.Options.First().Text.Should().Be("Option A");
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenPollDoesNotExist()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var query = new GetPollByIdQuery { Id = pollId };

            _pollRepositoryMock.Setup(r => r.GetPollWithDetailsAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                               .ReturnsAsync((Poll?)null);

            // Act
            Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage($"Entity \"Poll\" ({pollId}) was not found.");
        }
    }
}