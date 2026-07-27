using FleetOS.Application.Common.Interfaces;
using FleetOS.Application.Finance;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Finance;
using FleetOS.Domain.Fleet.Fuel;
using FleetOS.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace FleetOS.Infrastructure.Persistence.Repositories;

internal sealed class CostCenterRepository : ICostCenterRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public CostCenterRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(CostCenter entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<CostCenter>().AddAsync(entity, cancellationToken);

    public void Update(CostCenter entity) => _dbContext.Set<CostCenter>().Update(entity);
    public void Remove(CostCenter entity) => _dbContext.Set<CostCenter>().Remove(entity);

    public async Task<IReadOnlyList<CostCenter>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<CostCenter>().Where(c => c.TenantId == _tenantContext.TenantId).ToListAsync(cancellationToken);

    public async Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<CostCenter>().FirstOrDefaultAsync(c => c.TenantId == _tenantContext.TenantId && c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CostCenterDto>> GetAllCostCentersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<CostCenter>()
            .Where(c => c.TenantId == _tenantContext.TenantId)
            .Select(c => new CostCenterDto(c.Id, c.Name, c.Description, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}

internal sealed class FinancialCategoryRepository : IFinancialCategoryRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public FinancialCategoryRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(FinancialCategory entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<FinancialCategory>().AddAsync(entity, cancellationToken);

    public void Update(FinancialCategory entity) => _dbContext.Set<FinancialCategory>().Update(entity);
    public void Remove(FinancialCategory entity) => _dbContext.Set<FinancialCategory>().Remove(entity);

    public async Task<IReadOnlyList<FinancialCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<FinancialCategory>().Where(c => c.TenantId == _tenantContext.TenantId).ToListAsync(cancellationToken);

    public async Task<FinancialCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<FinancialCategory>().FirstOrDefaultAsync(c => c.TenantId == _tenantContext.TenantId && c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<FinancialCategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<FinancialCategory>()
            .Where(c => c.TenantId == _tenantContext.TenantId)
            .Select(c => new FinancialCategoryDto(c.Id, c.Name, c.Type, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}

internal sealed class FinancialMonthRepository : IFinancialMonthRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public FinancialMonthRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(FinancialMonth entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<FinancialMonth>().AddAsync(entity, cancellationToken);

    public void Update(FinancialMonth entity) => _dbContext.Set<FinancialMonth>().Update(entity);
    public void Remove(FinancialMonth entity) => _dbContext.Set<FinancialMonth>().Remove(entity);

    public async Task<IReadOnlyList<FinancialMonth>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<FinancialMonth>().Where(m => m.TenantId == _tenantContext.TenantId).ToListAsync(cancellationToken);

    public async Task<FinancialMonth?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<FinancialMonth>().FirstOrDefaultAsync(m => m.TenantId == _tenantContext.TenantId && m.Id == id, cancellationToken);

    public async Task<FinancialMonth?> GetOpenMonthAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<FinancialMonth>()
            .Where(m => m.TenantId == _tenantContext.TenantId && m.Status == MonthStatus.Open)
            .OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<FinancialMonth>> GetAllOrderedDescAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<FinancialMonth>()
            .Where(m => m.TenantId == _tenantContext.TenantId)
            .OrderByDescending(m => m.Year).ThenByDescending(m => m.MonthNumber)
            .ToListAsync(cancellationToken);

    public async Task<FinancialMonthReportDto?> GetMonthReportAsync(Guid monthId, CancellationToken cancellationToken = default)
    {
        var month = await _dbContext.Set<FinancialMonth>()
            .FirstOrDefaultAsync(m => m.Id == monthId && m.TenantId == _tenantContext.TenantId, cancellationToken);
        if (month is null) return null;

        var monthDto = new FinancialMonthDto(
            month.Id, month.Year, month.MonthNumber, month.Label, month.OwnerSalary,
            month.Status == MonthStatus.Open ? "open" : "closed",
            month.OpenedAt, month.ClosedAt, month.CreatedAt);

        var transactions = await _dbContext.Set<FinancialTransaction>()
            .Where(t => t.TenantId == _tenantContext.TenantId && t.FinancialMonthId == monthId && t.Status == TransactionStatus.Paid)
            .Join(_dbContext.Set<FinancialCategory>(), t => t.CategoryId, c => c.Id, (t, c) => new { t, c })
            .GroupJoin(_dbContext.Set<CostCenter>(), tc => tc.t.CostCenterId, cc => cc.Id, (tc, cc) => new { tc.t, tc.c, cc })
            .SelectMany(x => x.cc.DefaultIfEmpty(), (x, cc) => new { x.t, x.c, cc })
            .OrderByDescending(x => x.t.Date)
            .Select(x => new FinancialTransactionDto(
                x.t.Id, x.t.CategoryId, x.c.Name, x.t.CostCenterId, x.cc != null ? x.cc.Name : null,
                x.t.Type, x.t.Amount, x.t.Date, x.t.PaymentDate, x.t.Description, x.t.Status, x.t.ReferenceId, x.t.CreatedAt, x.t.UpdatedAt))
            .ToListAsync(cancellationToken);

        var revenues = transactions.Where(t => t.Type == TransactionType.Revenue).Sum(t => t.Amount);
        var expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var ownerTaxAmount = month.OwnerSalary * 0.27m;
        var netOwnerSalary = month.OwnerSalary - ownerTaxAmount;
        var netBalance = netOwnerSalary + revenues - expenses;

        return new FinancialMonthReportDto(monthDto, revenues, expenses, netBalance, month.OwnerSalary, ownerTaxAmount, netOwnerSalary, transactions);
    }
}

internal sealed class FinancialTransactionRepository : IFinancialTransactionRepository
{
    private readonly FleetOsDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public FinancialTransactionRepository(FleetOsDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task AddAsync(FinancialTransaction entity, CancellationToken cancellationToken = default)
        => await _dbContext.Set<FinancialTransaction>().AddAsync(entity, cancellationToken);

    public void Update(FinancialTransaction entity) => _dbContext.Set<FinancialTransaction>().Update(entity);
    public void Remove(FinancialTransaction entity) => _dbContext.Set<FinancialTransaction>().Remove(entity);

    public async Task<IReadOnlyList<FinancialTransaction>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Set<FinancialTransaction>().Where(t => t.TenantId == _tenantContext.TenantId).ToListAsync(cancellationToken);

    public async Task<FinancialTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Set<FinancialTransaction>().FirstOrDefaultAsync(t => t.TenantId == _tenantContext.TenantId && t.Id == id, cancellationToken);

    public async Task<PagedResult<FinancialTransactionDto>> GetPaginatedTransactionsAsync(int page, int pageSize, TransactionStatus? status, DateTime? startDate, DateTime? endDate, TransactionType? type, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<FinancialTransaction>()
            .Where(t => t.TenantId == _tenantContext.TenantId);

        if (status.HasValue) query = query.Where(t => t.Status == status.Value);
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);
        if (startDate.HasValue) query = query.Where(t => t.Date >= startDate.Value);
        if (endDate.HasValue) query = query.Where(t => t.Date <= endDate.Value);

        var joinQuery = query
            .Join(_dbContext.Set<FinancialCategory>(), t => t.CategoryId, c => c.Id, (t, c) => new { t, c })
            .GroupJoin(_dbContext.Set<CostCenter>(), tc => tc.t.CostCenterId, cc => cc.Id, (tc, cc) => new { tc.t, tc.c, cc })
            .SelectMany(x => x.cc.DefaultIfEmpty(), (x, cc) => new { x.t, x.c, cc });

        var totalCount = await joinQuery.CountAsync(cancellationToken);

        var items = await joinQuery
            .OrderByDescending(x => x.t.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new FinancialTransactionDto(
                x.t.Id, x.t.CategoryId, x.c.Name, x.t.CostCenterId, x.cc != null ? x.cc.Name : null,
                x.t.Type, x.t.Amount, x.t.Date, x.t.PaymentDate, x.t.Description, x.t.Status, x.t.ReferenceId, x.t.CreatedAt, x.t.UpdatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<FinancialTransactionDto>.Create(items, totalCount, page, pageSize);
    }

    public async Task<CashFlowSummaryDto> GetCashFlowSummaryAsync(DateTime? startDate, DateTime? endDate, decimal ownerSalary, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<FinancialTransaction>()
            .Where(t => t.TenantId == _tenantContext.TenantId && t.Status == TransactionStatus.Paid);

        if (startDate.HasValue) query = query.Where(t => t.Date >= startDate.Value);
        if (endDate.HasValue) query = query.Where(t => t.Date <= endDate.Value);

        var revenues = await query.Where(t => t.Type == TransactionType.Revenue).SumAsync(t => t.Amount, cancellationToken);
        var expenses = await query.Where(t => t.Type == TransactionType.Expense).SumAsync(t => t.Amount, cancellationToken);

        var fuelExpenses = await _dbContext.Set<FuelLog>()
            .Where(f => f.TenantId == _tenantContext.TenantId && (startDate == null || f.Date >= startDate) && (endDate == null || f.Date <= endDate))
            .SumAsync(f => f.TotalCost, cancellationToken);
        expenses += fuelExpenses;

        var ownerTaxAmount = ownerSalary * 0.27m; // 27% tax on owner salary
        var netOwnerSalary = ownerSalary - ownerTaxAmount;
        
        // Net balance is the leftover from owner salary after tax + any company revenues - company expenses
        var netBalance = netOwnerSalary + revenues - expenses;

        return new CashFlowSummaryDto(ownerSalary, ownerTaxAmount, netOwnerSalary, revenues, expenses, netBalance);
    }
}
