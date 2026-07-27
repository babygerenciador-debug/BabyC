using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Core.Tenants;
using FleetOS.Domain.Core.Users;
using FleetOS.Domain.Finance;
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
            await context.Database.OpenConnectionAsync();
            context.SetTenantId(Guid.Empty);

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

                // 7. Seed initial FinancialMonth for Baby Turismo
                var now = DateTime.UtcNow;
                var initialMonth = FinancialMonth.Open(
                    babyTenant.Id, babyOrg.Id, babyBu.Id,
                    now.Year, now.Month, babyTenant.OwnerSalary);
                await context.Set<FinancialMonth>().AddAsync(initialMonth);

                logger.LogInformation("Database seeded successfully.");
            }

            // Always seed initial month if none exists
            var babyTenantForMonth = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == "babyturismo");
            if (babyTenantForMonth != null)
            {
                if (!await context.Set<FinancialMonth>().AnyAsync(m => m.TenantId == babyTenantForMonth.Id))
                {
                    var org = await context.Organizations.FirstOrDefaultAsync(o => o.TenantId == babyTenantForMonth.Id);
                    var bu = await context.BusinessUnits.FirstOrDefaultAsync(b => b.TenantId == babyTenantForMonth.Id);
                    var now2 = DateTime.UtcNow;
                    var month = FinancialMonth.Open(
                        babyTenantForMonth.Id, org?.Id ?? Guid.Empty, bu?.Id ?? Guid.Empty,
                        now2.Year, now2.Month, babyTenantForMonth.OwnerSalary);
                    await context.Set<FinancialMonth>().AddAsync(month);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Initial FinancialMonth seeded for tenant {Slug}.", babyTenantForMonth.Slug);
                }

                context.SetTenantId(babyTenantForMonth.Id);
            }

            await SeedAdditionalUsersAsync(context, passwordService, logger, configuration);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            throw;
        }
    }

    private static async Task SeedAdditionalUsersAsync(
        FleetOsDbContext context,
        IPasswordService passwordService,
        ILogger logger,
        IConfiguration? configuration = null)
    {
        var added = false;

        // Extra users are configured via environment variables, not hardcoded
        // Set Seed__ExtraUserEmail, Seed__ExtraUserPassword, Seed__ExtraUserName in .env or Render
        var extraUserEmail = configuration?["Seed:ExtraUserEmail"];
        var extraUserPassword = configuration?["Seed:ExtraUserPassword"];
        var extraUserName = configuration?["Seed:ExtraUserName"];

        if (!string.IsNullOrEmpty(extraUserEmail) && !string.IsNullOrEmpty(extraUserPassword))
        {
            if (!await context.Users.IgnoreQueryFilters().AnyAsync(x => x.EmailAddress == extraUserEmail))
            {
                var emailResult = Email.Create(extraUserEmail);
                if (emailResult.IsSuccess && emailResult.Value is not null)
                {
                    var babyTenant = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == "babyturismo");
                    if (babyTenant != null)
                    {
                        var org = await context.Organizations.FirstOrDefaultAsync(o => o.TenantId == babyTenant.Id);
                        var bu = await context.BusinessUnits.FirstOrDefaultAsync(b => b.TenantId == babyTenant.Id);

                        var user = User.CreateAdminUser(
                            babyTenant.Id,
                            org?.Id ?? Guid.Empty,
                            bu?.Id ?? Guid.Empty,
                            extraUserName ?? "Extra Admin",
                            emailResult.Value,
                            passwordService.HashPassword(extraUserPassword),
                            UserRole.TenantAdmin);

                        await context.Users.AddAsync(user);
                        added = true;
                        logger.LogInformation("Additional user seeded: {Email}", extraUserEmail);
                    }
                }
            }
        }

        if (added) await context.SaveChangesAsync();
    }
}
