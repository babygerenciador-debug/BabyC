namespace FleetOS.Domain.Fleet.Maintenance;

public enum MaintenanceType
{
    Preventive = 1,
    Corrective = 2
}

public enum MaintenanceStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}
