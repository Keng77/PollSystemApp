using AutoMapper;
using FluentAssertions;
using Moq;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Mappings;
using PollSystemApp.Application.UseCases.Polls.Commands.CreatePoll;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PollSystemApp.Tests.Application.UseCases.Polls.Commands
{
    public class CreatePollCommandHandlerTests
    {
        private readonly Mock<IRepositoryManager> _repositoryManagerMock;
        private readonly IMapper _mapper;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly CreatePollCommandHandler _handler;

        private readonly Mock<IPollRepository> _pollRepositoryMock;
        private readonly Mock<IOptionRepository> _optionRepositoryMock;
        private readonly Mock<ITagRepository> _tagRepositoryMock;

        public CreatePollCommandHandlerTests()
        {
            _repositoryManagerMock = new Mock<IRepositoryManager>();

            _pollRepositoryMock = new Mock<IPollRepository>();
            _optionRepositoryMock = new Mock<IOptionRepository>();
            _tagRepositoryMock = new Mock<ITagRepository>();

            _repositoryManagerMock.Setup(r => r.Polls).Returns(_pollRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Options).Returns(_optionRepositoryMock.Object);
            _repositoryManagerMock.Setup(r => r.Tags).Returns(_tagRepositoryMock.Object);

            _currentUserServiceMock = new Mock<ICurrentUserService>();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            _mapper = mapperConfig.CreateMapper();

            _handler = new CreatePollCommandHandler(
                _repositoryManagerMock.Object,
                _mapper,
                _currentUserServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldCreatePollAndReturnPollId_WhenRequestIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreatePollCommand
            {
                Title = "Favorite Food",
                Options = new List<OptionForCreationDto>
                {
                    new() { Text = "Pizza" },
                    new() { Text = "Burger" }
                },
                Tags = new List<string> { "food", "fun" }
            };
            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId); 
            _tagRepositoryMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Tag, bool>>>(), false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync((Tag?)null);

            // Act
            var resultPollId = await _handler.Handle(command, CancellationToken.None);

            // Assert
            resultPollId.Should().NotBeEmpty();

            _pollRepositoryMock.Verify(r => r.Create(It.Is<Poll>(p =>
                p.Id == resultPollId &&
                p.CreatedBy == userId &&
                p.Title == command.Title &&
                p.Options.Count == 2
            )), Times.Once);

            _tagRepositoryMock.Verify(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Tag, bool>>>(), true,
                It.IsAny<CancellationToken>()), Times.Exactly(2));
            _repositoryManagerMock.Verify(r => r.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowForbiddenAccessException_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var command = new CreatePollCommand { Title = "Test" };
            _currentUserServiceMock.Setup(s => s.UserId).Returns((Guid?)null);

            // Act
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ForbiddenAccessException>();
        }

        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenNoOptionsAreProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new CreatePollCommand
            {
                Title = "Poll without options",
                Options = new List<OptionForCreationDto>()
            };
            _currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

            // Act 
            Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>()
                     .WithMessage("A poll must have at least two options.");
        }
    }
}