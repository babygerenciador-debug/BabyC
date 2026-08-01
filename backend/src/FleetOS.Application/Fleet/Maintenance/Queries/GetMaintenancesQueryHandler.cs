using FleetOS.Application.Common.Interfaces;
using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Maintenance.Queries;

internal sealed class GetMaintenancesQueryHandler : IRequestHandler<GetMaintenancesQuery, Result<PagedResult<MaintenanceDto>>>
{
    private readonly IMaintenanceRepository _maintenanceRepository;

    public GetMaintenancesQueryHandler(IMaintenanceRepository maintenanceRepository)
    {
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task<Result<PagedResult<MaintenanceDto>>> Handle(GetMaintenancesQuery request, CancellationToken cancellationToken)
    {
        var records = await _maintenanceRepository.GetPaginatedMaintenancesAsync(
            request.Page,
            request.PageSize,
            request.VehicleId,
            request.Type,
            request.Status,
            cancellationToken);

        return Result.Success(records);
    }
}
