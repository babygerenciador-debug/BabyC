using FleetOS.Api.Extensions;
using FleetOS.Application;
using FleetOS.Infrastructure;
using FleetOS.Infrastructure.Hubs;
using FleetOS.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting FleetOS API...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) =>
    {
        lc.ReadFrom.Configuration(ctx.Configuration)
          .Enrich.FromLogContext()
          .Enrich.WithMachineName()
          .Enrich.WithThreadId()
          .WriteTo.Console();

        // File logging only in non-production (Render has ephemeral disk)
        if (!ctx.HostingEnvironment.IsProduction())
        {
            lc.WriteTo.File("logs/fleetos-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30);
        }
    });

    builder.Services
        .AddApiServices(builder.Configuration)
        .AddAuthServices(builder.Configuration)
        .AddApplicationServices()
        .AddInfrastructureServices(builder.Configuration);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSignalR().AddHubOptions<FleetHub>(options =>
    {
        options.KeepAliveInterval = TimeSpan.FromSeconds(10);
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        options.EnableDetailedErrors = true;
    });
    var app = builder.Build();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FleetOS API v1");
            c.RoutePrefix = "swagger";
        });
    }

    // Only redirect HTTPS in local dev — in Docker/Prod, TLS is terminated at Nginx.
    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseCors("AllowedOrigins");
    app.UseStaticFiles(); // Added for receipt uploads
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseCorrelationId();
    app.UseTenantResolver();
    app.UseGlobalExceptionHandler();

    app.MapControllers();
    app.MapHub<FleetHub>("/hubs/fleet");
    app.MapHealthChecks("/health");

    await app.MigrateAndSeedAsync();

    await app.RunAsync();
    return 0;
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "FleetOS API terminated unexpectedly.");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
