using AutoMapper;
using FluentAssertions;
using MediatR;
using Moq;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Mappings;
using PollSystemApp.Application.UseCases.Polls.Commands.UpdatePollOption;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using System.Linq.Expressions;


namespace PollSystemApp.Tests.Application.UseCases.Polls.Commands
{
    public class UpdatePollOptionCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly UpdatePollOptionCommandHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;
        private readonly Mock<IOptionRepository> _optionRepositoryMock;

        public UpdatePollOptionCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _pollRepositoryMock = new Mock<IPollRepository>();
            _optionRepositoryMock = new Mock<IOptionRepository>();
            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Options).Returns(_optionRepositoryMock.Object);

            _currentUserServiceMock = new Mock<ICurrentUserService>();

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = mapperConfig.CreateMapper();

            _handler = new UpdatePollOptionCommandHandler(
                _repositoryManagerMock.Object,
                _mapper,
                _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldUpdateOption_WhenUserIsOwner()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var optionId = Guid.NewGuid();
            var command = new UpdatePollOptionCommand
            {
                PollId = pollId,
                OptionId = optionId,
                OptionData = new OptionForUpdateDto { Text = "Updated Text", Order = 10 }
            };

            var poll = new Poll { Id = pollId, CreatedBy = userId };
            var option = new Option { Id = optionId, PollId = pollId, Text = "Old Text", Order = 1 };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _optionRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Option, bool>>>(), true, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(option);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().Be(Unit.Value);
            option.Text.Should().Be("Updated Text");
            option.Order.Should().Be(10);
            _repositoryManagerMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenOptionDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var optionId = Guid.NewGuid();
            var command = new UpdatePollOptionCommand { PollId = pollId, OptionId = optionId, OptionData = new OptionForUpdateDto() };

            var poll = new Poll { Id = pollId, CreatedBy = userId };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _optionRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Option, bool>>>(), true, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((Option)null);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}