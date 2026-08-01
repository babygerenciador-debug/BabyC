using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Users;
using FleetOS.Domain.Fleet.VehicleIssues;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Domain.Operations.Drivers;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.VehicleIssues.Queries;

public record VehicleIssueDto(
    Guid Id,
    Guid VehicleId,
    string? VehicleLicensePlate,
    Guid? DriverId,
    string? DriverName,
    string Description,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ResolvedAt);

public record GetVehicleIssuesQuery : IRequest<Result<IReadOnlyList<VehicleIssueDto>>>;

internal sealed class GetVehicleIssuesQueryHandler : IRequestHandler<GetVehicleIssuesQuery, Result<IReadOnlyList<VehicleIssueDto>>>
{
    private readonly IVehicleIssueReportRepository _issueRepository;
    private readonly IRepository<Vehicle> _vehicleRepo;
    private readonly IDriverRepository _driverRepo;
    private readonly IUserRepository _userRepo;

    public GetVehicleIssuesQueryHandler(
        IVehicleIssueReportRepository issueRepository,
        IRepository<Vehicle> vehicleRepo,
        IDriverRepository driverRepo,
        IUserRepository userRepo)
    {
        _issueRepository = issueRepository;
        _vehicleRepo = vehicleRepo;
        _driverRepo = driverRepo;
        _userRepo = userRepo;
    }

    public async Task<Result<IReadOnlyList<VehicleIssueDto>>> Handle(GetVehicleIssuesQuery request, CancellationToken cancellationToken)
    {
        var issues = await _issueRepository.GetAllAsync(cancellationToken);
        var vehicles = await _vehicleRepo.GetAllAsync(cancellationToken);
        var drivers = await _driverRepo.GetAllAsync(cancellationToken);
        var users = await _userRepo.GetAllAsync(cancellationToken);

        var vMap = vehicles.ToDictionary(v => v.Id, v => (string?)v.LicensePlate);
        var dMap = drivers.ToDictionary(d => d.Id, d => d.UserId);
        var uMap = users.ToDictionary(u => u.Id, u => (string?)u.Name);

        var sorted = issues
            .OrderBy(i => i.Status == IssueStatus.Pending ? 0 : i.Status == IssueStatus.InReview ? 1 : 2)
            .ThenByDescending(i => i.CreatedAt)
            .ToList();

        var dtos = sorted.Select(i =>
        {
            var userId = i.DriverId.HasValue ? dMap.GetValueOrDefault(i.DriverId.Value, Guid.Empty) : Guid.Empty;
            var driverName = userId != Guid.Empty ? uMap.GetValueOrDefault(userId, null) : null;
            return new VehicleIssueDto(
                i.Id,
                i.VehicleId,
                vMap.GetValueOrDefault(i.VehicleId, i.VehicleId.ToString()),
                i.DriverId,
                driverName,
                i.Description,
                i.Status.ToString(),
                i.CreatedAt,
                i.ResolvedAt
            );
        }).ToList();

        return Result.Success<IReadOnlyList<VehicleIssueDto>>(dtos);
    }
}
