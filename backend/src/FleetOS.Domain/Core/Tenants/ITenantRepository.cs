using FleetOS.Domain.Common.Interfaces;

namespace FleetOS.Domain.Core.Tenants;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
