using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Tenants;
using FleetOS.Domain.Core.Users;
using FleetOS.Infrastructure.Persistence;
using FleetOS.Infrastructure.Persistence.Interceptors;
using FleetOS.Infrastructure.Persistence.Repositories;
using FleetOS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FleetOS.Infrastructure;

/// <summary>
/// Infrastructure DI registration.
/// Registers EF Core, Redis, repositories and services.
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── EF Core ────────────────────────────────────────────────────
        services.AddDbContext<FleetOsDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(FleetOsDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(maxRetryCount: 3);
                    npgsql.CommandTimeout(30);
                });

            options.UseSnakeCaseNamingConvention(); // PostgreSQL convention

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        // ── Interceptors / Scoped services ────────────────────────────
        services.AddScoped<AuditInterceptor>();
        services.RegisterIdentityServices();
        services.RegisterInfrastructureServices();

        // ── Redis (optional — falls back to in-memory cache) ───────────
        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            try
            {
                var redisPassword = configuration["Redis:Password"];
                var redisConfig = ConfigurationOptions.Parse(redisConnectionString);
                if (!string.IsNullOrWhiteSpace(redisPassword))
                    redisConfig.Password = redisPassword;

                services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfig));
                services.AddStackExchangeRedisCache(opts =>
                {
                    opts.ConfigurationOptions = redisConfig;
                    opts.InstanceName = "fleetos:";
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FleetOS] Redis connection failed, falling back to in-memory cache: {ex.Message}");
                services.AddDistributedMemoryCache();
            }
        }
        else
        {
            services.AddDistributedMemoryCache();
        }
        
        // ── Auth services ──────────────────────────────────────────────
        services.AddScoped<IPasswordService, BcryptPasswordService>();
        services.AddScoped<IJwtService, JwtService>();

        // ── Repositories ───────────────────────────────────────────────
        // (registered per module as they are built)
        services.RegisterRepositories();

        return services;
    }

    private static IServiceCollection RegisterIdentityServices(this IServiceCollection services)
    {
        // services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<FleetOsDbContext>());
        
        return services;
    }

    private static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services)
    {
        // Redis Cache
        // services.AddSingleton<ICacheService, RedisCacheService>();

        // Supabase Storage
        services.AddScoped<IStorageService, SupabaseStorageService>();

        // Real-time notifications
        services.AddScoped<IFleetNotificationService, FleetNotificationService>();

        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IFuelLogRepository, FuelLogRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        
        // Inventory
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockBalanceRepository, StockBalanceRepository>();
        services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
        
        // Finance
        services.AddScoped<ICostCenterRepository, CostCenterRepository>();
        services.AddScoped<IFinancialCategoryRepository, FinancialCategoryRepository>();
        services.AddScoped<IFinancialMonthRepository, FinancialMonthRepository>();
        services.AddScoped<IFinancialTransactionRepository, FinancialTransactionRepository>();
        
        // Dashboard
        services.AddScoped<IDashboardRepository, DashboardRepository>();

        // Notifications & Issues
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IVehicleIssueReportRepository, VehicleIssueReportRepository>();
        
        // Background Jobs
        services.AddHostedService<FleetOS.Infrastructure.BackgroundJobs.AlertJob>();

        return services;
    }
}
