using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FleetOS.Infrastructure.Hubs;

[Authorize]
public class FleetHub : Hub
{
    private readonly ILogger<FleetHub> _logger;

    public FleetHub(ILogger<FleetHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
        _logger.LogInformation("SignalR connected: ConnectionId={ConnectionId}, TenantId={TenantId}", Context.ConnectionId, tenantId);

        if (!string.IsNullOrEmpty(tenantId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tenantId = Context.User?.FindFirst("tenant_id")?.Value;
        _logger.LogInformation(exception, "SignalR disconnected: ConnectionId={ConnectionId}, TenantId={TenantId}, Reason={Reason}",
            Context.ConnectionId, tenantId, exception?.Message ?? "Client disconnected");

        if (!string.IsNullOrEmpty(tenantId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Tenant_{tenantId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
