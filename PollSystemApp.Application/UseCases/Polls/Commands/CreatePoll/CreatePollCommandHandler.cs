using AutoMapper;
using MediatR;
using PollSystemApp.Application.Common.Interfaces; 
using PollSystemApp.Domain.Polls;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PollSystemApp.Application.UseCases.Polls.Commands.CreatePoll
{
    public class CreatePollCommandHandler : IRequestHandler<CreatePollCommand, Guid>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        // private readonly ICurrentUserService _currentUserService; 

        public CreatePollCommandHandler(IRepositoryManager repositoryManager, IMapper mapper /*, ICurrentUserService currentUserService */)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            // _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreatePollCommand request, CancellationToken cancellationToken)
        {
            
            // var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var placeholderUserId = new Guid("11111112-49b6-410c-bc78-2d54a9991870"); // Временная заглушка (ID роли User)

            var poll = _mapper.Map<Poll>(request);
            poll.Id = Guid.NewGuid();
            poll.CreatedAt = DateTime.UtcNow;
            poll.CreatedBy = placeholderUserId; // Заменить на userId

            
            if (request.Tags != null && request.Tags.Any())
            {
                poll.Tags = new List<Tag>();
                foreach (var tagName in request.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var existingTag = await _repositoryManager.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
                    if (existingTag != null)
                    {
                        poll.Tags.Add(existingTag);
                    }
                    else
                    {
                        var newTag = new Tag { Id = Guid.NewGuid(), Name = tagName };
                        _repositoryManager.Tags.Create(newTag); 
                        poll.Tags.Add(newTag);
                    }
                }
            }

            _repositoryManager.Polls.Create(poll);
                        
            if (request.Options != null && request.Options.Any())
            {
                foreach (var optionDto in request.Options)
                {
                    var option = _mapper.Map<Option>(optionDto);
                    option.Id = Guid.NewGuid();
                    option.PollId = poll.Id;
                    _repositoryManager.Options.Create(option);
                }
            }

            await _repositoryManager.CommitAsync(cancellationToken);

            return poll.Id;
        }
    }
}