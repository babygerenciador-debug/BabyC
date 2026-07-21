using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Queries;

public sealed record GetDriversQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    string? Status) : IRequest<Result<PagedResult<DriverDto>>>;
