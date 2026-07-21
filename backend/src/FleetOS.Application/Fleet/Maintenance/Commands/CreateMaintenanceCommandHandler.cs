using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.Maintenance;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Maintenance.Commands;

internal sealed class CreateMaintenanceCommandHandler : IRequestHandler<CreateMaintenanceCommand, Result<Guid>>
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public CreateMaintenanceCommandHandler(
        IMaintenanceRepository maintenanceRepository,
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _maintenanceRepository = maintenanceRepository;
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(CreateMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle is null)
            return Result.Failure<Guid>(Error.NotFound("Vehicle.NotFound", "Vehicle not found."));

        var result = MaintenanceRecord.Create(
            _tenantContext.TenantId,
            _tenantContext.OrganizationId,
            _tenantContext.BusinessUnitId,
            request.VehicleId,
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
            return Result.Failure<Guid>(result.Error);

        var record = result.Value!;

        await _maintenanceRepository.AddAsync(record, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyMaintenanceCreatedAsync(record.Id, cancellationToken);

        return Result.Success(record.Id);
    }
}
