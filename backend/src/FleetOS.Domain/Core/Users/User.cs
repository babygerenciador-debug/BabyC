using FleetOS.Domain.Common;
using FleetOS.Domain.Common.ValueObjects;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Core.Users;

/// <summary>
/// User aggregate root representing an authenticated identity in FleetOS.
/// Can be an admin, manager, or driver.
/// USR-001 — Email must be unique within the Tenant.
/// USR-003 — Disabled user cannot login.
/// USR-006 — Only admins can create/disable users.
/// </summary>
public sealed class User : AggregateRoot
{
    private User() { } // EF Core

    private User(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        string name,
        Email email,
        string passwordHash,
        UserRole role)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        Name = name;
        EmailAddress = email.Value;
        PasswordHash = passwordHash;
        Role = role;
        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // ─── Identity ─────────────────────────────────────────────────────
    public string Name { get; private set; } = default!;
    public string EmailAddress { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }

    // ─── Driver-specific auth (CPF login) ─────────────────────────────
    public string? CpfHash { get; private set; }
    public string? CpfLast4 { get; private set; }
    public bool IsDriverAccount => Role == UserRole.Driver;

    // ─── Security ─────────────────────────────────────────────────────
    public int FailedLoginAttempts { get; private set; }
    public DateTimeOffset? LockedUntil { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }
    public DateTimeOffset? PasswordChangedAt { get; private set; }

    // ─── Preferences ──────────────────────────────────────────────────
    public string Language { get; private set; } = "pt-BR";
    public string Theme { get; private set; } = "dark";

    // ─── Refresh Tokens ───────────────────────────────────────────────
    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    // ─── Factory — Admin/Manager user (email+password) ───────────────
    public static User CreateAdminUser(
        Guid tenantId, Guid organizationId, Guid businessUnitId,
        string name, Email email, string passwordHash, UserRole role)
    {
        var user = new User(Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            name, email, passwordHash, role);
        user.RaiseDomainEvent(new UserCreatedEvent(user.Id, user.TenantId, user.EmailAddress));
        return user;
    }

    // ─── Factory — Driver user (CPF + global password) ───────────────
    public static User CreateDriverUser(
        Guid tenantId, Guid organizationId, Guid businessUnitId,
        string name, Email email, string passwordHash,
        string cpfHash, string cpfLast4)
    {
        var user = new User(Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            name, email, passwordHash, UserRole.Driver);
        user.CpfHash = cpfHash;
        user.CpfLast4 = cpfLast4;
        user.RaiseDomainEvent(new UserCreatedEvent(user.Id, user.TenantId, user.EmailAddress));
        return user;
    }

    // ─── Auth behaviors ───────────────────────────────────────────────
    public Result RecordLoginSuccess()
    {
        if (Status == UserStatus.Disabled)
            return Result.Failure(Error.Auth.UserDisabled);
        if (Status == UserStatus.Locked && LockedUntil > DateTimeOffset.UtcNow)
            return Result.Failure(Error.Auth.UserBlocked);

        FailedLoginAttempts = 0;
        LockedUntil = null;
        LastLoginAt = DateTimeOffset.UtcNow;
        if (Status == UserStatus.Locked) Status = UserStatus.Active;
        return Result.Success();
    }

    public Result RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            Status = UserStatus.Locked;
            LockedUntil = DateTimeOffset.UtcNow.AddMinutes(30);
            RaiseDomainEvent(new UserLockedEvent(Id, TenantId));
            return Result.Failure(Error.Auth.UserBlocked);
        }
        return Result.Failure(Error.Auth.InvalidCredentials);
    }

    public bool IsLockedOut()
        => Status == UserStatus.Locked && LockedUntil > DateTimeOffset.UtcNow;

    // ─── Password ─────────────────────────────────────────────────────
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordChangedAt = DateTimeOffset.UtcNow;
        // Invalidate all refresh tokens on password change (USR-005)
        _refreshTokens.Clear();
        RaiseDomainEvent(new PasswordChangedEvent(Id, TenantId));
    }

    // ─── Refresh Tokens ───────────────────────────────────────────────
    public RefreshToken AddRefreshToken(string token, DateTimeOffset expiresAt)
    {
        var rt = new RefreshToken(Guid.Empty, Id, token, expiresAt, TenantId, OrganizationId, BusinessUnitId);
        _refreshTokens.Add(rt);
        return rt;
    }

    public void RevokeRefreshToken(string token)
    {
        var rt = _refreshTokens.FirstOrDefault(t => t.Token == token);
        rt?.Revoke();
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var rt in _refreshTokens.Where(t => !t.IsRevoked))
            rt.Revoke();
    }

    // ─── Status ───────────────────────────────────────────────────────
    public void Disable()
    {
        Status = UserStatus.Disabled;
        RevokeAllRefreshTokens();
        RaiseDomainEvent(new UserDisabledEvent(Id, TenantId));
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        RaiseDomainEvent(new UserActivatedEvent(Id, TenantId));
    }

    public void UnlockManually()
    {
        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
        LockedUntil = null;
    }

    // ─── Profile ──────────────────────────────────────────────────────
    public void UpdateProfile(string name, string language, string theme)
    {
        Name = name;
        Language = language;
        Theme = theme;
    }
}

public enum UserStatus { Active, Locked, Disabled, Archived }
public enum UserRole
{
    SystemAdmin = 0,    // FleetOS platform admin (sees all tenants)
    TenantAdmin = 1,    // Company admin
    Manager = 2,        // BusinessUnit manager
    Driver = 3          // Driver (limited access)
}

// ─── Domain Events ────────────────────────────────────────────────────
public sealed record UserCreatedEvent(Guid UserId, Guid TenantId, string Email) : DomainEvent;
public sealed record UserLockedEvent(Guid UserId, Guid TenantId) : DomainEvent;
public sealed record UserActivatedEvent(Guid UserId, Guid TenantId) : DomainEvent;
public sealed record UserDisabledEvent(Guid UserId, Guid TenantId) : DomainEvent;
public sealed record PasswordChangedEvent(Guid UserId, Guid TenantId) : DomainEvent;
