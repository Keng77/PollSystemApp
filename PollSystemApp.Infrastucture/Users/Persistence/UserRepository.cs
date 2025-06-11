using Microsoft.AspNetCore.Identity;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Domain.Users;
using PollSystemApp.Infrastructure.Common.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PollSystemApp.Infrastructure.Users.Persistence
{
    public class UserRepository(UserManager<User> userManager, AppDbContext dbContext) : IUserRepository
    {
        private readonly UserManager<User> _userManager = userManager;
        private readonly AppDbContext _dbContext = dbContext;

        public async Task<User?> GetByIdAsync(Guid id, bool trackChanges = false) =>
          await _userManager.FindByIdAsync(id.ToString());
        public async Task<User?> GetByEmailAsync(string email) =>
          await _userManager.FindByEmailAsync(email);
        public async Task<User?> GetByUserNameAsync(string name) =>
          await _userManager.FindByNameAsync(name);
        public async Task<IList<string>> GetRolesAsync(User user) =>
          await _userManager.GetRolesAsync(user);

        public async Task<IdentityResult> AddRolesToUserAsync(User user, ICollection<string> roles) =>
          await _userManager.AddToRolesAsync(user, roles);
        public async Task<IdentityResult> RegisterAsync(User user, string password) =>
          await _userManager.CreateAsync(user, password);
        public async Task<IdentityResult> UpdateAsync(User user) =>
          await _userManager.UpdateAsync(user);
        public async Task<IdentityResult> DeleteAsync(User user) =>
          await _userManager.DeleteAsync(user);

        public async Task<bool> CheckPasswordAsync(User user, string password) =>
          await _userManager.CheckPasswordAsync(user, password);

    }

}
