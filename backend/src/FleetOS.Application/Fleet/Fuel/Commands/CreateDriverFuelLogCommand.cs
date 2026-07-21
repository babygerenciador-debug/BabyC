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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateDriverFuelLogCommandHandler(
        IFuelLogRepository fuelLogRepository,
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _fuelLogRepository = fuelLogRepository;
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateDriverFuelLogCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle is null)
            return Result.Failure<Guid>(Error.NotFound("Vehicle.NotFound", "Vehicle not found."));

        var result = FuelLog.Create(
            _tenantContext.TenantId,
            _tenantContext.OrganizationId,
            _tenantContext.BusinessUnitId,
            request.VehicleId,
            null,
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

        return Result.Success(result.Value!.Id);
    }
}
