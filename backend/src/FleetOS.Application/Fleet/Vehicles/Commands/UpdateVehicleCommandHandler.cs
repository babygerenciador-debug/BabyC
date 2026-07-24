using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Users;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Shared.Results;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace FleetOS.Application.Fleet.Vehicles.Commands;

internal sealed class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, Result>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUserRepository    _userRepository;
    private readonly IDriverRepository  _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public UpdateVehicleCommandHandler(
        IVehicleRepository vehicleRepository,
        IUserRepository    userRepository,
        IDriverRepository  driverRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _vehicleRepository = vehicleRepository;
        _userRepository    = userRepository;
        _driverRepository  = driverRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (vehicle is null)
            return Result.Failure(Error.NotFound("Vehicle.NotFound", "Vehicle not found."));

        var result = vehicle.Update(
            request.Nickname,
            NormalizeNull(request.Chassi),
            NormalizeNull(request.Brand),
            NormalizeNull(request.Color),
            NormalizeNull(request.Renavam),
            NormalizeNull(request.AnttNumber),
            request.Capacity,
            request.Year,
            NormalizeNull(request.Model),
            NormalizeNull(request.PhotoUrl));

        if (result.IsFailure)
            return result;

        // ── Driver Assignment ──────────────────────────────────────────
        if (request.DriverId.HasValue)
        {
            var driver = await _driverRepository.GetByIdAsync(request.DriverId.Value, cancellationToken);
            if (driver == null)
                return Result.Failure(Error.NotFound("Driver.NotFound", "Driver not found with the provided ID."));
            vehicle.AssignDriver(driver.Id);
        }
        else if (request.DriverCpf != null)
        {
            if (string.IsNullOrWhiteSpace(request.DriverCpf))
            {
                vehicle.ReleaseDriver();
            }
            else
            {
                var cpfHash = HashCpf(request.DriverCpf);
                
                var user = await _userRepository.GetByCpfHashAsync(_tenantContext.TenantId, cpfHash, cancellationToken);
                if (user == null)
                    return Result.Failure(Error.NotFound("Driver.NotFound", "No user found with the provided CPF."));

                var driver = await _driverRepository.GetByUserIdAsync(user.Id, cancellationToken);
                if (driver == null)
                    return Result.Failure(Error.NotFound("Driver.NotFound", "The user with the provided CPF is not a registered driver."));

                vehicle.AssignDriver(driver.Id);
            }
        }

        _vehicleRepository.Update(vehicle);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyVehicleUpdatedAsync(vehicle.Id, cancellationToken);

        return Result.Success();
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
