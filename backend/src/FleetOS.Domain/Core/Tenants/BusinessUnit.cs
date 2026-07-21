using FleetOS.Domain.Common;

namespace FleetOS.Domain.Core.Tenants;

/// <summary>
/// BusinessUnit represents the operational unit (branch/garage) where operations happen.
/// All vehicles, drivers, trips, inventory and finance belong to a BusinessUnit.
/// BR-0003 — Every BusinessUnit belongs to exactly one Organization.
/// BU-001 — Every operational record must belong to a BusinessUnit.
/// </summary>
public sealed class BusinessUnit : AggregateRoot
{
    private BusinessUnit() { } // EF Core

    private BusinessUnit(Guid id, Guid tenantId, Guid organizationId, string name, string code)
        : base(id, tenantId, organizationId, id)
    {
        BusinessUnitId = id;
        Name = name;
        Code = code;
        Status = BusinessUnitStatus.Active;
        IsHeadOffice = false;
    }

    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;  // Unique within Organization
    public BusinessUnitStatus Status { get; private set; }
    public bool IsHeadOffice { get; private set; }

    // ─── Address ─────────────────────────────────────────────────────
    public string? ZipCode { get; private set; }
    public string? Street { get; private set; }
    public string? Number { get; private set; }
    public string? District { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }

    // ─── Contact ─────────────────────────────────────────────────────
    public string? Phone { get; private set; }
    public string? Email { get; private set; }

    // ─── Config ──────────────────────────────────────────────────────
    public string TimeZone { get; private set; } = "America/Sao_Paulo";

    // ─── Factory ──────────────────────────────────────────────────────
    public static BusinessUnit Create(Guid tenantId, Guid organizationId, string name, string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        return new BusinessUnit(Guid.NewGuid(), tenantId, organizationId, name, code.ToUpperInvariant());
    }

    // ─── Behaviors ───────────────────────────────────────────────────
    public void Rename(string name) => Name = name;

    public void UpdateAddress(string? zipCode, string? street, string? number,
        string? district, string? city, string? state)
    {
        ZipCode = zipCode;
        Street = street;
        Number = number;
        District = district;
        City = city;
        State = state;
    }

    public void UpdateContact(string? phone, string? email)
    {
        Phone = phone;
        Email = email;
    }

    public void Suspend()
    {
        Status = BusinessUnitStatus.Suspended;
        RaiseDomainEvent(new BusinessUnitSuspendedEvent(Id, TenantId));
    }

    public void Archive()
    {
        Status = BusinessUnitStatus.Archived;
        RaiseDomainEvent(new BusinessUnitArchivedEvent(Id, TenantId));
    }

    public void Activate()
    {
        Status = BusinessUnitStatus.Active;
        RaiseDomainEvent(new BusinessUnitActivatedEvent(Id, TenantId));
    }

    public void SetAsHeadOffice() => IsHeadOffice = true;
    public void UnsetHeadOffice() => IsHeadOffice = false;
}

public enum BusinessUnitStatus { Active, Suspended, Archived }

// ─── Domain Events ────────────────────────────────────────────────────
public sealed record BusinessUnitActivatedEvent(Guid BusinessUnitId, Guid TenantId) : DomainEvent;
public sealed record BusinessUnitSuspendedEvent(Guid BusinessUnitId, Guid TenantId) : DomainEvent;
public sealed record BusinessUnitArchivedEvent(Guid BusinessUnitId, Guid TenantId) : DomainEvent;
