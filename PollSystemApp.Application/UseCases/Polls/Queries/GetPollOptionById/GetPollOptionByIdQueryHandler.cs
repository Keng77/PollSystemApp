﻿using AutoMapper;
using MediatR;
using PollSystemApp.Application.Common.Dto.OptionDtos;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Polls;

namespace PollSystemApp.Application.UseCases.Polls.Queries.GetPollOptionById
{
    public class GetPollOptionByIdQueryHandler : IRequestHandler<GetPollOptionByIdQuery, OptionDto>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;

        public GetPollOptionByIdQueryHandler(IRepositoryManager repositoryManager, IMapper mapper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
        }

        public async Task<OptionDto> Handle(GetPollOptionByIdQuery request, CancellationToken cancellationToken)
        {
            var pollExists = await _repositoryManager.Polls.ExistsAsync(p => p.Id == request.PollId, cancellationToken);
            if (!pollExists)
            {
                throw new NotFoundException(nameof(Poll), request.PollId);
            }

            var option = await _repositoryManager.Options.FirstOrDefaultAsync(
                o => o.Id == request.OptionId && o.PollId == request.PollId,
                trackChanges: false,
                cancellationToken: cancellationToken);

            if (option == null)
            {
                throw new NotFoundException($"Option with ID '{request.OptionId}' not found for Poll with ID '{request.PollId}'.");
            }

            var optionDto = _mapper.Map<OptionDto>(option);
            return optionDto;
        }
    }
}