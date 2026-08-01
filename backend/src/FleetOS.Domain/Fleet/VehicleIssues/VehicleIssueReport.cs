using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Fleet.VehicleIssues;

public sealed class VehicleIssueReport : AggregateRoot
{
    private VehicleIssueReport() { } // EF Core

    private VehicleIssueReport(Guid id, Guid tenantId, Guid organizationId, Guid businessUnitId, Guid vehicleId, Guid? driverId, string description)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        VehicleId = vehicleId;
        DriverId = driverId;
        Description = description;
        Status = IssueStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid VehicleId { get; private set; }
    public Guid? DriverId { get; private set; }
    public string Description { get; private set; } = default!;
    public IssueStatus Status { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    public static Result<VehicleIssueReport> Create(Guid tenantId, Guid organizationId, Guid businessUnitId, Guid vehicleId, Guid? driverId, string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<VehicleIssueReport>(Error.Validation("VehicleIssue.DescriptionRequired", "Description is required."));

        var report = new VehicleIssueReport(Guid.NewGuid(), tenantId, organizationId, businessUnitId, vehicleId, driverId, description.Trim());
        return Result.Success(report);
    }

    public Result UpdateStatus(IssueStatus newStatus)
    {
        if (Status == newStatus)
            return Result.Failure(Error.Validation("VehicleIssue.UnchangedStatus", "Status is already the requested one."));

        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (newStatus == IssueStatus.Resolved)
        {
            ResolvedAt = DateTimeOffset.UtcNow;
        }

        return Result.Success();
    }
}
