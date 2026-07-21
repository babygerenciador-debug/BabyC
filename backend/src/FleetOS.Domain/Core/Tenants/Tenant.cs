using FleetOS.Domain.Common;

namespace FleetOS.Domain.Core.Tenants;

/// <summary>
/// Tenant represents a SaaS customer (company that contracted FleetOS).
/// All operational data belongs to a Tenant.
/// BR-0001, BR-0002 — Every record must have a TenantId.
/// </summary>
public sealed class Tenant : AggregateRoot
{
    private Tenant() { } // EF Core

    private Tenant(Guid id, string name, string slug) : base(id, id, Guid.Empty, Guid.Empty)
    {
        // TenantId = own Id (tenant is the root)
        TenantId = id;
        Name = name;
        Slug = slug;
        Status = TenantStatus.Active;
        Plan = TenantPlan.Trial;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public TenantStatus Status { get; private set; }
    public TenantPlan Plan { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? PrimaryColor { get; private set; }
    public string TimeZone { get; private set; } = "America/Sao_Paulo";
    public string Language { get; private set; } = "pt-BR";

    // ─── Relationships ────────────────────────────────────────────────
    private readonly List<Organization> _organizations = [];
    public IReadOnlyList<Organization> Organizations => _organizations.AsReadOnly();

    // ─── Factory ──────────────────────────────────────────────────────
    public static Tenant Create(string name, string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        var tenant = new Tenant(Guid.NewGuid(), name, slug.ToLowerInvariant().Trim());
        tenant.RaiseDomainEvent(new TenantCreatedEvent(tenant.Id, tenant.Name));
        return tenant;
    }

    // ─── Behaviors ───────────────────────────────────────────────────
    public void Suspend()
    {
        Status = TenantStatus.Suspended;
        RaiseDomainEvent(new TenantSuspendedEvent(Id));
    }

    public void Activate()
    {
        Status = TenantStatus.Active;
        RaiseDomainEvent(new TenantActivatedEvent(Id));
    }

    public void UpdateBranding(string? logoUrl, string? primaryColor)
    {
        LogoUrl = logoUrl;
        PrimaryColor = primaryColor;
    }

    public void SetPlan(TenantPlan plan) => Plan = plan;

    public void UpdateLocalization(string timeZone, string language)
    {
        TimeZone = timeZone;
        Language = language;
    }
}

public enum TenantStatus { Active, Suspended, Cancelled }
public enum TenantPlan { Trial, Starter, Professional, Enterprise }

// ─── Domain Events ────────────────────────────────────────────────────
public sealed record TenantCreatedEvent(Guid TenantId, string Name) : DomainEvent;
public sealed record TenantActivatedEvent(Guid TenantId) : DomainEvent;
public sealed record TenantSuspendedEvent(Guid TenantId) : DomainEvent;
