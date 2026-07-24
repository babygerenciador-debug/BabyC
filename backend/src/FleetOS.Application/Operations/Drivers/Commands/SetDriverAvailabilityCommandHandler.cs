using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Operations.Drivers;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Commands;

internal sealed class SetDriverAvailabilityCommandHandler : IRequestHandler<SetDriverAvailabilityCommand, Result>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public SetDriverAvailabilityCommandHandler(
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(SetDriverAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByIdAsync(request.Id, cancellationToken);
        if (driver is null)
            return Result.Failure(Error.NotFound("Driver.NotFound", "Driver not found."));

        driver.SetAvailability(request.IsAvailable);
        _driverRepository.Update(driver);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyDriverUpdatedAsync(driver.Id, cancellationToken);

        return Result.Success();
    }
}
