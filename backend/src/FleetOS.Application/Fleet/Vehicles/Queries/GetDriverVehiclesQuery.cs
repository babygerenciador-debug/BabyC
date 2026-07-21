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
    private readonly ITenantContext _tenantContext;

    public GetDriverVehiclesQueryHandler(
        IVehicleRepository vehicleRepository,
        ITenantContext tenantContext)
    {
        _vehicleRepository = vehicleRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<List<DriverVehicleDto>>> Handle(GetDriverVehiclesQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _vehicleRepository.GetAllAsync(cancellationToken);
        var dtos = vehicles
            .Where(v => v.Status == Domain.Fleet.Vehicles.VehicleStatus.Available)
            .Select(v => new DriverVehicleDto(v.Id, v.LicensePlate, v.Nickname))
            .ToList();

        return Result.Success(dtos);
    }
}
