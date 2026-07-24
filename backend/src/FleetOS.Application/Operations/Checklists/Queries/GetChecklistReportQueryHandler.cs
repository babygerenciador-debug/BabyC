using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Users;
using FleetOS.Domain.Fleet.Vehicles;
using FleetOS.Domain.Operations.Checklists;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Checklists.Queries;

internal sealed class GetChecklistReportQueryHandler : IRequestHandler<GetChecklistReportQuery, Result<IReadOnlyList<ChecklistReportRowDto>>>
{
    private readonly IRepository<DailyChecklist> _dailyRepo;
    private readonly IRepository<Vehicle> _vehicleRepo;
    private readonly IUserRepository _userRepo;

    public GetChecklistReportQueryHandler(
        IRepository<DailyChecklist> dailyRepo,
        IRepository<Vehicle> vehicleRepo,
        IUserRepository userRepo)
    {
        _dailyRepo = dailyRepo;
        _vehicleRepo = vehicleRepo;
        _userRepo = userRepo;
    }

    public async Task<Result<IReadOnlyList<ChecklistReportRowDto>>> Handle(GetChecklistReportQuery request, CancellationToken cancellationToken)
    {
        var all = await _dailyRepo.GetAllAsync(cancellationToken);
        var vehicles = await _vehicleRepo.GetAllAsync(cancellationToken);
        var users = await _userRepo.GetAllAsync(cancellationToken);

        var vMap = vehicles.ToDictionary(v => v.Id, v => v.LicensePlate);
        var uMap = users.ToDictionary(u => u.Id, u => u.Name);

        var query = all.AsEnumerable();

        if (request.VehicleId.HasValue)
            query = query.Where(d => d.VehicleId == request.VehicleId.Value);

        if (DateOnly.TryParse(request.StartDate, out var start))
            query = query.Where(d => d.Date >= start);

        if (DateOnly.TryParse(request.EndDate, out var end))
            query = query.Where(d => d.Date <= end);

        var rows = query.OrderByDescending(d => d.Date).Select(d => new ChecklistReportRowDto(
            d.Date.ToString("yyyy-MM-dd"),
            vMap.GetValueOrDefault(d.VehicleId, d.VehicleId.ToString()),
            uMap.GetValueOrDefault(d.DriverId, d.DriverId.ToString()),
            d.Status.ToString(),
            d.Items.Count,
            d.Items.Count(i => i.IsCompleted))).ToList();

        return Result.Success<IReadOnlyList<ChecklistReportRowDto>>(rows);
    }
}
