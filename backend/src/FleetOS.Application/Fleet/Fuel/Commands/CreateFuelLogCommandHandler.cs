using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.Fuel;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Fuel.Commands;

internal sealed class CreateFuelLogCommandHandler : IRequestHandler<CreateFuelLogCommand, Result<Guid>>
{
    private readonly IFuelLogRepository _fuelLogRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public CreateFuelLogCommandHandler(
        IFuelLogRepository fuelLogRepository,
        IVehicleRepository vehicleRepository,
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _fuelLogRepository = fuelLogRepository;
        _vehicleRepository = vehicleRepository;
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(CreateFuelLogCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle is null)
            return Result.Failure<Guid>(Error.NotFound("Vehicle.NotFound", "Vehicle not found."));

        if (request.DriverId.HasValue)
        {
            var driver = await _driverRepository.GetByIdAsync(request.DriverId.Value, cancellationToken);
            if (driver is null)
                return Result.Failure<Guid>(Error.NotFound("Driver.NotFound", "Driver not found."));
        }

        var result = FuelLog.Create(
            _tenantContext.TenantId,
            _tenantContext.OrganizationId,
            _tenantContext.BusinessUnitId,
            request.VehicleId,
            request.DriverId,
            request.Date,
            request.Odometer,
            request.Liters,
            request.TotalCost,
            request.ReceiptUrl,
            request.Notes);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        var fuelLog = result.Value!;

        // Calculate average consumption if possible
        var lastLog = await _fuelLogRepository.GetLastFuelLogForVehicleAsync(request.VehicleId, cancellationToken);
        if (lastLog != null && request.Odometer > lastLog.Odometer)
        {
            var distance = request.Odometer - lastLog.Odometer;
            var consumption = distance / request.Liters;
            fuelLog.SetAverageConsumption(consumption);
        }

        await _fuelLogRepository.AddAsync(fuelLog, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyFuelLogCreatedAsync(fuelLog.Id, cancellationToken);

        return Result.Success(fuelLog.Id);
    }
}
