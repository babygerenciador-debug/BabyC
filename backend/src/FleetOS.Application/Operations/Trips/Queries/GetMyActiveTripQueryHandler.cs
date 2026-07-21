using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Queries;

internal sealed class GetMyActiveTripQueryHandler : IRequestHandler<GetMyActiveTripQuery, Result<TripDto?>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly ITripRepository _tripRepository;
    private readonly ITenantContext _tenantContext;

    public GetMyActiveTripQueryHandler(
        IDriverRepository driverRepository,
        ITripRepository tripRepository,
        ITenantContext tenantContext)
    {
        _driverRepository = driverRepository;
        _tripRepository = tripRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<TripDto?>> Handle(GetMyActiveTripQuery request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByUserIdAsync(_tenantContext.UserId, cancellationToken);
        if (driver is null)
            return Result.Success((TripDto?)null);

        var trips = await _tripRepository.GetAllAsync(cancellationToken);
        var activeTrip = trips
            .Where(t => t.DriverId == driver.Id && t.Status != Domain.Operations.Trips.TripStatus.Completed && t.Status != Domain.Operations.Trips.TripStatus.Cancelled)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefault();

        if (activeTrip is null)
            return Result.Success((TripDto?)null);

        var dto = new TripDto(
            activeTrip.Id,
            activeTrip.DriverId,
            string.Empty,
            activeTrip.VehicleId,
            string.Empty,
            activeTrip.Origin,
            activeTrip.Destination,
            activeTrip.ScheduledStartDate,
            activeTrip.ScheduledEndDate,
            activeTrip.TripValue,
            activeTrip.PaymentStatus.ToString(),
            activeTrip.Notes,
            activeTrip.ActualStartDate,
            activeTrip.ActualEndDate,
            activeTrip.ChecklistCompleted,
            activeTrip.ChecklistNotes,
            activeTrip.Status.ToString(),
            activeTrip.CreatedAt,
            activeTrip.UpdatedAt);

        return Result.Success((TripDto?)dto);
    }
}
