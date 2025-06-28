using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.UseCases.Votes.Commands.CreateVote
{
    public class CreateVoteCommandHandler : IRequestHandler<CreateVoteCommand, Unit>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateVoteCommandHandler(
            IRepositoryManager repositoryManager,
            ICurrentUserService currentUserService,
            IHttpContextAccessor httpContextAccessor)
        {
            _repositoryManager = repositoryManager;
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Unit> Handle(CreateVoteCommand request, CancellationToken cancellationToken)
        {
            var poll = await _repositoryManager.Polls.GetByIdAsync(request.PollId, trackChanges: false);
            if (poll == null)
            {
                throw new NotFoundException(nameof(Poll), request.PollId);
            }

            var now = DateTime.UtcNow;
            if (now < poll.StartDate)
            {
                throw new BadRequestException($"Voting for this poll has not started yet (starts at {poll.StartDate:o}).");
            }
            if (now > poll.EndDate)
            {
                throw new BadRequestException($"Voting for this poll has ended (ended at {poll.EndDate:o}).");
            }

            if (request.OptionIds == null || !request.OptionIds.Any())
            {
                throw new BadRequestException("At least one option must be selected to vote.");
            }
            if (!poll.IsMultipleChoice && request.OptionIds.Count > 1)
            {
                throw new BadRequestException("This poll does not allow multiple choices. Please select only one option.");
            }

            var validOptions = await _repositoryManager.Options
                .FindByCondition(o => o.PollId == request.PollId && request.OptionIds.Contains(o.Id), trackChanges: false)
                .ToListAsync(cancellationToken);

            if (validOptions.Count != request.OptionIds.Distinct().Count())
            {
                var invalidOptionIds = request.OptionIds.Except(validOptions.Select(vo => vo.Id)).ToList();
                throw new BadRequestException($"One or more selected options are invalid or do not belong to this poll. Invalid IDs: {string.Join(", ", invalidOptionIds)}");
            }

            Guid? userIdForVote = null;
            string ipAddressForVote = "ANONYMOUS";

            if (!poll.IsAnonymous)
            {
                if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
                {
                    throw new ForbiddenAccessException("User must be authenticated to vote in a non-anonymous poll.");
                }
                userIdForVote = _currentUserService.UserId.Value;
                ipAddressForVote = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "UNKNOWN_IP_AUTH";

                bool alreadyVoted = await _repositoryManager.Votes
                    .ExistsAsync(v => v.PollId == request.PollId && v.UserId == userIdForVote, cancellationToken);

                if (alreadyVoted)
                {
                    throw new BadRequestException("You have already voted in this poll.");
                }
            }

            var votesToCreate = new List<Vote>();
            foreach (var optionId in request.OptionIds.Distinct())
            {
                votesToCreate.Add(new Vote
                {
                    Id = Guid.NewGuid(),
                    PollId = request.PollId,
                    OptionId = optionId,
                    UserId = userIdForVote,
                    CreatedAt = now,
                    IpAddress = ipAddressForVote
                });
            }

            if (!votesToCreate.Any())
            {
                throw new BadRequestException("No valid votes to create.");
            }

            await _repositoryManager.Votes.CreateRangeAsync(votesToCreate, cancellationToken);
            await _repositoryManager.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}