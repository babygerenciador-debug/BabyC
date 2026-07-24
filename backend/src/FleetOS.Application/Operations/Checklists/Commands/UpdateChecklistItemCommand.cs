using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Checklists.Commands;

public sealed record UpdateChecklistItemCommand(
    Guid Id,
    string Title,
    bool IsActive) : IRequest<Result>;
