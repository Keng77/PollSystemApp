using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PollSystemApp.Application.Common.Behaviors;
using PollSystemApp.Application.Common.Interfaces;
using PollSystemApp.Application.Services;
using System.Reflection;

namespace PollSystemApp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            services.AddScoped<IPollResultsCalculator, PollResultsCalculator>();

            return services;
        }
    }
}