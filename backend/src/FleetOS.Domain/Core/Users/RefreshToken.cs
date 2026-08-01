using FleetOS.Domain.Common;

namespace FleetOS.Domain.Core.Users;

/// <summary>Refresh Token entity for JWT session management.</summary>
public sealed class RefreshToken : Entity
{
    private RefreshToken() { } // EF Core

    public RefreshToken(Guid id, Guid userId, string token, DateTimeOffset expiresAt, Guid tenantId, Guid organizationId, Guid businessUnitId)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        IsRevoked = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string Token { get; private set; } = default!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke(string? replacedBy = null)
    {
        IsRevoked = true;
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByToken = replacedBy;
    }
}
