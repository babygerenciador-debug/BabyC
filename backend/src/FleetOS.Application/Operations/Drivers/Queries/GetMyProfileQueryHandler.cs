using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Queries;

internal sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, Result<DriverDto>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly ITenantContext _tenantContext;

    public GetMyProfileQueryHandler(
        IDriverRepository driverRepository,
        ITenantContext tenantContext)
    {
        _driverRepository = driverRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<DriverDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByUserIdAsync(_tenantContext.UserId, cancellationToken);
        if (driver is null)
            return Result.Failure<DriverDto>(Error.NotFound("Driver.NotFound", "Driver profile not found."));

        var dto = await _driverRepository.GetDriverByIdWithUserAsync(driver.Id, cancellationToken);
        return dto is null
            ? Result.Failure<DriverDto>(Error.NotFound("Driver.NotFound", "Driver profile not found."))
            : Result.Success(dto);
    }
}
