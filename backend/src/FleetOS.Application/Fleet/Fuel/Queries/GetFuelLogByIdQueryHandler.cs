using FleetOS.Application.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Fuel.Queries;

internal sealed class GetFuelLogByIdQueryHandler : IRequestHandler<GetFuelLogByIdQuery, Result<FuelLogDto>>
{
    private readonly IFuelLogRepository _fuelLogRepository;

    public GetFuelLogByIdQueryHandler(IFuelLogRepository fuelLogRepository)
    {
        _fuelLogRepository = fuelLogRepository;
    }

    public async Task<Result<FuelLogDto>> Handle(GetFuelLogByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await _fuelLogRepository.GetFuelLogByIdWithDetailsAsync(request.Id, cancellationToken);
        if (log is null)
            return Result.Failure<FuelLogDto>(Error.NotFound("FuelLog.NotFound", "Fuel log not found."));

        return Result.Success(log);
    }
}
