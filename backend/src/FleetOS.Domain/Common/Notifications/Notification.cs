using FleetOS.Shared.Results;

namespace FleetOS.Domain.Common.Notifications;

public sealed class Notification : Entity
{
    private Notification() { } // EF Core

    private Notification(Guid id, Guid tenantId, Guid organizationId, Guid businessUnitId, Guid? userId, string? role, string title, string message, NotificationType type)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        UserId = userId;
        Role = role;
        Title = title;
        Message = message;
        Type = type;
        IsRead = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid? UserId { get; private set; } // Null if targeted to a Role
    public string? Role { get; private set; } // Null if targeted to a specific UserId
    public string Title { get; private set; } = default!;
    public string Message { get; private set; } = default!;
    public bool IsRead { get; private set; }
    public NotificationType Type { get; private set; }

    public static Result<Notification> Create(Guid tenantId, Guid organizationId, Guid businessUnitId, Guid? userId, string? role, string title, string message, NotificationType type = NotificationType.Info)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Notification>(Error.Validation("Notification.TitleRequired", "Title is required."));
            
        if (string.IsNullOrWhiteSpace(message))
            return Result.Failure<Notification>(Error.Validation("Notification.MessageRequired", "Message is required."));

        if (userId == null && string.IsNullOrWhiteSpace(role))
            return Result.Failure<Notification>(Error.Validation("Notification.TargetRequired", "Either UserId or Role must be provided."));

        var notification = new Notification(Guid.NewGuid(), tenantId, organizationId, businessUnitId, userId, role, title.Trim(), message.Trim(), type);
        return Result.Success(notification);
    }

    public void MarkAsRead()
    {
        IsRead = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
