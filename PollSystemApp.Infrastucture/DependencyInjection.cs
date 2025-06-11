using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PollSystemApp.Domain.Users;
using PollSystemApp.Infrastructure.Common.Persistence;


namespace PollSystemApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
              .AddHttpContextAccessor()
              .AddAuthorization()
              .AddConfigIdentity()
              .AddPersistence(configuration);

            return services;
        }

        
        private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

            services.AddDbContext<AppDbContext>(opts =>
              opts.UseSqlServer(connectionString, b =>
              {
                  b.MigrationsAssembly("PollSystemApp.Infrastructure");
                  b.EnableRetryOnFailure();
              })
            );
                        
            return services;
        }

       
        public static IServiceCollection AddConfigIdentity(this IServiceCollection services)
        {
            var builder = services.AddIdentity<User, Role>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireUppercase = true;
                o.Password.RequireNonAlphanumeric = true;
                o.Password.RequiredLength = 8;
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            return services;
        }
    }


}
