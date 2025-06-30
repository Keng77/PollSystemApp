using PollSystemApp.Api;
using PollSystemApp.Application;
using PollSystemApp.Infrastructure;
using PollSystemApp.Infrastructure.Common.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Конфигурация логирования 
    builder.Host.UseSerilog((context, services, configuration) => configuration
       .ReadFrom.Configuration(context.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext());

    // Регистрация сервисов по слоям 
    builder.Services
        .AddApiServices() 
        .AddApplicationServices()
        .AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // Конфигурация HTTP пайплайна 
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        await app.SeedDatabaseAsync();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Poll System API V1");
        });
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly"); 
}
finally
{
    Log.CloseAndFlush();
}