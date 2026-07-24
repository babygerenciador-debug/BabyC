using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Vehicles.Commands;

internal sealed class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand, Result>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public DeleteVehicleCommandHandler(
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (vehicle is null)
            return Result.Failure(Error.NotFound("Vehicle.NotFound", "Vehicle not found."));

        vehicle.Delete();
        
        _vehicleRepository.Remove(vehicle);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyVehicleUpdatedAsync(request.Id, cancellationToken);

        return Result.Success();
    }
}
