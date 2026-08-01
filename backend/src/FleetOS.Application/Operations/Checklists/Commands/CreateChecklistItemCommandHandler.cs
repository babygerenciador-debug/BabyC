using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Operations.Checklists;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Checklists.Commands;

internal sealed class CreateChecklistItemCommandHandler : IRequestHandler<CreateChecklistItemCommand, Result<Guid>>
{
    private readonly IRepository<ChecklistItem> _repo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenant;

    public CreateChecklistItemCommandHandler(
        IRepository<ChecklistItem> repo,
        IUnitOfWork unitOfWork,
        ITenantContext tenant)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
        _tenant = tenant;
    }

    public async Task<Result<Guid>> Handle(CreateChecklistItemCommand request, CancellationToken cancellationToken)
    {
        var result = ChecklistItem.Create(
            _tenant.TenantId, _tenant.OrganizationId, _tenant.BusinessUnitId,
            request.Title);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        await _repo.AddAsync(result.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(_tenant.TenantId, cancellationToken);

        return Result.Success(result.Value!.Id);
    }
}
