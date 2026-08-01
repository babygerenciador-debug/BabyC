using FleetOS.Domain.Inventory;
using FleetOS.Shared.Pagination;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Inventory.Queries;

public sealed record GetCategoriesQuery() : IRequest<Result<IReadOnlyList<ProductCategoryDto>>>;

public sealed record GetProductsQuery(
    int Page,
    int PageSize,
    string? SearchTerm,
    Guid? CategoryId) : IRequest<Result<PagedResult<ProductDto>>>;

public sealed record GetMainStockQuery(
    int Page,
    int PageSize,
    string? SearchTerm) : IRequest<Result<PagedResult<StockBalanceDto>>>;

public sealed record GetVehicleStockQuery(
    Guid VehicleId,
    int Page,
    int PageSize,
    string? SearchTerm) : IRequest<Result<PagedResult<StockBalanceDto>>>;

public sealed record GetStockAlertsQuery() : IRequest<Result<IReadOnlyList<StockBalanceDto>>>;

public sealed record GetMovementsByProductQuery(
    Guid ProductId,
    int Page,
    int PageSize) : IRequest<Result<PagedResult<InventoryMovementDto>>>;
