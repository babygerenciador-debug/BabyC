using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Commands;

public sealed record DeleteDriverCommand(Guid Id) : IRequest<Result>;
