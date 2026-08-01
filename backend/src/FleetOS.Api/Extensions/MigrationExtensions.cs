using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FleetOS.Api.Extensions;

public static class MigrationExtensions
{
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await Infrastructure.Persistence.DbInitializer.InitializeAsync(scope.ServiceProvider);
    }
}
