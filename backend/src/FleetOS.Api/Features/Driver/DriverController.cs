using FleetOS.Api.Controllers;
using FleetOS.Application.Fleet.Fuel.Commands;
using FleetOS.Application.Fleet.Vehicles.Queries;
using FleetOS.Application.Operations.Checklists.Commands;
using FleetOS.Application.Operations.Checklists.Queries;
using FleetOS.Application.Operations.Drivers.Queries;
using FleetOS.Application.Operations.Trips.Commands;
using FleetOS.Application.Operations.Trips.Queries;
using FleetOS.Application.VehicleIssues.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Driver;

[Authorize(Roles = "Driver")]
[Route("api/v1/[controller]")]
public sealed class DriverController : BaseController
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMyProfileQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("trips")]
    public async Task<IActionResult> GetMyTrips(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetDriverTripsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("my-trip")]
    public async Task<IActionResult> GetMyActiveTrip(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMyActiveTripQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("vehicles")]
    public async Task<IActionResult> GetMyVehicles(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetDriverVehiclesQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("fuel-logs")]
    public async Task<IActionResult> CreateFuelLog(
        [FromBody] CreateDriverFuelLogCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Created($"/api/v1/driver/fuel-logs/{result.Value}", result.Value) : BadRequest(result.Error);
    }

    [HttpPost("trips/{id:guid}/start")]
    public async Task<IActionResult> StartTrip(Guid id, CancellationToken cancellationToken)
    {
        var command = new StartTripCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpGet("checklist")]
    public async Task<IActionResult> GetMyChecklist(
        [FromQuery] Guid vehicleId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTodayChecklistQuery(vehicleId), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("checklist/complete")]
    public async Task<IActionResult> CompleteChecklist(
        [FromBody] CompleteDailyChecklistRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompleteDailyChecklistCommand(request.VehicleId, request.ChecklistItemIds);
        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("issues")]
    public async Task<IActionResult> ReportIssue(
        [FromBody] ReportDriverIssueRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ReportDriverIssueCommand(request.VehicleId, request.Description), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("trips/{id:guid}/complete")]
    public async Task<IActionResult> CompleteTrip(
        Guid id,
        [FromBody] DriverCompleteTripRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompleteTripCommand(id, request.ChecklistCompleted, request.ChecklistNotes);
        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}

public sealed record DriverCompleteTripRequest(bool ChecklistCompleted, string? ChecklistNotes);

public sealed record CompleteDailyChecklistRequest(Guid VehicleId, IReadOnlyList<Guid> ChecklistItemIds);

public sealed record ReportDriverIssueRequest(Guid VehicleId, string Description);
