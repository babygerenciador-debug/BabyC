using FleetOS.Application.Finance;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Finance;
using FleetOS.Shared.Pagination;

namespace FleetOS.Application.Common.Interfaces;

public interface ICostCenterRepository : IRepository<CostCenter>
{
    Task<IReadOnlyList<CostCenterDto>> GetAllCostCentersAsync(CancellationToken cancellationToken = default);
}

public interface IFinancialCategoryRepository : IRepository<FinancialCategory>
{
    Task<IReadOnlyList<FinancialCategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
}

public interface IFinancialMonthRepository : IRepository<FinancialMonth>
{
    Task<FinancialMonth?> GetOpenMonthAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FinancialMonth>> GetAllOrderedDescAsync(CancellationToken cancellationToken = default);
    Task<FinancialMonthReportDto?> GetMonthReportAsync(Guid monthId, CancellationToken cancellationToken = default);
}

public interface IFinancialTransactionRepository : IRepository<FinancialTransaction>
{
    Task<PagedResult<FinancialTransactionDto>> GetPaginatedTransactionsAsync(int page, int pageSize, TransactionStatus? status, DateTime? startDate, DateTime? endDate, TransactionType? type, CancellationToken cancellationToken = default);
    Task<CashFlowSummaryDto> GetCashFlowSummaryAsync(DateTime? startDate, DateTime? endDate, decimal ownerSalary, CancellationToken cancellationToken = default);
}
