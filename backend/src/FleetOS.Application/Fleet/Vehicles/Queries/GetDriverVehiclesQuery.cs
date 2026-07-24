using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Vehicles.Queries;

public sealed record GetDriverVehiclesQuery() : IRequest<Result<List<DriverVehicleDto>>>;

public sealed record DriverVehicleDto(Guid Id, string LicensePlate, string Nickname);

internal sealed class GetDriverVehiclesQueryHandler : IRequestHandler<GetDriverVehiclesQuery, Result<List<DriverVehicleDto>>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITripRepository _tripRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly ITenantContext _tenantContext;

    public GetDriverVehiclesQueryHandler(
        IVehicleRepository vehicleRepository,
        ITripRepository tripRepository,
        IDriverRepository driverRepository,
        ITenantContext tenantContext)
    {
        _vehicleRepository = vehicleRepository;
        _tripRepository = tripRepository;
        _driverRepository = driverRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<DriverVehicleDto>>> Handle(GetDriverVehiclesQuery request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByUserIdAsync(_tenantContext.UserId, cancellationToken);
        if (driver is null)
            return Result.Success(new List<DriverVehicleDto>());

        var vehicles = await _vehicleRepository.GetAllAsync(cancellationToken);
        var result = new List<DriverVehicleDto>();
        var seen = new HashSet<Guid>();

        var assigned = vehicles.FirstOrDefault(v => v.AssignedDriverId == driver.Id);
        if (assigned is not null)
        {
            result.Add(new DriverVehicleDto(assigned.Id, assigned.LicensePlate, assigned.Nickname));
            seen.Add(assigned.Id);
        }

        var activeTrip = await _tripRepository.GetActiveTripByDriverIdAsync(driver.Id, cancellationToken);
        if (activeTrip is not null && seen.Add(activeTrip.VehicleId))
        {
            var tripVehicle = vehicles.FirstOrDefault(v => v.Id == activeTrip.VehicleId);
            if (tripVehicle is not null)
                result.Add(new DriverVehicleDto(tripVehicle.Id, tripVehicle.LicensePlate, tripVehicle.Nickname));
        }

        return Result.Success(result);
    }
}
