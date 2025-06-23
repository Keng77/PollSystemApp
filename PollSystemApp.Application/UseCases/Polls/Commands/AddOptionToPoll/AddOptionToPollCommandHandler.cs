using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Responses;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PollSystemApp.Application.UseCases.Polls.Commands.AddOptionToPoll
{
    public class AddOptionToPollCommandHandler : IRequestHandler<AddOptionToPollCommand, ApiBaseResponse>
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

        public async Task<ApiBaseResponse> Handle(AddOptionToPollCommand request, CancellationToken cancellationToken)
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

            if (request.OptionData.Order <= 0) 
            {
                var lastOption = await _repositoryManager.Options
                    .FindByCondition(o => o.PollId == request.PollId, trackChanges: false)
                    .OrderByDescending(o => o.Order)
                    .FirstOrDefaultAsync(cancellationToken);

                option.Order = (lastOption?.Order ?? -1) + 1; 
            }

            _repositoryManager.Options.Create(option);
            await _repositoryManager.CommitAsync(cancellationToken);

            var optionDto = _mapper.Map<OptionDto>(option);
            return new ApiOkResponse<OptionDto>(optionDto);
        }
    }
}