using AutoMapper;
using MediatR;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.UseCases.Polls.Commands.CreatePoll
{
    public class CreatePollCommandHandler : IRequestHandler<CreatePollCommand, Guid>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CreatePollCommandHandler(IRepositoryManager repositoryManager, IMapper mapper, ICurrentUserService currentUserService)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreatePollCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                throw new ForbiddenAccessException("User is not authenticated or user ID could not be determined.");
            }
            
            if (request.Options == null || request.Options.Count < 2)
            {
                throw new BadRequestException("A poll must have at least two options.");
            }

            var poll = _mapper.Map<Poll>(request);

            poll.Id = Guid.NewGuid();
            poll.CreatedAt = DateTime.UtcNow;
            poll.CreatedBy = userId.Value;

            foreach (var option in poll.Options)
            {
                option.Id = Guid.NewGuid();
            }

            if (request.Tags != null && request.Tags.Any())
            {
                poll.Tags = new List<Tag>();
                foreach (var tagName in request.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var normalizedTagName = tagName.ToLower();

                    var existingTag = await _repositoryManager.Tags.FirstOrDefaultAsync(
                        t => t.Name.ToLower() == normalizedTagName,
                        trackChanges: true,
                        cancellationToken: cancellationToken);

                    if (existingTag != null)
                    {
                        poll.Tags.Add(existingTag);
                    }
                    else
                    {
                        var newTag = new Tag { Id = Guid.NewGuid(), Name = tagName };
                        poll.Tags.Add(newTag);
                    }
                }
            }

            _repositoryManager.Polls.Create(poll);
            await _repositoryManager.CommitAsync(cancellationToken);

            return poll.Id;
        }
    }
}