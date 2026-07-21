using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Commands;

internal sealed class SwapTripVehicleCommandHandler : IRequestHandler<SwapTripVehicleCommand, Result>
{
    private readonly ITripRepository _tripRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public SwapTripVehicleCommandHandler(
        ITripRepository tripRepository,
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _tripRepository = tripRepository;
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(SwapTripVehicleCommand request, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetByIdAsync(request.TripId, cancellationToken);
        if (trip is null)
            return Result.Failure(Error.NotFound("Trip.NotFound", "Trip not found."));

        var vehicle = await _vehicleRepository.GetByIdAsync(request.NewVehicleId, cancellationToken);
        if (vehicle is null)
            return Result.Failure(Error.NotFound("Vehicle.NotFound", "New vehicle not found."));

        if (!vehicle.IsAvailableForTrip)
            return Result.Failure(Error.Validation("Vehicle.Unavailable", "Vehicle is not available for trips."));

        var result = trip.SwapVehicle(request.NewVehicleId);
        if (result.IsFailure)
            return result;

        _tripRepository.Update(trip);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyTripVehicleSwappedAsync(trip.Id, request.NewVehicleId, cancellationToken);

        return Result.Success();
    }
}
