using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Fuel.Queries;

public sealed record GetFuelLogByIdQuery(Guid Id) : IRequest<Result<FuelLogDto>>;
