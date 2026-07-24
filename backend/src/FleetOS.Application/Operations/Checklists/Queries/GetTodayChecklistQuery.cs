using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Checklists.Queries;

public sealed record GetTodayChecklistQuery(Guid VehicleId) : IRequest<Result<DailyChecklistDto?>>;
