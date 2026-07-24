using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Operations.Checklists;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Checklists.Queries;

internal sealed class GetTodayChecklistQueryHandler : IRequestHandler<GetTodayChecklistQuery, Result<DailyChecklistDto?>>
{
    private readonly IRepository<DailyChecklist> _dailyRepo;
    private readonly IRepository<ChecklistItem> _itemRepo;
    private readonly ITenantContext _tenant;

    public GetTodayChecklistQueryHandler(
        IRepository<DailyChecklist> dailyRepo,
        IRepository<ChecklistItem> itemRepo,
        ITenantContext tenant)
    {
        _dailyRepo = dailyRepo;
        _itemRepo = itemRepo;
        _tenant = tenant;
    }

    public async Task<Result<DailyChecklistDto?>> Handle(GetTodayChecklistQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var existing = (await _dailyRepo.GetAllAsync(cancellationToken))
            .FirstOrDefault(d => d.VehicleId == request.VehicleId && d.Date == today);

        if (existing is not null)
            return Result.Success<DailyChecklistDto?>(Map(existing));

        var templates = (await _itemRepo.GetAllAsync(cancellationToken))
            .Where(i => i.IsActive)
            .OrderBy(i => i.SortOrder)
            .ToList();

        if (templates.Count == 0)
            return Result.Success<DailyChecklistDto?>(null);

        return Result.Success<DailyChecklistDto?>(new DailyChecklistDto(
            Guid.Empty,
            request.VehicleId,
            Guid.Empty,
            today.ToString("yyyy-MM-dd"),
            "Pending",
            null,
            templates.Select(t => new DailyChecklistItemDto(
                Guid.Empty, t.Id, t.Title, false, null)).ToList()));
    }

    private static DailyChecklistDto Map(DailyChecklist d) => new(
        d.Id, d.VehicleId, d.DriverId,
        d.Date.ToString("yyyy-MM-dd"),
        d.Status.ToString(),
        d.CompletedAt,
        d.Items.Select(i => new DailyChecklistItemDto(
            i.Id, i.ChecklistItemId, i.Title, i.IsCompleted, i.CompletedAt)).ToList());
}
