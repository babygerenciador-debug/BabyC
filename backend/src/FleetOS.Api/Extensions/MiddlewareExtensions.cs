using FleetOS.Domain.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace FleetOS.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (!context.Request.Headers.TryGetValue("X-Correlation-ID", out var correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                context.Request.Headers.Append("X-Correlation-ID", correlationId);
            }
            context.Response.Headers.Append("X-Correlation-ID", correlationId);
            await next();
        });
        return app;
    }

    public static IApplicationBuilder UseTenantResolver(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var tenantIdClaim = context.User.FindFirst("tenant_id");
                if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
                {
                    // For Global Query Filters
                    var dbContext = context.RequestServices.GetRequiredService<FleetOS.Infrastructure.Persistence.FleetOsDbContext>();
                    dbContext.SetTenantId(tenantId);
                }
            }
            await next();
        });
        return app;
    }

    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        return app;
    }
}

/// <summary>
/// TenantContext implementation that reads directly from HttpContext.User
/// </summary>
public sealed class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid TenantId => Guid.TryParse(User?.FindFirst("tenant_id")?.Value, out var id) ? id : Guid.Empty;
    public Guid OrganizationId => Guid.TryParse(User?.FindFirst("organization_id")?.Value, out var id) ? id : Guid.Empty;
    public Guid BusinessUnitId => Guid.TryParse(User?.FindFirst("business_unit_id")?.Value, out var id) ? id : Guid.Empty;
    public Guid UserId => Guid.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : Guid.Empty;
    
    public UserRoleContext UserRole => Enum.TryParse<UserRoleContext>(User?.FindFirst(ClaimTypes.Role)?.Value, out var role) 
        ? role 
        : UserRoleContext.Driver;
        
    public string? CorrelationId => httpContextAccessor.HttpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault();
}
