using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Maintenance.Commands;

internal sealed class DeleteMaintenanceCommandHandler : IRequestHandler<DeleteMaintenanceCommand, Result>
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteMaintenanceCommandHandler(
        IMaintenanceRepository maintenanceRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _maintenanceRepository = maintenanceRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(DeleteMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var record = await _maintenanceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (record is null)
            return Result.Failure(Error.NotFound("MaintenanceRecord.NotFound", "Maintenance record not found."));

        record.Delete();
        
        _maintenanceRepository.Remove(record);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        return Result.Success();
    }
}
