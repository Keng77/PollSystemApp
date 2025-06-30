using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PollSystemApp.Domain.Users;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PollSystemApp.Infrastructure.Common.Persistence
{
    public class DataSeeder
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(UserManager<User> userManager, RoleManager<Role> roleManager, ILogger<DataSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedRolesAndAdminAsync(AdminSettings adminSettings)
        {
            await SeedRoleAsync(UserRoles.Admin);
            await SeedRoleAsync(UserRoles.User);
            await SeedAdminUserAsync(adminSettings);
        }

        private async Task SeedRoleAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new Role { Name = roleName });
                _logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
            }
        }

        private async Task SeedAdminUserAsync(AdminSettings adminSettings)
        {
            var existingAdmin = await _userManager.FindByEmailAsync(adminSettings.Email);
            if (existingAdmin == null)
            {
                var adminUser = new User
                {
                    UserName = adminSettings.UserName,
                    Email = adminSettings.Email,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, adminSettings.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                    _logger.LogInformation("Admin user '{UserName}' created successfully and assigned '{AdminRole}' role.", adminSettings.UserName, UserRoles.Admin);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create admin user. Errors: {Errors}", errors);
                }
            }
        }
    }

    public class AdminSettings
    {
        public const string SectionName = "AdminCredentials";
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}