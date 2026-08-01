using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Finance.Commands;

public sealed record UpdateFinanceSettingsCommand(decimal OwnerSalary) : IRequest<Result>;
