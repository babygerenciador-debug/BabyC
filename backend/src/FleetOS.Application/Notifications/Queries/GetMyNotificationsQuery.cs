using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Notifications.Queries;

public record NotificationDto(Guid Id, string Title, string Message, string Type, DateTimeOffset CreatedAt);

public record GetMyNotificationsQuery : IRequest<Result<IReadOnlyList<NotificationDto>>>;

internal sealed class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ITenantContext _tenantContext;

    public GetMyNotificationsQueryHandler(INotificationRepository notificationRepository, ITenantContext tenantContext)
    {
        _notificationRepository = notificationRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _tenantContext.UserId;
        var role = _tenantContext.UserRole.ToString();

        var notifications = await _notificationRepository.GetUnreadNotificationsAsync(userId, new[] { role }, cancellationToken);

        var dtos = notifications.Select(n => new NotificationDto(
            n.Id,
            n.Title,
            n.Message,
            n.Type.ToString(),
            n.CreatedAt
        )).ToList();

        return Result.Success<IReadOnlyList<NotificationDto>>(dtos);
    }
}
