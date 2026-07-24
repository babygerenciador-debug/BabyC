using FleetOS.Application.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Vehicles.Queries;

internal sealed class GetVehicleByIdQueryHandler : IRequestHandler<GetVehicleByIdQuery, Result<VehicleDto>>
{
    private readonly IVehicleRepository _vehicleRepository;

    public GetVehicleByIdQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Result<VehicleDto>> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _vehicleRepository.GetVehicleByIdWithDriverAsync(request.Id, cancellationToken);
        if (dto is null)
            return Result.Failure<VehicleDto>(Error.NotFound("Vehicle.NotFound", "Vehicle not found."));

        return Result.Success(dto);
    }
}
