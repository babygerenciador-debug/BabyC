using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Users;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Shared.Results;
using MediatR;
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

    public CreateVehicleCommandHandler(
        IVehicleRepository vehicleRepository,
        IUserRepository    userRepository,
        IDriverRepository  driverRepository,
        IUnitOfWork        unitOfWork,
        ITenantContext     tenantContext,
        IFleetNotificationService notificationService)
    {
        _vehicleRepository = vehicleRepository;
        _userRepository    = userRepository;
        _driverRepository  = driverRepository;
        _unitOfWork        = unitOfWork;
        _tenantContext     = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(
        CreateVehicleCommand request,
        CancellationToken    cancellationToken)
    {
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
            request.Chassi,
            request.Nickname,
            request.Brand,
            request.Color,
            request.Renavam,
            request.AnttNumber,
            request.Capacity,
            request.Year,
            request.Model);

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
        if (!string.IsNullOrWhiteSpace(request.DriverCpf))
        {
            var cpfHash = HashCpf(request.DriverCpf);
            
            var user = await _userRepository.GetByCpfHashAsync(_tenantContext.TenantId, cpfHash, cancellationToken);
            if (user == null)
                return Result.Failure<Guid>(Error.NotFound("Driver.NotFound", "No user found with the provided CPF."));

            var driver = await _driverRepository.GetByUserIdAsync(user.Id, cancellationToken);
            if (driver == null)
                return Result.Failure<Guid>(Error.NotFound("Driver.NotFound", "The user with the provided CPF is not a registered driver."));

            vehicle.AssignDriver(driver.Id);
        }

        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyVehicleCreatedAsync(vehicle.Id, cancellationToken);

        return Result.Success(vehicle.Id);
    }

    private static string HashCpf(string cpf)
    {
        var normalized = new string(cpf.Where(char.IsDigit).ToArray());
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToBase64String(bytes);
    }
}