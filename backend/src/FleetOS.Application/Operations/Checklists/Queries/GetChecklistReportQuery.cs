using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Checklists.Queries;

public sealed record GetChecklistReportQuery(
    Guid? VehicleId,
    string? StartDate,
    string? EndDate) : IRequest<Result<IReadOnlyList<ChecklistReportRowDto>>>;
