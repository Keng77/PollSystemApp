using AutoMapper;
using MediatR;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Responses; 
using PollSystemApp.Domain.Polls;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PollSystemApp.Domain.Common.Exceptions; 
using System.Collections.Generic; 

namespace PollSystemApp.Application.UseCases.Polls.Commands.CreatePoll
{
    public class CreatePollCommandHandler : IRequestHandler<CreatePollCommand, ApiBaseResponse>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CreatePollCommandHandler(IRepositoryManager repositoryManager, IMapper mapper , ICurrentUserService currentUserService)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<ApiBaseResponse> Handle(CreatePollCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue) 
            { 
                throw new ForbiddenAccessException("User is not authenticated or user ID could not be determined.");
            }

            var poll = _mapper.Map<Poll>(request);
            poll.Id = Guid.NewGuid();
            poll.CreatedAt = DateTime.UtcNow;
            poll.CreatedBy = userId.Value;

            if (request.Tags != null && request.Tags.Count != 0)
            {
                poll.Tags = [];
                foreach (var tagName in request.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var existingTag = await _repositoryManager.Tags.FirstOrDefaultAsync(t => t.Name.Equals(tagName, StringComparison.CurrentCultureIgnoreCase), cancellationToken: cancellationToken);
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

            if (request.Options != null && request.Options.Count != 0)
            {
                foreach (var optionDto in request.Options)
                {
                    var option = _mapper.Map<Option>(optionDto);
                    option.Id = Guid.NewGuid();
                    option.PollId = poll.Id;
                    _repositoryManager.Options.Create(option);
                }
            }
            else 
            {
               throw new BadRequestException("A poll must have at least one option.");
            }

            await _repositoryManager.CommitAsync(cancellationToken);
            return new ApiOkResponse<Guid>(poll.Id);
        }
    }
}