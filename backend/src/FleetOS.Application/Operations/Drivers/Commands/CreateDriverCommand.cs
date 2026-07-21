using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Commands;

public sealed record CreateDriverCommand(
    string Name,
    string Email,
    string Password,
    string Cpf,
    string CnhNumber,
    string CnhCategory,
    DateTime CnhExpirationDate) : IRequest<Result<Guid>>;
