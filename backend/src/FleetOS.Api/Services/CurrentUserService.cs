using System.Security.Claims;
using FleetOS.Domain.Common.Interfaces;

namespace FleetOS.Api.Services;

/// <summary>
/// Reads the current authenticated user from the HTTP context claims.
/// Implements ICurrentUserService defined in FleetOS.Domain.Common.Interfaces.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            // Claim name must match what JwtService writes: "tenant_id"
            var tenantIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("tenant_id")?.Value;
            if (Guid.TryParse(tenantIdClaim, out var tenantId))
            {
                return tenantId;
            }
            return null;
        }
    }
}
