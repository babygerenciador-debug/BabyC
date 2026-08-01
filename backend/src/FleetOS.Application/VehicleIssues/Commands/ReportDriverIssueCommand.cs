using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.VehicleIssues;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.VehicleIssues.Commands;

public sealed record ReportDriverIssueCommand(Guid VehicleId, string Description) : IRequest<Result<Guid>>;

internal sealed class ReportDriverIssueCommandHandler : IRequestHandler<ReportDriverIssueCommand, Result<Guid>>
{
    private readonly IVehicleIssueReportRepository _issueRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;

    public ReportDriverIssueCommandHandler(
        IVehicleIssueReportRepository issueRepository,
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUser)
    {
        _issueRepository = issueRepository;
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(ReportDriverIssueCommand request, CancellationToken cancellationToken)
    {
        Guid? driverId = null;
        if (_currentUser.UserId.HasValue)
        {
            var driver = await _driverRepository.GetByUserIdAsync(_currentUser.UserId.Value, cancellationToken);
            if (driver is not null)
                driverId = driver.Id;
        }

        var result = VehicleIssueReport.Create(
            _tenantContext.TenantId,
            _tenantContext.OrganizationId,
            _tenantContext.BusinessUnitId,
            request.VehicleId,
            driverId,
            request.Description);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        await _issueRepository.AddAsync(result.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        return Result.Success(result.Value!.Id);
    }
}
