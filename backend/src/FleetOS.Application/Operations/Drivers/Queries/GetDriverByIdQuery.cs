using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Queries;

public sealed record GetDriverByIdQuery(Guid Id) : IRequest<Result<DriverDto>>;
