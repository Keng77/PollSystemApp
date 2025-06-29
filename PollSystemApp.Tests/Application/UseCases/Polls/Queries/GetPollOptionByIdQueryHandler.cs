using AutoMapper;
using FluentAssertions;
using Moq;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Mappings;
using PollSystemApp.Application.UseCases.Polls.Queries.GetPollOptionById;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using System.Linq.Expressions;


namespace PollSystemApp.Tests.Application.UseCases.Polls.Queries
{
    public class GetPollOptionByIdQueryHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly IMapper _mapper;
        private readonly GetPollOptionByIdQueryHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;
        private readonly Mock<IOptionRepository> _optionRepositoryMock;

        public GetPollOptionByIdQueryHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _pollRepositoryMock = new Mock<IPollRepository>();
            _optionRepositoryMock = new Mock<IOptionRepository>();
            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Options).Returns(_optionRepositoryMock.Object);

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = mapperConfig.CreateMapper();

            _handler = new GetPollOptionByIdQueryHandler(_repositoryManagerMock.Object, _mapper);
        }

        [Fact]
        public async Task Handle_ShouldReturnOptionDto_WhenPollAndOptionExist()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var optionId = Guid.NewGuid();
            var query = new GetPollOptionByIdQuery { PollId = pollId, OptionId = optionId };

            var option = new Option { Id = optionId, PollId = pollId, Text = "Correct Option" };

            _pollRepositoryMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);
            _optionRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Option, bool>>>(), false, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(option);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<OptionDto>();
            result.Id.Should().Be(optionId);
            result.Text.Should().Be("Correct Option");
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenPollDoesNotExist()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var optionId = Guid.NewGuid();
            var query = new GetPollOptionByIdQuery { PollId = pollId, OptionId = optionId };

            _pollRepositoryMock.Setup(r => r.ExistsAsync(p => p.Id == pollId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(false);

            // Act
            Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                     .WithMessage($"Entity \"Poll\" ({pollId}) was not found.");
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenOptionDoesNotExistForGivenPoll()
        {
            // Arrange
            var pollId = Guid.NewGuid();
            var optionId = Guid.NewGuid();
            var query = new GetPollOptionByIdQuery { PollId = pollId, OptionId = optionId };

            _pollRepositoryMock.Setup(r => r.ExistsAsync(p => p.Id == pollId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(true);
            _optionRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Option, bool>>>(), false, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((Option)null);

            // Act
            Func<Task> act = () => _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}