using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Core.Tenants;
using FleetOS.Domain.Core.Users;
using FleetOS.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FleetOS.Infrastructure.Persistence;

public static class DbInitializer
{
    /// <summary>
    /// Applies pending migrations and seeds initial data.
    /// Receives an already-scoped IServiceProvider (no extra inner scope needed here).
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<FleetOsDbContext>();
        var passwordService = serviceProvider.GetRequiredService<IPasswordService>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        try
        {
            logger.LogInformation("Applying migrations...");
            await context.Database.MigrateAsync();

            logger.LogInformation("Seeding database...");

            if (!await context.Tenants.AnyAsync())
            {
                // Read seed credentials from configuration (.env / docker-compose env vars)
                var sysAdminEmail = configuration["Seed:SystemAdminEmail"] ?? "admin@fleetos.io";
                var sysAdminPassword = configuration["Seed:SystemAdminPassword"] ?? "Admin@123456";
                var tenantName = configuration["Seed:TenantName"] ?? "Baby Turismo";
                var tenantAdminEmail = configuration["Seed:TenantAdminEmail"] ?? "admin@babyturismo.com.br";
                var tenantAdminPassword = configuration["Seed:TenantAdminPassword"] ?? "Tenant@123456";

                // 1. Create FleetOS Platform Tenant (System Admin)
                var platformTenant = Tenant.Create("FleetOS Platform", "fleetos");
                platformTenant.SetPlan(TenantPlan.Enterprise);
                
                // 2. Create Baby Turismo Demo Tenant
                var babyTenant = Tenant.Create(tenantName, "babyturismo");
                babyTenant.SetPlan(TenantPlan.Professional);
                babyTenant.UpdateBranding("https://example.com/logo-baby.png", "#f43f5e");

                await context.Tenants.AddRangeAsync(platformTenant, babyTenant);
                await context.SaveChangesAsync(); // Commit tenants to get their IDs

                // 3. Create Default Organizations
                var babyOrg = Organization.Create(babyTenant.Id, "Baby Turismo Matriz", "12345678000199");
                await context.Organizations.AddAsync(babyOrg);
                await context.SaveChangesAsync();

                // 4. Create Default Business Units
                var babyBu = BusinessUnit.Create(babyTenant.Id, babyOrg.Id, "Garagem Principal", "MATRIZ-01");
                babyBu.SetAsHeadOffice();
                await context.BusinessUnits.AddAsync(babyBu);
                await context.SaveChangesAsync();

                // 5. Create System Admin
                var sysAdminEmailResult = Email.Create(sysAdminEmail);
                if (sysAdminEmailResult.IsFailure) 
                    throw new InvalidOperationException($"Erro ao criar e-mail do sistema: {sysAdminEmailResult.Error}");

                var sysAdmin = User.CreateAdminUser(
                    platformTenant.Id, 
                    Guid.Empty, 
                    Guid.Empty,
                    "System Administrator", 
                    sysAdminEmailResult.Value!,
                    passwordService.HashPassword(sysAdminPassword), 
                    UserRole.SystemAdmin);

                // 6. Create Baby Turismo Admin
                var babyAdminEmailResult = Email.Create(tenantAdminEmail);
                if (babyAdminEmailResult.IsFailure) 
                    throw new InvalidOperationException($"Erro ao criar e-mail do tenant: {babyAdminEmailResult.Error}");

                var babyAdmin = User.CreateAdminUser(
                    babyTenant.Id,
                    babyOrg.Id,
                    babyBu.Id,
                    "Admin Baby Turismo",
                    babyAdminEmailResult.Value!,
                    passwordService.HashPassword(tenantAdminPassword),
                    UserRole.TenantAdmin);

                await context.Users.AddRangeAsync(sysAdmin, babyAdmin);
                await context.SaveChangesAsync();

                logger.LogInformation("Database seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            throw;
        }
    }
}
