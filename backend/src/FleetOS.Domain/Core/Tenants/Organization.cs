using FleetOS.Domain.Common;

namespace FleetOS.Domain.Core.Tenants;

/// <summary>
/// Organization represents a legal entity (company) within a Tenant.
/// Example: Baby Turismo LTDA under FleetOS platform.
/// BR-0004 — Every Organization belongs to exactly one Tenant.
/// </summary>
public sealed class Organization : Entity
{
    private Organization() { } // EF Core

    private Organization(Guid id, Guid tenantId, string name, string? cnpj)
        : base(id, tenantId, id, Guid.Empty)
    {
        OrganizationId = id;
        Name = name;
        Cnpj = cnpj;
        Status = OrganizationStatus.Active;
    }

    public string Name { get; private set; } = default!;
    public string? Cnpj { get; private set; }
    public OrganizationStatus Status { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? ZipCode { get; private set; }
    public string? LogoUrl { get; private set; }

    // ─── Relationships ────────────────────────────────────────────────
    private readonly List<BusinessUnit> _businessUnits = [];
    public IReadOnlyList<BusinessUnit> BusinessUnits => _businessUnits.AsReadOnly();

    // ─── Factory ──────────────────────────────────────────────────────
    public static Organization Create(Guid tenantId, string name, string? cnpj = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Organization(Guid.NewGuid(), tenantId, name, cnpj);
    }

    // ─── Behaviors ───────────────────────────────────────────────────
    public void UpdateInfo(string name, string? phone, string? email, string? address,
        string? city, string? state, string? zipCode, string? logoUrl)
    {
        Name = name;
        Phone = phone;
        Email = email;
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        LogoUrl = logoUrl;
    }

    public void Disable() => Status = OrganizationStatus.Disabled;
    public void Enable() => Status = OrganizationStatus.Active;
}

public enum OrganizationStatus { Active, Disabled }
