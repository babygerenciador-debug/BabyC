using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Tenants;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Finance.Commands;

internal sealed class UpdateFinanceSettingsCommandHandler : IRequestHandler<UpdateFinanceSettingsCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public UpdateFinanceSettingsCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(UpdateFinanceSettingsCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantRepository.GetByIdAsync(_tenantContext.TenantId, cancellationToken);
        if (tenant is null)
            return Result.Failure(Error.NotFound("Tenant.NotFound", "Tenant not found."));

        if (request.OwnerSalary < 0)
            return Result.Failure(Error.Validation("OwnerSalary.Invalid", "Owner salary must be zero or greater."));

        tenant.SetOwnerSalary(request.OwnerSalary);
        _tenantRepository.Update(tenant);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyDashboardUpdateAsync(cancellationToken);

        return Result.Success();
    }
}
