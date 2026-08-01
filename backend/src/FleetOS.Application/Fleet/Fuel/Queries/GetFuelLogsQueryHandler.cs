using FleetOS.Application.Common.Interfaces;
using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Fuel.Queries;

internal sealed class GetFuelLogsQueryHandler : IRequestHandler<GetFuelLogsQuery, Result<PagedResult<FuelLogDto>>>
{
    private readonly IFuelLogRepository _fuelLogRepository;

    public GetFuelLogsQueryHandler(IFuelLogRepository fuelLogRepository)
    {
        _fuelLogRepository = fuelLogRepository;
    }

    public async Task<Result<PagedResult<FuelLogDto>>> Handle(GetFuelLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _fuelLogRepository.GetPaginatedFuelLogsAsync(
            request.Page,
            request.PageSize,
            request.VehicleId,
            request.DriverId,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        return Result.Success(logs);
    }
}
