using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Common.Interfaces.Authentication;
using PollSystemApp.Application.Common.Settings;
using PollSystemApp.Domain.Users;
using PollSystemApp.Infrastructure.Authentication;
using PollSystemApp.Infrastructure.Common.Persistence;
using PollSystemApp.Infrastructure.Services;
using System.Text;



namespace PollSystemApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

            services
              .AddHttpContextAccessor()
              .AddPersistence(configuration)
              .AddConfigIdentity()
              .AddJwtAuthentication(configuration);

            services.AddAuthorization();

            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IRepositoryManager, RepositoryManager>();
            services.AddSingleton<ITokenValidator, TokenValidator>();

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

        private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            
            var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

            if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Secret))
            {
               throw new InvalidOperationException("JWT Secret is not configured. Please check your configuration (appsettings.json, user secrets, environment variables).");
            }

           
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; 
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false; 
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),

                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }


}
