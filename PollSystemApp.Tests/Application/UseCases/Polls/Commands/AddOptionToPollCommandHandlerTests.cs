using AutoMapper;
using FluentAssertions;
using Moq;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Mappings;
using PollSystemApp.Application.UseCases.Polls.Commands.AddOptionToPoll;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Tests.Application.UseCases.Polls.Commands
{
    public class AddOptionToPollCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly AddOptionToPollCommandHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;
        private readonly Mock<IOptionRepository> _optionRepositoryMock;

        public AddOptionToPollCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();
            _pollRepositoryMock = new Mock<IPollRepository>();
            _optionRepositoryMock = new Mock<IOptionRepository>();
            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Options).Returns(_optionRepositoryMock.Object);

            _currentUserServiceMock = new Mock<ICurrentUserService>();

            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            _mapper = mapperConfig.CreateMapper();

            _handler = new AddOptionToPollCommandHandler(
                _repositoryManagerMock.Object,
                _mapper,
                _currentUserServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldAddOptionAndSetOrderAutomatically_WhenOrderIsNotProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var command = new AddOptionToPollCommand
            {
                PollId = pollId,
                OptionData = new OptionForCreationDto { Text = "New Option", Order = 0 } 
            };

            var poll = new Poll { Id = pollId, CreatedBy = userId };
            var lastOption = new Option { PollId = pollId, Order = 5 }; 

            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);
            _optionRepositoryMock.Setup(r => r.GetLastOptionByPollIdAsync(pollId, false, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(lastOption);

            // Act
            var resultDto = await _handler.Handle(command, CancellationToken.None);

            // Assert
            resultDto.Should().NotBeNull();
            resultDto.Text.Should().Be("New Option");
            resultDto.Order.Should().Be(6);

            _optionRepositoryMock.Verify(r => r.Create(It.Is<Option>(o => o.Order == 6)), Times.Once);
            _repositoryManagerMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsNotOwner()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var pollId = Guid.NewGuid();
            var command = new AddOptionToPollCommand { PollId = pollId, OptionData = new OptionForCreationDto() };
            var poll = new Poll { Id = pollId, CreatedBy = ownerId };

            _currentUserServiceMock.Setup(s => s.UserId).Returns(otherUserId);
            _currentUserServiceMock.Setup(s => s.IsInRole("Admin")).Returns(false);
            _pollRepositoryMock.Setup(r => r.GetByIdAsync(pollId, false)).ReturnsAsync(poll);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenAccessException>();
        }
    }
}