using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Operations.Checklists;

public enum DailyChecklistStatus
{
    Pending = 0,
    Completed = 1,
    Partial = 2
}

public sealed class DailyChecklist : AggregateRoot
{
    private readonly List<DailyChecklistItem> _items = new();

    private DailyChecklist() { }

    private DailyChecklist(Guid id, Guid tenantId, Guid organizationId, Guid businessUnitId, Guid vehicleId, Guid driverId, DateOnly date)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        VehicleId = vehicleId;
        DriverId = driverId;
        Date = date;
        Status = DailyChecklistStatus.Pending;
    }

    public Guid VehicleId { get; private set; }
    public Guid DriverId { get; private set; }
    public DateOnly Date { get; private set; }
    public DailyChecklistStatus Status { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public IReadOnlyCollection<DailyChecklistItem> Items => _items.AsReadOnly();

    public static Result<DailyChecklist> Create(
        Guid tenantId, Guid organizationId, Guid businessUnitId,
        Guid vehicleId, Guid driverId, DateOnly date,
        IEnumerable<ChecklistItem> templateItems)
    {
        var checklist = new DailyChecklist(Guid.NewGuid(), tenantId, organizationId, businessUnitId, vehicleId, driverId, date);

        foreach (var template in templateItems.OrderBy(i => i.SortOrder))
        {
            checklist._items.Add(new DailyChecklistItem(
                Guid.NewGuid(), template.Id, template.Title));
        }

        return Result.Success(checklist);
    }

    public Result CompleteItemByTemplateId(Guid checklistItemId)
    {
        var item = _items.FirstOrDefault(i => i.ChecklistItemId == checklistItemId);
        if (item is null)
            return Result.Failure(Error.NotFound("DailyChecklistItem", checklistItemId));

        item.Complete();
        UpdateStatus();
        return Result.Success();
    }

    private void UpdateStatus()
    {
        var total = _items.Count;
        var done = _items.Count(i => i.IsCompleted);

        if (done == 0) Status = DailyChecklistStatus.Pending;
        else if (done >= total)
        {
            Status = DailyChecklistStatus.Completed;
            CompletedAt = DateTimeOffset.UtcNow;
        }
        else Status = DailyChecklistStatus.Partial;

        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public sealed class DailyChecklistItem
{
    private DailyChecklistItem() { }

    internal DailyChecklistItem(Guid id, Guid checklistItemId, string title)
    {
        Id = id;
        ChecklistItemId = checklistItemId;
        Title = title;
        IsCompleted = false;
    }

    public Guid Id { get; private set; }
    public Guid DailyChecklistId { get; private set; }
    public Guid ChecklistItemId { get; private set; }
    public string Title { get; private set; } = default!;
    public bool IsCompleted { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    internal void Complete()
    {
        IsCompleted = true;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
