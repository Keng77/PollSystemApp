using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity; 
using PollSystemApp.Application.Common.Interfaces; 
using PollSystemApp.Application.Common.Responses;
using PollSystemApp.Domain.Common.Exceptions; 
using PollSystemApp.Domain.Users; 
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic; 

namespace PollSystemApp.Application.UseCases.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ApiBaseResponse>
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public RegisterUserCommandHandler(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<ApiBaseResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = _mapper.Map<User>(request); 
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        var result = await _repositoryManager.Users.RegisterAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            throw new BadRequestException($"User registration failed: {string.Join(", ", errors)}");
        }

        var roleResult = await _repositoryManager.Users.AddRolesToUserAsync(user, new List<string> { "User" });
        if (!roleResult.Succeeded)
        {
        }

        return new ApiOkResponse(); 
    }
}