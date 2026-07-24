using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Tenants;
using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Finance.Queries;

internal sealed class GetCostCentersQueryHandler : IRequestHandler<GetCostCentersQuery, Result<IReadOnlyList<CostCenterDto>>>
{
    private readonly ICostCenterRepository _repository;

    public GetCostCentersQueryHandler(ICostCenterRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<CostCenterDto>>> Handle(GetCostCentersQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetAllCostCentersAsync(cancellationToken);
        return Result.Success(result);
    }
}

internal sealed class GetFinancialCategoriesQueryHandler : IRequestHandler<GetFinancialCategoriesQuery, Result<IReadOnlyList<FinancialCategoryDto>>>
{
    private readonly IFinancialCategoryRepository _repository;

    public GetFinancialCategoriesQueryHandler(IFinancialCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<FinancialCategoryDto>>> Handle(GetFinancialCategoriesQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetAllCategoriesAsync(cancellationToken);
        return Result.Success(result);
    }
}

internal sealed class GetFinancialCategoryByIdQueryHandler : IRequestHandler<GetFinancialCategoryByIdQuery, Result<FinancialCategoryDto>>
{
    private readonly IFinancialCategoryRepository _repository;

    public GetFinancialCategoryByIdQueryHandler(IFinancialCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<FinancialCategoryDto>> Handle(GetFinancialCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null)
            return Result.Failure<FinancialCategoryDto>(Error.NotFound("Category.NotFound", "Financial category not found."));

        var dto = new FinancialCategoryDto(category.Id, category.Name, category.Type, category.CreatedAt, category.UpdatedAt);
        return Result.Success(dto);
    }
}

internal sealed class GetCostCenterByIdQueryHandler : IRequestHandler<GetCostCenterByIdQuery, Result<CostCenterDto>>
{
    private readonly ICostCenterRepository _repository;

    public GetCostCenterByIdQueryHandler(ICostCenterRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CostCenterDto>> Handle(GetCostCenterByIdQuery request, CancellationToken cancellationToken)
    {
        var costCenter = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (costCenter is null)
            return Result.Failure<CostCenterDto>(Error.NotFound("CostCenter.NotFound", "Cost center not found."));

        var dto = new CostCenterDto(costCenter.Id, costCenter.Name, costCenter.Description, costCenter.CreatedAt, costCenter.UpdatedAt);
        return Result.Success(dto);
    }
}

internal sealed class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, Result<PagedResult<FinancialTransactionDto>>>
{
    private readonly IFinancialTransactionRepository _repository;

    public GetTransactionsQueryHandler(IFinancialTransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<FinancialTransactionDto>>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetPaginatedTransactionsAsync(request.Page, request.PageSize, request.Status, request.StartDate, request.EndDate, request.Type, cancellationToken);
        return Result.Success(result);
    }
}

internal sealed class GetCashFlowSummaryQueryHandler : IRequestHandler<GetCashFlowSummaryQuery, Result<CashFlowSummaryDto>>
{
    private readonly IFinancialTransactionRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly ITenantRepository _tenantRepository;

    public GetCashFlowSummaryQueryHandler(
        IFinancialTransactionRepository repository,
        ITenantContext tenantContext,
        ITenantRepository tenantRepository)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<CashFlowSummaryDto>> Handle(GetCashFlowSummaryQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(_tenantContext.TenantId, cancellationToken);
        var ownerSalary = tenant?.OwnerSalary ?? 0;

        var result = await _repository.GetCashFlowSummaryAsync(request.StartDate, request.EndDate, ownerSalary, cancellationToken);
        return Result.Success(result);
    }
}
