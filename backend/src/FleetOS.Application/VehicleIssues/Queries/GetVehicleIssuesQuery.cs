using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.VehicleIssues.Queries;

public record VehicleIssueDto(Guid Id, Guid VehicleId, Guid? DriverId, string Description, string Status, DateTimeOffset CreatedAt, DateTimeOffset? ResolvedAt);

public record GetVehicleIssuesQuery : IRequest<Result<IReadOnlyList<VehicleIssueDto>>>;

internal sealed class GetVehicleIssuesQueryHandler : IRequestHandler<GetVehicleIssuesQuery, Result<IReadOnlyList<VehicleIssueDto>>>
{
    private readonly IVehicleIssueReportRepository _issueRepository;

    public GetVehicleIssuesQueryHandler(IVehicleIssueReportRepository issueRepository)
    {
        _issueRepository = issueRepository;
    }

    public async Task<Result<IReadOnlyList<VehicleIssueDto>>> Handle(GetVehicleIssuesQuery request, CancellationToken cancellationToken)
    {
        var issues = await _issueRepository.GetAllAsync(cancellationToken);

        var dtos = issues.Select(i => new VehicleIssueDto(
            i.Id,
            i.VehicleId,
            i.DriverId,
            i.Description,
            i.Status.ToString(),
            i.CreatedAt,
            i.ResolvedAt
        ))
        .OrderByDescending(i => i.CreatedAt)
        .ToList();

        return Result.Success<IReadOnlyList<VehicleIssueDto>>(dtos);
    }
}
