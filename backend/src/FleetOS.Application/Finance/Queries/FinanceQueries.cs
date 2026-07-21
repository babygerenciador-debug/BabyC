using FleetOS.Domain.Finance;
using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Finance.Queries;

public sealed record GetCostCentersQuery() : IRequest<Result<IReadOnlyList<CostCenterDto>>>;
public sealed record GetFinancialCategoriesQuery() : IRequest<Result<IReadOnlyList<FinancialCategoryDto>>>;
public sealed record GetFinancialCategoryByIdQuery(Guid Id) : IRequest<Result<FinancialCategoryDto>>;
public sealed record GetCostCenterByIdQuery(Guid Id) : IRequest<Result<CostCenterDto>>;

public sealed record GetTransactionsQuery(
    int Page,
    int PageSize,
    TransactionStatus? Status,
    DateTime? StartDate,
    DateTime? EndDate,
    TransactionType? Type) : IRequest<Result<PagedResult<FinancialTransactionDto>>>;

public sealed record GetCashFlowSummaryQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    decimal OwnerSalary) : IRequest<Result<CashFlowSummaryDto>>;
