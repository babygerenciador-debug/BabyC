using FleetOS.Domain.Fleet.Maintenance;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Fleet.Maintenance.Commands;

public sealed record UpdateMaintenanceCommand(
    Guid Id,
    MaintenanceType Type,
    MaintenanceStatus Status,
    DateTime Date,
    int Odometer,
    string Description,
    decimal TotalCost,
    string? ProviderName,
    string? InvoiceUrl,
    string? Notes) : IRequest<Result>;
