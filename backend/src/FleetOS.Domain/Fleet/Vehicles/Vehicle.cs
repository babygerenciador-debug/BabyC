using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Fleet.Vehicles;

/// <summary>
/// Vehicle aggregate root representing a bus/vehicle in the fleet.
/// Includes document tracking, fuel alerts and operational status.
/// </summary>
public sealed class Vehicle : AggregateRoot
{
    private readonly List<VehicleDocument> _documents = new();

    private Vehicle() { } // EF Core

    private Vehicle(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        string licensePlate,
        string? chassi,
        string nickname,
        string? brand,
        string? color,
        string? renavam,
        string? anttNumber,
        int? capacity,
        int? year,
        string? model)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        LicensePlate = licensePlate;
        Chassi       = chassi;
        Nickname     = nickname;
        Brand        = brand;
        Color        = color;
        Renavam      = renavam;
        AnttNumber   = anttNumber;
        Capacity     = capacity;
        Year         = year;
        Model        = model;
        Status       = VehicleStatus.Available;
        AssignedDriverId = null;
        CreatedAt    = DateTimeOffset.UtcNow;
    }

    // ─── Identity ─────────────────────────────────────────────────────
    public string  LicensePlate { get; private set; } = default!;
    public string? Chassi       { get; private set; }
    public string  Nickname     { get; private set; } = default!;
    public string? Brand        { get; private set; }
    public string? Color        { get; private set; }
    public string? Model        { get; private set; }
    public int?    Capacity     { get; private set; }
    public int?    Year         { get; private set; }
    public string? PhotoUrl     { get; private set; }

    // ─── Status ───────────────────────────────────────────────────────
    public VehicleStatus Status { get; private set; }
    public Guid? AssignedDriverId { get; private set; }

    // ─── Documentation ────────────────────────────────────────────────
    public string?   Renavam        { get; private set; }
    public string?   AnttNumber     { get; private set; }
    public DateTime? AnttExpiry     { get; private set; }
    public DateTime? ArtespExpiry   { get; private set; }
    public DateTime? InsuranceExpiry  { get; private set; }
    public DateTime? LicensingExpiry  { get; private set; }

    // ─── Fuel Tracking ────────────────────────────────────────────────
    public FuelAlertMode? FuelAlertMode  { get; private set; }
    public int?           FuelAlertDays { get; private set; }
    public DateTimeOffset? LastFuelAt   { get; private set; }
    public decimal?       CurrentOdometerKm { get; private set; }

    public IReadOnlyCollection<VehicleDocument> Documents => _documents.AsReadOnly();

    // ─── Factory ──────────────────────────────────────────────────────
    public static Result<Vehicle> Create(
        Guid   tenantId,
        Guid   organizationId,
        Guid   businessUnitId,
        string licensePlate,
        string? chassi,
        string nickname,
        string? brand,
        string? color,
        string? renavam,
        string? anttNumber,
        int? capacity,
        int? year,
        string? model)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            return Result.Failure<Vehicle>(Error.Validation("Vehicle.LicensePlateRequired", "License plate is required."));

        if (string.IsNullOrWhiteSpace(nickname))
            return Result.Failure<Vehicle>(Error.Validation("Vehicle.NicknameRequired", "Vehicle nickname is required."));

        if (capacity.HasValue && capacity <= 0)
            return Result.Failure<Vehicle>(Error.Validation("Vehicle.InvalidCapacity", "Capacity must be greater than zero."));

        var vehicle = new Vehicle(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            licensePlate.ToUpperInvariant().Trim(),
            chassi?.ToUpperInvariant().Trim(),
            nickname.Trim(),
            brand,
            color,
            renavam,
            anttNumber,
            capacity,
            year,
            model);

        return Result.Success(vehicle);
    }

    // ─── Update ───────────────────────────────────────────────────────
    public Result Update(
        string  nickname,
        string? chassi,
        string? brand,
        string? color,
        string? renavam,
        string? anttNumber,
        int?    capacity,
        int?    year,
        string? model,
        string? photoUrl)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            return Result.Failure(Error.Validation("Vehicle.NicknameRequired", "Vehicle nickname is required."));

        if (capacity.HasValue && capacity <= 0)
            return Result.Failure(Error.Validation("Vehicle.InvalidCapacity", "Capacity must be greater than zero."));

        Nickname   = nickname.Trim();
        Chassi     = chassi?.ToUpperInvariant().Trim();
        Brand      = brand;
        Color      = color;
        Renavam    = renavam;
        AnttNumber = anttNumber;
        Capacity   = capacity;
        Year       = year;
        Model      = model;
        PhotoUrl   = photoUrl;
        UpdatedAt  = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    // ─── Document Expiries ────────────────────────────────────────────
    public void UpdateDocumentExpiries(
        DateTime? anttExpiry,
        DateTime? artespExpiry,
        DateTime? insuranceExpiry,
        DateTime? licensingExpiry)
    {
        AnttExpiry      = anttExpiry;
        ArtespExpiry    = artespExpiry;
        InsuranceExpiry = insuranceExpiry;
        LicensingExpiry = licensingExpiry;
        UpdatedAt       = DateTimeOffset.UtcNow;
    }

    // ─── Fuel ─────────────────────────────────────────────────────────
    public void ConfigureFuelAlert(FuelAlertMode mode, int alertDays)
    {
        FuelAlertMode = mode;
        FuelAlertDays = alertDays;
        UpdatedAt     = DateTimeOffset.UtcNow;
    }

    public void RecordFueling(DateTimeOffset fueledAt, decimal odometerKm)
    {
        LastFuelAt        = fueledAt;
        CurrentOdometerKm = odometerKm;
        UpdatedAt         = DateTimeOffset.UtcNow;
    }

    // ─── Status & Driver ──────────────────────────────────────────────
    public void UpdateStatus(VehicleStatus status)
    {
        Status    = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AssignDriver(Guid driverId)
    {
        AssignedDriverId = driverId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ReleaseDriver()
    {
        AssignedDriverId = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // ─── Photo ────────────────────────────────────────────────────────
    public void SetPhoto(string photoUrl)
    {
        PhotoUrl  = photoUrl;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    // ─── Documents ────────────────────────────────────────────────────
    public Result AddDocument(string name, DateTime expirationDate, string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("VehicleDocument.NameRequired", "Document name is required."));

        var doc = new VehicleDocument(Guid.Empty, Id, name, expirationDate, fileUrl);
        _documents.Add(doc);

        return Result.Success();
    }

    public Result RemoveDocument(Guid documentId)
    {
        var doc = _documents.FirstOrDefault(d => d.Id == documentId);
        if (doc is null)
            return Result.Failure(Error.NotFound("VehicleDocument", documentId));

        _documents.Remove(doc);
        return Result.Success();
    }

    // ─── Soft Delete ──────────────────────────────────────────────────
    public void Delete()
    {
        Status = VehicleStatus.OutOfService;
        SoftDelete(Guid.Empty);
    }

    // ─── Availability check ───────────────────────────────────────────
    public bool IsAvailableForTrip => Status == VehicleStatus.Available;
}

public enum VehicleStatus
{
    Available       = 0,
    InTrip          = 1,
    InMaintenance   = 2,
    OutOfService    = 3
}

public enum FuelAlertMode
{
    SinceLastFuel = 0,  // Alert X days after last fuel
    FixedCycle    = 1   // Alert every X days regardless
}
