using PollSystemApp.Api;
using PollSystemApp.Application;
using PollSystemApp.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ������������ ����������� 
    builder.Host.UseSerilog((context, services, configuration) => configuration
       .ReadFrom.Configuration(context.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext());

    // ����������� �������� �� ����� 
    builder.Services
        .AddApiServices() 
        .AddApplicationServices()
        .AddInfrastructure(builder.Configuration);

    var app = builder.Build();

    // ������������ HTTP ��������� 
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
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