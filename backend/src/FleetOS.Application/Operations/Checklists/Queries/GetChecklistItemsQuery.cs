using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Checklists.Queries;

public sealed record GetChecklistItemsQuery : IRequest<Result<IReadOnlyList<ChecklistItemDto>>>;
