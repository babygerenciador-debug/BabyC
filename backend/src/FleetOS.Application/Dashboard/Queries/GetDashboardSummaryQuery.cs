using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Dashboard.Queries;

public sealed record GetDashboardSummaryQuery() : IRequest<Result<DashboardSummaryDto>>;
