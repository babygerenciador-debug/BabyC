namespace FleetOS.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// Contains Id, multi-tenant context, audit fields and soft delete.
/// Every entity MUST inherit from this class.
/// </summary>
public abstract class Entity
{
    protected Entity() { }

    protected Entity(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId)
    {
        Id = id;
        TenantId = tenantId;
        OrganizationId = organizationId;
        BusinessUnitId = businessUnitId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // ─── Identity ────────────────────────────────────────────────────
    public Guid Id { get; protected set; }

    // ─── Multi-Tenant Context ────────────────────────────────────────
    public Guid TenantId { get; protected set; }
    public Guid OrganizationId { get; protected set; }
    public Guid BusinessUnitId { get; protected set; }

    // ─── Audit Fields ─────────────────────────────────────────────────
    public DateTimeOffset CreatedAt { get; protected set; }
    public Guid? CreatedBy { get; protected set; }
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }

    // ─── Soft Delete ─────────────────────────────────────────────────
    public DateTimeOffset? DeletedAt { get; protected set; }
    public Guid? DeletedBy { get; protected set; }
    public bool IsDeleted => DeletedAt.HasValue;

    // ─── Concurrency ─────────────────────────────────────────────────
    public uint RowVersion { get; protected set; }

    // ─── Audit methods (called by EF interceptor) ────────────────────
    public void SetCreatedBy(Guid userId)
    {
        CreatedBy = userId;
    }

    public void SetUpdatedBy(Guid userId)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = userId;
    }

    public void SoftDelete(Guid deletedBy)
    {
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    // ─── Equality ────────────────────────────────────────────────────
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);
    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}
