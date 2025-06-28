using AutoMapper;
using MediatR;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using PollSystemApp.Domain.Users;

namespace PollSystemApp.Application.UseCases.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Unit>
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public RegisterUserCommandHandler(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = _mapper.Map<User>(request);
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;

        var result = await _repositoryManager.Users.RegisterAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errorsDictionary = result.Errors
            .GroupBy(e => e.Code.Contains("Password") ? "Password" : e.Code.Contains("Email") ? "Email" : e.Code.Contains("UserName") ? "UserName" : "General")
            .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            throw new ValidationAppException(errorsDictionary);
        }

        var roleResult = await _repositoryManager.Users.AddRolesToUserAsync(user, new List<string> { "User" });
        if (!roleResult.Succeeded)
        {
        }

        return Unit.Value;
    }
}