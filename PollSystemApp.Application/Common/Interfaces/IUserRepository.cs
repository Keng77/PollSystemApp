using Microsoft.AspNetCore.Identity;
using PollSystemApp.Domain.Users;

namespace PollSystemApp.Application.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, bool trackChanges = false);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUserNameAsync(string name);
        Task<IList<string>> GetRolesAsync(User user);

        Task<IdentityResult> RegisterAsync(User user, string password);
        Task<IdentityResult> AddRolesToUserAsync(User user, ICollection<string> roles);
        Task<IdentityResult> UpdateAsync(User user);
        Task<IdentityResult> DeleteAsync(User user);

        Task<bool> CheckPasswordAsync(User user, string password);

    }
}
