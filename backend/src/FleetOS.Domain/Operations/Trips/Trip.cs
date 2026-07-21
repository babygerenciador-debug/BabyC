using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Operations.Trips;

/// <summary>
/// Trip aggregate root representing an actual executed trip.
/// </summary>
public sealed class Trip : AggregateRoot
{
    private Trip() { } // EF Core

    private Trip(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid driverId,
        Guid vehicleId,
        string origin,
        string destination,
        DateTime scheduledStartDate,
        DateTime scheduledEndDate,
        decimal tripValue,
        PaymentStatus paymentStatus,
        string? notes)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        DriverId = driverId;
        VehicleId = vehicleId;
        Origin = origin;
        Destination = destination;
        ScheduledStartDate = scheduledStartDate;
        ScheduledEndDate = scheduledEndDate;
        TripValue = tripValue;
        PaymentStatus = paymentStatus;
        Notes = notes;
        Status = TripStatus.Created;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid DriverId { get; private set; }
    public Guid VehicleId { get; private set; }
    public string Origin { get; private set; } = default!;
    public string Destination { get; private set; } = default!;
    public DateTime ScheduledStartDate { get; private set; }
    public DateTime ScheduledEndDate { get; private set; }
    public string? Notes { get; private set; }
    public decimal TripValue { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }

    public DateTime? ActualStartDate { get; private set; }
    public DateTime? ActualEndDate { get; private set; }

    public bool ChecklistCompleted { get; private set; }
    public string? ChecklistNotes { get; private set; }

    public TripStatus Status { get; private set; }

    public static Result<Trip> Create(
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid driverId,
        Guid vehicleId,
        string origin,
        string destination,
        DateTime scheduledStartDate,
        DateTime scheduledEndDate,
        decimal tripValue,
        PaymentStatus paymentStatus,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(origin))
            return Result.Failure<Trip>(Error.Validation("Trip.OriginRequired", "Origin is required."));

        if (string.IsNullOrWhiteSpace(destination))
            return Result.Failure<Trip>(Error.Validation("Trip.DestinationRequired", "Destination is required."));

        if (scheduledStartDate >= scheduledEndDate)
            return Result.Failure<Trip>(Error.Validation("Trip.InvalidDates", "Scheduled end date must be after start date."));

        if (tripValue < 0)
            return Result.Failure<Trip>(Error.Validation("Trip.InvalidValue", "Trip value cannot be negative."));

        var trip = new Trip(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            driverId, vehicleId,
            origin.Trim(), destination.Trim(),
            scheduledStartDate, scheduledEndDate,
            tripValue, paymentStatus,
            notes?.Trim());

        return Result.Success(trip);
    }

    public Result MarkAsPaid()
    {
        if (PaymentStatus == PaymentStatus.Paid)
            return Result.Failure(Error.Validation("Trip.AlreadyPaid", "Trip is already paid."));

        PaymentStatus = PaymentStatus.Paid;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result StartTrip()
    {
        if (Status != TripStatus.Created)
            return Result.Failure(Error.Validation("Trip.InvalidStatus", "Only created trips can be started."));

        Status = TripStatus.InProgress;
        ActualStartDate = DateTime.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result CompleteTrip(bool checklistCompleted, string? checklistNotes)
    {
        if (Status != TripStatus.InProgress)
            return Result.Failure(Error.Validation("Trip.InvalidStatus", "Only in-progress trips can be completed."));

        if (!checklistCompleted)
            return Result.Failure(Error.Validation("Trip.ChecklistRequired", "Checklist must be completed to finish the trip."));

        ChecklistCompleted = checklistCompleted;
        ChecklistNotes = checklistNotes?.Trim();
        
        Status = TripStatus.Completed;
        ActualEndDate = DateTime.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result CancelTrip()
    {
        if (Status == TripStatus.Completed || Status == TripStatus.Cancelled)
            return Result.Failure(Error.Validation("Trip.InvalidStatus", "Completed or already cancelled trips cannot be cancelled."));

        Status = TripStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result SwapVehicle(Guid newVehicleId)
    {
        if (Status == TripStatus.Completed || Status == TripStatus.Cancelled)
            return Result.Failure(Error.Validation("Trip.InvalidStatus", "Cannot swap vehicle on completed or cancelled trips."));

        if (newVehicleId == Guid.Empty)
            return Result.Failure(Error.Validation("Trip.InvalidVehicle", "Vehicle ID is required."));

        if (newVehicleId == VehicleId)
            return Result.Failure(Error.Validation("Trip.SameVehicle", "New vehicle must be different from current vehicle."));

        VehicleId = newVehicleId;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }
}
