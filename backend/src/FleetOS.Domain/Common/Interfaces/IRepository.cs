using FleetOS.Shared.Pagination;

namespace FleetOS.Domain.Common.Interfaces;

/// <summary>Generic repository interface following the Repository Pattern.</summary>
public interface IRepository<T> where T : Entity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity); // Soft delete — sets DeletedAt
}

/// <summary>Unit of Work interface for transaction management.</summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task<int> CommitAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<int> CommitAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>Tenant context injected into every request.</summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    Guid OrganizationId { get; }
    Guid BusinessUnitId { get; }
    Guid UserId { get; }
    UserRoleContext UserRole { get; }
    string? CorrelationId { get; }
}

/// <summary>
/// Service to get the current authenticated user ID from the HTTP context.
/// Defined here (Domain layer) to respect DIP: Infrastructure depends on Domain, not the other way.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
}

public enum UserRoleContext { SystemAdmin, TenantAdmin, Manager, Driver }

