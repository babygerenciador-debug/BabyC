using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Inventory.Commands;

public sealed record CreateProductCategoryCommand(string Name, string? Description) : IRequest<Result<Guid>>;

public sealed record UpdateProductCategoryCommand(Guid Id, string Name, string? Description) : IRequest<Result>;

public sealed record CreateProductCommand(
    Guid CategoryId,
    string Name,
    string? SKU,
    string? Description,
    decimal AverageUnitPrice) : IRequest<Result<Guid>>;

public sealed record UpdateProductCommand(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? SKU,
    string? Description) : IRequest<Result>;
