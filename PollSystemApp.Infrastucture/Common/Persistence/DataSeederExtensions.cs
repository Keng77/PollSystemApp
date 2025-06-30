using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PollSystemApp.Domain.Users;
using System;
using System.Threading.Tasks;

namespace PollSystemApp.Infrastructure.Common.Persistence
{
    public static class DataSeederExtensions
    {
        public static async Task SeedDatabaseAsync(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();

                try
                {
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    var roleManager = services.GetRequiredService<RoleManager<Role>>();
                    var adminSettings = services.GetRequiredService<IOptions<AdminSettings>>().Value;
                    var seederLogger = loggerFactory.CreateLogger<DataSeeder>();

                    var seeder = new DataSeeder(userManager, roleManager, seederLogger);
                    await seeder.SeedRolesAndAdminAsync(adminSettings);
                }
                catch (Exception ex)
                {
                    var logger = loggerFactory.CreateLogger("DataSeeder");
                    logger.LogError(ex, "An error occurred during data seeding.");
                }
            }
        }
    }
}