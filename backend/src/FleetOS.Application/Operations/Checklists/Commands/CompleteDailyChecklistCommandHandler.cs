using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Operations.Checklists;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Checklists.Commands;

internal sealed class CompleteDailyChecklistCommandHandler : IRequestHandler<CompleteDailyChecklistCommand, Result>
{
    private readonly IRepository<DailyChecklist> _dailyRepo;
    private readonly IRepository<ChecklistItem> _itemRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenant;
    private readonly ICurrentUserService _user;

    public CompleteDailyChecklistCommandHandler(
        IRepository<DailyChecklist> dailyRepo,
        IRepository<ChecklistItem> itemRepo,
        IUnitOfWork unitOfWork,
        ITenantContext tenant,
        ICurrentUserService user)
    {
        _dailyRepo = dailyRepo;
        _itemRepo = itemRepo;
        _unitOfWork = unitOfWork;
        _tenant = tenant;
        _user = user;
    }

    public async Task<Result> Handle(CompleteDailyChecklistCommand request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var daily = (await _dailyRepo.GetAllAsync(cancellationToken))
            .FirstOrDefault(d => d.VehicleId == request.VehicleId && d.Date == today);

        if (daily is null)
        {
            var templates = (await _itemRepo.GetAllAsync(cancellationToken))
                .Where(i => i.IsActive)
                .ToList();

            var created = DailyChecklist.Create(
                _tenant.TenantId, _tenant.OrganizationId, _tenant.BusinessUnitId,
                request.VehicleId, _user.UserId!.Value, today, templates);

            if (created.IsFailure)
                return Result.Failure(created.Error);

            daily = created.Value!;
            await _dailyRepo.AddAsync(daily, cancellationToken);
        }

        foreach (var checklistItemId in request.ChecklistItemIds)
        {
            var result = daily.CompleteItemByTemplateId(checklistItemId);
            if (result.IsFailure)
                return result;
        }

        await _unitOfWork.CommitAsync(_tenant.TenantId, cancellationToken);

        return Result.Success();
    }
}
