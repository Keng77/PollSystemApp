using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.UseCases.Polls.Commands.AddOptionToPoll
{
    public class AddOptionToPollCommandHandler : IRequestHandler<AddOptionToPollCommand, OptionDto>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public AddOptionToPollCommandHandler(IRepositoryManager repositoryManager, IMapper mapper, ICurrentUserService currentUserService)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<OptionDto> Handle(AddOptionToPollCommand request, CancellationToken cancellationToken)
        {
            var poll = await _repositoryManager.Polls.GetByIdAsync(request.PollId, trackChanges: false);
            if (poll == null)
            {
                throw new NotFoundException(nameof(Poll), request.PollId);
            }

            var currentUserId = _currentUserService.UserId;
            if (poll.CreatedBy != currentUserId && !_currentUserService.IsInRole("Admin"))
            {
                throw new ForbiddenAccessException("You are not authorized to add options to this poll.");
            }

            var option = _mapper.Map<Option>(request.OptionData);
            option.Id = Guid.NewGuid();
            option.PollId = request.PollId;

            if (request.OptionData.Order > 0)
            {
                bool orderExists = await _repositoryManager.Options.ExistsAsync(
                    o => o.PollId == request.PollId && o.Order == request.OptionData.Order,
                    cancellationToken);

                if (orderExists)
                {
                    throw new BadRequestException($"An option with order '{request.OptionData.Order}' already exists in this poll.");
                }
            }
            else 
            {
                var lastOption = await _repositoryManager.Options.GetLastOptionByPollIdAsync(request.PollId, false, cancellationToken);
                option.Order = (lastOption?.Order ?? 0) + 1; 
            }

            _repositoryManager.Options.Create(option);
            await _repositoryManager.CommitAsync(cancellationToken);

            var optionDto = _mapper.Map<OptionDto>(option);
            return optionDto;
        }
    }
}