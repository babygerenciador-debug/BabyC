using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Finance;

public sealed class FinancialCategory : AggregateRoot
{
    private FinancialCategory() { }

    private FinancialCategory(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        string name,
        TransactionType type)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        Name = name;
        Type = type;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public string Name { get; private set; } = default!;
    public TransactionType Type { get; private set; }

    public static Result<FinancialCategory> Create(
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        string name,
        TransactionType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<FinancialCategory>(Error.Validation("FinancialCategory.NameRequired", "Name is required."));

        var category = new FinancialCategory(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            name.Trim(), type);

        return Result.Success(category);
    }

    public Result Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("FinancialCategory.NameRequired", "Name is required."));

        Name = name.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public void Delete()
    {
        SoftDelete(Guid.Empty);
    }
}
