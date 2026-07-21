using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Notifications.Commands;

public record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest<Result>;

internal sealed class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(INotificationRepository notificationRepository, IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);
        
        if (notification == null)
            return Result.Failure(Error.NotFound("Notification.NotFound", "Notification not found."));

        notification.MarkAsRead();
        _notificationRepository.Update(notification);
        
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
