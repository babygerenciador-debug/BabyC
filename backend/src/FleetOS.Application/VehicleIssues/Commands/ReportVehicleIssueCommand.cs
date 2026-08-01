using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.VehicleIssues;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.VehicleIssues.Commands;

public record ReportVehicleIssueCommand(Guid VehicleId, string Description) : IRequest<Result<Guid>>;

internal sealed class ReportVehicleIssueCommandHandler : IRequestHandler<ReportVehicleIssueCommand, Result<Guid>>
{
    private readonly IVehicleIssueReportRepository _issueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public ReportVehicleIssueCommandHandler(IVehicleIssueReportRepository issueRepository, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _issueRepository = issueRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(ReportVehicleIssueCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;
        var orgId = _tenantContext.OrganizationId;
        var buId = _tenantContext.BusinessUnitId;
        var driverId = _tenantContext.UserRole == UserRoleContext.Driver ? (Guid?)_tenantContext.UserId : null;

        var reportResult = VehicleIssueReport.Create(tenantId, orgId, buId, request.VehicleId, driverId, request.Description);

        if (reportResult.IsFailure)
            return Result.Failure<Guid>(reportResult.Error);

        await _issueRepository.AddAsync(reportResult.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        // TODO: Enviar notificação via SignalR para o Admin sobre este novo relato

        return Result.Success(reportResult.Value!.Id);
    }
}
