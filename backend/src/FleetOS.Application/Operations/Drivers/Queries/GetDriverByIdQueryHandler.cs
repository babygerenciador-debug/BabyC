using FleetOS.Application.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Queries;

internal sealed class GetDriverByIdQueryHandler : IRequestHandler<GetDriverByIdQuery, Result<DriverDto>>
{
    private readonly IDriverRepository _driverRepository;

    public GetDriverByIdQueryHandler(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    public async Task<Result<DriverDto>> Handle(GetDriverByIdQuery request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetDriverByIdWithUserAsync(request.Id, cancellationToken);
        if (driver is null)
            return Result.Failure<DriverDto>(Error.NotFound("Driver.NotFound", "Driver not found."));

        return Result.Success(driver);
    }
}
