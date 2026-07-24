using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Checklists.Commands;

public sealed record DeleteChecklistItemCommand(Guid Id) : IRequest<Result>;
