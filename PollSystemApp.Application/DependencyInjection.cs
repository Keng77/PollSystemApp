using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace PollSystemApp.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            

            return services;
        }
    }
}