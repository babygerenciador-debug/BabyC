using FleetOS.Application.Common.Interfaces;
using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Queries;

internal sealed class GetDriversQueryHandler : IRequestHandler<GetDriversQuery, Result<PagedResult<DriverDto>>>
{
    private readonly IDriverRepository _driverRepository;

    public GetDriversQueryHandler(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    public async Task<Result<PagedResult<DriverDto>>> Handle(GetDriversQuery request, CancellationToken cancellationToken)
    {
        var result = await _driverRepository.GetPaginatedDriversAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            request.Status,
            cancellationToken);

        return Result.Success(result);
    }
}
