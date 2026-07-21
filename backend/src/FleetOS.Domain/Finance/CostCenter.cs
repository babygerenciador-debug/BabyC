using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Finance;

public sealed class CostCenter : AggregateRoot
{
    private CostCenter() { }

    private CostCenter(
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

    public static Result<CostCenter> Create(
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        string name,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<CostCenter>(Error.Validation("CostCenter.NameRequired", "Name is required."));

        var costCenter = new CostCenter(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            name.Trim(), description?.Trim());

        return Result.Success(costCenter);
    }

    public Result Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("CostCenter.NameRequired", "Name is required."));

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
