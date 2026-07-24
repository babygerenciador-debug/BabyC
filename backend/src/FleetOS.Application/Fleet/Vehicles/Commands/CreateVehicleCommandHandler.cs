using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Users;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace FleetOS.Application.Fleet.Vehicles.Commands;

internal sealed class CreateVehicleCommandHandler
    : IRequestHandler<CreateVehicleCommand, Result<Guid>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUserRepository    _userRepository;
    private readonly IDriverRepository  _driverRepository;
    private readonly IUnitOfWork        _unitOfWork;
    private readonly ITenantContext     _tenantContext;
    private readonly IFleetNotificationService _notificationService;
    private readonly ILogger<CreateVehicleCommandHandler> _logger;

    public CreateVehicleCommandHandler(
        IVehicleRepository vehicleRepository,
        IUserRepository    userRepository,
        IDriverRepository  driverRepository,
        IUnitOfWork        unitOfWork,
        ITenantContext     tenantContext,
        IFleetNotificationService notificationService,
        ILogger<CreateVehicleCommandHandler> logger)
    {
        _vehicleRepository = vehicleRepository;
        _userRepository    = userRepository;
        _driverRepository  = driverRepository;
        _unitOfWork        = unitOfWork;
        _tenantContext     = tenantContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
        CreateVehicleCommand request,
        CancellationToken    cancellationToken)
    {
        _logger.LogInformation("CreateVehicle: Plate={Plate}, Nickname={Nick}, DriverCpf={Cpf}, DriverId={DriverId}",
            request.LicensePlate, request.Nickname, request.DriverCpf, request.DriverId);

        // ── Uniqueness checks ──────────────────────────────────────────
        var existingByPlate = await _vehicleRepository.GetByLicensePlateAsync(
            request.LicensePlate, cancellationToken);
        if (existingByPlate != null)
            return Result.Failure<Guid>(Error.Validation(
                "Vehicle.LicensePlateConflict",
                "License plate already exists in this company."));

        if (!string.IsNullOrWhiteSpace(request.Chassi))
        {
            var existingByChassi = await _vehicleRepository.GetByChassiAsync(
                request.Chassi, cancellationToken);
            if (existingByChassi != null)
                return Result.Failure<Guid>(Error.Validation(
                    "Vehicle.ChassiConflict",
                    "Chassi already exists in this company."));
        }

        // ── Create entity ──────────────────────────────────────────────
        var vehicleResult = Vehicle.Create(
            _tenantContext.TenantId,
            _tenantContext.OrganizationId,
            _tenantContext.BusinessUnitId,
            request.LicensePlate,
            NormalizeNull(request.Chassi),
            request.Nickname,
            NormalizeNull(request.Brand),
            NormalizeNull(request.Color),
            NormalizeNull(request.Renavam),
            NormalizeNull(request.AnttNumber),
            request.Capacity,
            request.Year,
            NormalizeNull(request.Model));

        if (vehicleResult.IsFailure)
            return Result.Failure<Guid>(vehicleResult.Error);

        var vehicle = vehicleResult.Value!;

        // ── Document expiries ──────────────────────────────────────────
        if (request.AnttExpiry.HasValue || request.ArtespExpiry.HasValue
            || request.InsuranceExpiry.HasValue || request.LicensingExpiry.HasValue)
        {
            vehicle.UpdateDocumentExpiries(
                request.AnttExpiry,
                request.ArtespExpiry,
                request.InsuranceExpiry,
                request.LicensingExpiry);
        }

        // ── Fuel alert config ──────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(request.FuelAlertMode)
            && Enum.TryParse<FuelAlertMode>(request.FuelAlertMode, true, out var alertMode)
            && request.FuelAlertDays.HasValue)
        {
            vehicle.ConfigureFuelAlert(alertMode, request.FuelAlertDays.Value);
        }

        // ── Driver Assignment ──────────────────────────────────────────
        if (request.DriverId.HasValue)
        {
            var driver = await _driverRepository.GetByIdAsync(request.DriverId.Value, cancellationToken);
            if (driver == null)
                return Result.Failure<Guid>(Error.NotFound("Driver.NotFound", "Driver not found with the provided ID."));
            vehicle.AssignDriver(driver.Id);
            _logger.LogInformation("CreateVehicle: Assigned driver by ID={DriverId}", request.DriverId);
        }
        else if (!string.IsNullOrWhiteSpace(request.DriverCpf))
        {
            var cpfHash = HashCpf(request.DriverCpf);
            
            var user = await _userRepository.GetByCpfHashAsync(_tenantContext.TenantId, cpfHash, cancellationToken);
            if (user == null)
                return Result.Failure<Guid>(Error.NotFound("Driver.NotFound", "No user found with the provided CPF."));

            var driver = await _driverRepository.GetByUserIdAsync(user.Id, cancellationToken);
            if (driver == null)
                return Result.Failure<Guid>(Error.NotFound("Driver.NotFound", "The user with the provided CPF is not a registered driver."));

            vehicle.AssignDriver(driver.Id);
            _logger.LogInformation("CreateVehicle: Assigned driver by CPF, DriverId={DriverId}", driver.Id);
        }

        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        var saved = await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);
        _logger.LogInformation("CreateVehicle: Saved {Count} rows, VehicleId={VehicleId}", saved, vehicle.Id);

        await _notificationService.NotifyVehicleCreatedAsync(vehicle.Id, cancellationToken);

        return Result.Success(vehicle.Id);
    }

    private static string? NormalizeNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string HashCpf(string cpf)
    {
        var normalized = new string(cpf.Where(char.IsDigit).ToArray());
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToBase64String(bytes);
    }
}