using MediatR;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Common.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PollSystemApp.Application.UseCases.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ICurrentUserService _currentUserService;

        public ChangePasswordCommandHandler(IRepositoryManager repositoryManager, ICurrentUserService currentUserService)
        {
            _repositoryManager = repositoryManager;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                throw new ForbiddenAccessException("User is not authenticated.");
            }

            var user = await _repositoryManager.Users.GetByIdAsync(userId.Value)
                ?? throw new NotFoundException("User not found.");

            var changePasswordResult = await _repositoryManager.Users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                var errors = changePasswordResult.Errors
                    .ToDictionary(e => e.Code.Contains("Password") ? "CurrentPassword" : "General", e => new[] { e.Description });

                throw new ValidationAppException(errors);
            }
            return Unit.Value;
        }
    }
}