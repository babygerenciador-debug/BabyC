using FleetOS.Application.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Maintenance.Queries;

internal sealed class GetMaintenanceByIdQueryHandler : IRequestHandler<GetMaintenanceByIdQuery, Result<MaintenanceDto>>
{
    private readonly IMaintenanceRepository _maintenanceRepository;

    public GetMaintenanceByIdQueryHandler(IMaintenanceRepository maintenanceRepository)
    {
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task<Result<MaintenanceDto>> Handle(GetMaintenanceByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _maintenanceRepository.GetMaintenanceByIdWithDetailsAsync(request.Id, cancellationToken);
        if (record is null)
            return Result.Failure<MaintenanceDto>(Error.NotFound("MaintenanceRecord.NotFound", "Maintenance record not found."));

        return Result.Success(record);
    }
}
