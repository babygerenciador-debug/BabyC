using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Inventory;

public sealed class ProductCategory : AggregateRoot
{
    private ProductCategory() { }

    private ProductCategory(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        string name,
        string? description)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    public static Result<ProductCategory> Create(
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        string name,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<ProductCategory>(Error.Validation("ProductCategory.NameRequired", "Name is required."));

        var category = new ProductCategory(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            name.Trim(), description?.Trim());

        return Result.Success(category);
    }

    public Result Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("ProductCategory.NameRequired", "Name is required."));

        Name = name.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public void Delete()
    {
        SoftDelete(Guid.Empty);
    }
}
