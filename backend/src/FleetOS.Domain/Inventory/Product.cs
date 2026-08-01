using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Inventory;

public sealed class Product : AggregateRoot
{
    private Product() { }

    private Product(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid categoryId,
        string name,
        string? sku,
        string? description,
        decimal averageUnitPrice)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        CategoryId = categoryId;
        Name = name;
        SKU = sku;
        Description = description;
        AverageUnitPrice = averageUnitPrice;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? SKU { get; private set; }
    public string? Description { get; private set; }
    public decimal AverageUnitPrice { get; private set; }

    public static Result<Product> Create(
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid categoryId,
        string name,
        string? sku,
        string? description,
        decimal averageUnitPrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Product>(Error.Validation("Product.NameRequired", "Name is required."));

        if (averageUnitPrice < 0)
            return Result.Failure<Product>(Error.Validation("Product.InvalidPrice", "Price cannot be negative."));

        var product = new Product(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            categoryId, name.Trim(), sku?.Trim(), description?.Trim(), averageUnitPrice);

        return Result.Success(product);
    }

    public Result Update(Guid categoryId, string name, string? sku, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("Product.NameRequired", "Name is required."));

        CategoryId = categoryId;
        Name = name.Trim();
        SKU = sku?.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public void UpdateAveragePrice(decimal newPrice)
    {
        if (newPrice >= 0)
        {
            AverageUnitPrice = newPrice;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void Delete()
    {
        SoftDelete(Guid.Empty);
    }
}
