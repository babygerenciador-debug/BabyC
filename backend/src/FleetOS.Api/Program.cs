using FleetOS.Api.Extensions;
using FleetOS.Application;
using FleetOS.Infrastructure;
using FleetOS.Infrastructure.Hubs;
using FleetOS.Infrastructure.Persistence;
using NetEscapades.AspNetCore.SecurityHeaders;
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

    // Security headers policies (NetEscapades) — configured in DI for per-endpoint override support
    builder.Services.AddSecurityHeaderPolicies()
        .SetDefaultPolicy(policy =>
        {
            policy.AddFrameOptionsDeny()                                        // X-Frame-Options: DENY
                  .AddContentTypeOptionsNoSniff()                               // X-Content-Type-Options: nosniff
                  .AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 31536000) // HSTS: 1 year
                  .AddReferrerPolicyStrictOriginWhenCrossOrigin()               // Referrer-Policy
                  .AddXssProtectionBlock()                                      // X-XSS-Protection: 1; mode=block (legacy browsers)
                  .AddContentSecurityPolicy(csp =>
                  {
                      // Base URI - only allow same origin
                      csp.AddBaseUri().Self();
                      
                      // Default-src - fallback for other directives
                      csp.AddDefaultSrc().Self();
                      
                      // Script-src - allow self, external scripts (no inline scripts)
                      // NOTE: unsafe-inline removed - theme initialization moved to /theme-init.js
                      csp.AddScriptSrc()
                          .Self()
                          .From("https://fonts.googleapis.com")
                          .From("https://fonts.gstatic.com");
                      
                      // Style-src - allow self, inline styles, Google Fonts
                      csp.AddStyleSrc()
                          .Self()
                          .UnsafeInline()
                          .From("https://fonts.googleapis.com")
                          .From("https://fonts.gstatic.com");
                      
                      // Font-src - allow Google Fonts
                      csp.AddFontSrc()
                          .Self()
                          .From("https://fonts.gstatic.com");
                      
                      // Img-src - allow self, data URIs, blob URLs
                      csp.AddImgSrc()
                          .Self()
                          .Data()
                          .Blob();
                      
                      // Connect-src - allow self for API calls, WebSocket for SignalR
                      csp.AddConnectSrc()
                          .Self()
                          .From("https://*.onrender.com")                        // Backend API
                          .From("wss://*.onrender.com");                         // SignalR WebSocket
                      
                      // Frame-src - prevent framing (already covered by X-Frame-Options)
                      csp.AddFrameSrc().None();
                      
                      // Object-src - prevent Flash/other plugins
                      csp.AddObjectSrc().None();
                      
                      // Media-src - allow self for uploaded content
                      csp.AddMediaSrc().Self();
                      
                      // Worker-src - allow self for web workers
                      csp.AddWorkerSrc().Self().Blob();
                      
                      // Form-action - only allow self
                      csp.AddFormAction().Self();
                  })
                  .AddPermissionsPolicy(ppBuilder =>
                  {
                      ppBuilder.AddCamera().Self();                             // camera=(self)
                      ppBuilder.AddMicrophone().None();                         // microphone=()
                      ppBuilder.AddGeolocation().Self();                        // geolocation=(self)
                  });
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSignalR().AddHubOptions<FleetHub>(options =>
    {
        options.KeepAliveInterval = TimeSpan.FromSeconds(10);
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        options.EnableDetailedErrors = true;
    });
    var app = builder.Build();

    // Security headers MUST be first middleware to apply to ALL responses (including errors)
    app.UseSecurityHeaders();

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
    app.UseWebSockets(new WebSocketOptions
    {
        KeepAliveInterval = TimeSpan.FromSeconds(30)
    });

    // SignalR and health must be before rate limiter to avoid WS upgrade being blocked
    app.MapHub<FleetHub>("/hubs/fleet");
    app.MapHealthChecks("/health");

    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseStaticFiles(); // Added for receipt uploads

    app.UseCorrelationId();
    app.UseTenantResolver();
    app.UseGlobalExceptionHandler();

    app.MapControllers();

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

// Expose Program type for WebApplicationFactory<Program> in integration tests
public partial class Program { }
