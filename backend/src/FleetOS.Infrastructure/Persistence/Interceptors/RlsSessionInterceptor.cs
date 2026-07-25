using System.Data.Common;
using FleetOS.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace FleetOS.Infrastructure.Persistence.Interceptors;

public sealed class RlsSessionInterceptor : DbConnectionInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public RlsSessionInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection, ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId?.ToString() ?? Guid.Empty.ToString();
        var userId = _currentUserService.UserId?.ToString() ?? Guid.Empty.ToString();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT set_config('app.current_tenant_id', @p0, false),
                   set_config('app.current_user_id', @p1, false)
            """;
        cmd.Parameters.Add(new NpgsqlParameter("p0", tenantId));
        cmd.Parameters.Add(new NpgsqlParameter("p1", userId));
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        var tenantId = _currentUserService.TenantId?.ToString() ?? Guid.Empty.ToString();
        var userId = _currentUserService.UserId?.ToString() ?? Guid.Empty.ToString();

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT set_config('app.current_tenant_id', @p0, false),
                   set_config('app.current_user_id', @p1, false)
            """;
        cmd.Parameters.Add(new NpgsqlParameter("p0", tenantId));
        cmd.Parameters.Add(new NpgsqlParameter("p1", userId));
        cmd.ExecuteNonQuery();
    }
}
