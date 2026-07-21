using FleetOS.Application.Common.Interfaces;
using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Inventory.Queries;

internal sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<ProductCategoryDto>>>
{
    private readonly IProductCategoryRepository _repository;

    public GetCategoriesQueryHandler(IProductCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ProductCategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetAllCategoriesAsync(cancellationToken);
        return Result.Success(result);
    }
}

internal sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetPaginatedProductsAsync(request.Page, request.PageSize, request.SearchTerm, request.CategoryId, cancellationToken);
        return Result.Success(result);
    }
}

internal sealed class GetMainStockQueryHandler : IRequestHandler<GetMainStockQuery, Result<PagedResult<StockBalanceDto>>>
{
    private readonly IStockBalanceRepository _repository;

    public GetMainStockQueryHandler(IStockBalanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<StockBalanceDto>>> Handle(GetMainStockQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetMainStockAsync(request.Page, request.PageSize, request.SearchTerm, cancellationToken);
        return Result.Success(result);
    }
}

internal sealed class GetVehicleStockQueryHandler : IRequestHandler<GetVehicleStockQuery, Result<PagedResult<StockBalanceDto>>>
{
    private readonly IStockBalanceRepository _repository;

    public GetVehicleStockQueryHandler(IStockBalanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<StockBalanceDto>>> Handle(GetVehicleStockQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetVehicleStockAsync(request.VehicleId, request.Page, request.PageSize, request.SearchTerm, cancellationToken);
        return Result.Success(result);
    }
}

internal sealed class GetStockAlertsQueryHandler : IRequestHandler<GetStockAlertsQuery, Result<IReadOnlyList<StockBalanceDto>>>
{
    private readonly IStockBalanceRepository _repository;

    public GetStockAlertsQueryHandler(IStockBalanceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<StockBalanceDto>>> Handle(GetStockAlertsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetStockAlertsAsync(cancellationToken);
        return Result.Success(result);
    }
}

internal sealed class GetMovementsByProductQueryHandler : IRequestHandler<GetMovementsByProductQuery, Result<PagedResult<InventoryMovementDto>>>
{
    private readonly IInventoryMovementRepository _repository;

    public GetMovementsByProductQueryHandler(IInventoryMovementRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<InventoryMovementDto>>> Handle(GetMovementsByProductQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetMovementsByProductAsync(request.ProductId, request.Page, request.PageSize, cancellationToken);
        return Result.Success(result);
    }
}
