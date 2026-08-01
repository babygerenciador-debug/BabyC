using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Operations.Checklists;

public sealed class ChecklistItem : AggregateRoot
{
    private ChecklistItem() { }

    private ChecklistItem(Guid id, Guid tenantId, Guid organizationId, Guid businessUnitId, string title)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        Title = title;
        IsActive = true;
    }

    public string Title { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }

    public static Result<ChecklistItem> Create(
        Guid tenantId, Guid organizationId, Guid businessUnitId,
        string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<ChecklistItem>(Error.Validation("ChecklistItem.TitleRequired", "Title is required."));

        return Result.Success(new ChecklistItem(Guid.NewGuid(), tenantId, organizationId, businessUnitId, title.Trim()));
    }

    public Result Update(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(Error.Validation("ChecklistItem.TitleRequired", "Title is required."));

        Title = title.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetSortOrder(int order)
    {
        SortOrder = order;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Delete()
    {
        SoftDelete(Guid.Empty);
    }
}
