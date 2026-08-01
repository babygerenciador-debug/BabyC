using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.Fuel;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Fuel.Commands;

public sealed record CreateDriverFuelLogCommand(
    Guid VehicleId,
    int Odometer,
    decimal Liters,
    decimal TotalCost,
    DateTime Date) : IRequest<Result<Guid>>;

internal sealed class CreateDriverFuelLogCommandHandler : IRequestHandler<CreateDriverFuelLogCommand, Result<Guid>>
{
    private readonly IFuelLogRepository _fuelLogRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IFleetNotificationService _notificationService;

    public CreateDriverFuelLogCommandHandler(
        IFuelLogRepository fuelLogRepository,
        IVehicleRepository vehicleRepository,
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUser,
        IFleetNotificationService notificationService)
    {
        _fuelLogRepository = fuelLogRepository;
        _vehicleRepository = vehicleRepository;
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(CreateDriverFuelLogCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle is null)
            return Result.Failure<Guid>(Error.NotFound("Vehicle.NotFound", "Vehicle not found."));

        Guid? driverId = null;
        if (_currentUser.UserId.HasValue)
        {
            var driver = await _driverRepository.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
            if (driver is not null)
                driverId = driver.Id;
        }

        var result = FuelLog.Create(
            _tenantContext.TenantId,
            _tenantContext.OrganizationId,
            _tenantContext.BusinessUnitId,
            request.VehicleId,
            driverId,
            request.Date,
            request.Odometer,
            request.Liters,
            request.TotalCost,
            null,
            null);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        await _fuelLogRepository.AddAsync(result.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyFuelLogCreatedAsync(result.Value!.Id, cancellationToken);

        return Result.Success(result.Value!.Id);
    }
}
