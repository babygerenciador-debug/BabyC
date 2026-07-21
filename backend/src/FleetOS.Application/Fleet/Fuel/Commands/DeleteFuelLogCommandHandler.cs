using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Fuel.Commands;

internal sealed class DeleteFuelLogCommandHandler : IRequestHandler<DeleteFuelLogCommand, Result>
{
    private readonly IFuelLogRepository _fuelLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteFuelLogCommandHandler(
        IFuelLogRepository fuelLogRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _fuelLogRepository = fuelLogRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(DeleteFuelLogCommand request, CancellationToken cancellationToken)
    {
        var fuelLog = await _fuelLogRepository.GetByIdAsync(request.Id, cancellationToken);
        if (fuelLog is null)
            return Result.Failure(Error.NotFound("FuelLog.NotFound", "Fuel log not found."));

        fuelLog.Delete();
        
        _fuelLogRepository.Remove(fuelLog);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        return Result.Success();
    }
}
