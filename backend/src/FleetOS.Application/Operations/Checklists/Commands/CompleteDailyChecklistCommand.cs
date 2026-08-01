using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Checklists.Commands;

public sealed record CompleteDailyChecklistCommand(
    Guid VehicleId,
    IReadOnlyList<Guid> ChecklistItemIds) : IRequest<Result>;
