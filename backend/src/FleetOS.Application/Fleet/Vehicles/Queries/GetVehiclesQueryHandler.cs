using FleetOS.Application.Common.Interfaces;
using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Vehicles.Queries;

internal sealed class GetVehiclesQueryHandler : IRequestHandler<GetVehiclesQuery, Result<PagedResult<VehicleDto>>>
{
    private readonly IVehicleRepository _vehicleRepository;

    public GetVehiclesQueryHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Result<PagedResult<VehicleDto>>> Handle(GetVehiclesQuery request, CancellationToken cancellationToken)
    {
        var result = await _vehicleRepository.GetPaginatedVehiclesAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.Status,
            cancellationToken);

        return Result.Success(result);
    }
}
