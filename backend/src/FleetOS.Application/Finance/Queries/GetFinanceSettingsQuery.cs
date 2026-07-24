using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Finance.Queries;

public sealed record GetFinanceSettingsQuery : IRequest<Result<FinanceSettingsDto>>;
