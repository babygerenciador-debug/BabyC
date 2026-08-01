using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Common.Notifications;

namespace FleetOS.Application.Common.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetUnreadNotificationsAsync(Guid? userId, string[] roles, CancellationToken cancellationToken = default);
}
