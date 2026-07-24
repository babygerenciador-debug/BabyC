using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Checklists.Commands;

public sealed record CreateChecklistItemCommand(
    string Title) : IRequest<Result<Guid>>;
