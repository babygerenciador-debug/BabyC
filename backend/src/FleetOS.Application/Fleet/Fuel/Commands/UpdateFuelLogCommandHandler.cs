using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Fuel.Commands;

internal sealed class UpdateFuelLogCommandHandler : IRequestHandler<UpdateFuelLogCommand, Result>
{
    private readonly IFuelLogRepository _fuelLogRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateFuelLogCommandHandler(
        IFuelLogRepository fuelLogRepository,
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _fuelLogRepository = fuelLogRepository;
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(UpdateFuelLogCommand request, CancellationToken cancellationToken)
    {
        var fuelLog = await _fuelLogRepository.GetByIdAsync(request.Id, cancellationToken);
        if (fuelLog is null)
            return Result.Failure(Error.NotFound("FuelLog.NotFound", "Fuel log not found."));

        if (request.DriverId.HasValue)
        {
            var driver = await _driverRepository.GetByIdAsync(request.DriverId.Value, cancellationToken);
            if (driver is null)
                return Result.Failure(Error.NotFound("Driver.NotFound", "Driver not found."));
        }

        var result = fuelLog.Update(
            request.DriverId,
            request.Date,
            request.Odometer,
            request.Liters,
            request.TotalCost,
            request.ReceiptUrl,
            request.Notes);

        if (result.IsFailure)
            return result;

        // Recalculate average consumption if necessary? For MVP, we'll keep it simple and just update the log.
        // A full recalculation would involve finding the previous log again.
        
        _fuelLogRepository.Update(fuelLog);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        return Result.Success();
    }
}
