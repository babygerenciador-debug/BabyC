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
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (vehicle is null)
            return Result.Failure<VehicleDto>(Error.NotFound("Vehicle.NotFound", "Vehicle not found."));

        var dto = new VehicleDto(
            vehicle.Id,
            vehicle.LicensePlate,
            vehicle.Chassi,
            vehicle.Nickname,
            vehicle.Brand,
            vehicle.Color,
            vehicle.Model,
            vehicle.Capacity,
            vehicle.Year,
            vehicle.PhotoUrl,
            vehicle.Status.ToString(),
            vehicle.AssignedDriverId,
            vehicle.Renavam,
            vehicle.AnttNumber,
            vehicle.AnttExpiry,
            vehicle.ArtespExpiry,
            vehicle.InsuranceExpiry,
            vehicle.LicensingExpiry,
            vehicle.FuelAlertMode?.ToString(),
            vehicle.FuelAlertDays,
            vehicle.LastFuelAt,
            vehicle.CurrentOdometerKm,
            vehicle.CreatedAt
        );

        return Result.Success(dto);
    }
}
