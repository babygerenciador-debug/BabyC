using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Maintenance.Commands;

internal sealed class UpdateMaintenanceCommandHandler : IRequestHandler<UpdateMaintenanceCommand, Result>
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public UpdateMaintenanceCommandHandler(
        IMaintenanceRepository maintenanceRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _maintenanceRepository = maintenanceRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(UpdateMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var record = await _maintenanceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (record is null)
            return Result.Failure(Error.NotFound("MaintenanceRecord.NotFound", "Maintenance record not found."));

        var result = record.Update(
            request.Type,
            request.Status,
            request.Date,
            request.Odometer,
            request.Description,
            request.TotalCost,
            request.ProviderName,
            request.InvoiceUrl,
            request.Notes);

        if (result.IsFailure)
            return result;
        
        _maintenanceRepository.Update(record);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyMaintenanceUpdatedAsync(record.Id, cancellationToken);

        return Result.Success();
    }
}
