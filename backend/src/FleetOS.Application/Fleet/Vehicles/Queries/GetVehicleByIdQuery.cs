using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Vehicles.Queries;

public sealed record GetVehicleByIdQuery(Guid Id) : IRequest<Result<VehicleDto>>;
