using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Commands;

public sealed record CompleteTripCommand(
    Guid Id,
    bool ChecklistCompleted,
    string? ChecklistNotes) : IRequest<Result>;
