using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Commands;

public sealed record UpdateDriverCommand(
    Guid Id,
    string CnhNumber,
    string CnhCategory,
    DateTime CnhExpirationDate) : IRequest<Result>;
