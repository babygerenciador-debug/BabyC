using FleetOS.Api.Controllers;
using FleetOS.Application.Operations.Trips.Commands;
using FleetOS.Application.Operations.Trips.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Trips;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/[controller]")]
public sealed class TripsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateTrip(
        [FromBody] CreateTripCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Created($"/api/v1/trips/{result.Value}", result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? driverId = null,
        [FromQuery] Guid? vehicleId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTripsQuery(page, pageSize, searchTerm, status, driverId, vehicleId);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTripById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetTripByIdQuery(id), cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> StartTrip(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new StartTripCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> CompleteTrip(
        Guid id,
        [FromBody] CompleteTripRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompleteTripCommand(id, request.ChecklistCompleted, request.ChecklistNotes);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPatch("{id:guid}/pay")]
    public async Task<IActionResult> PayTrip(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new PayTripCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPatch("{id:guid}/swap-vehicle")]
    public async Task<IActionResult> SwapVehicle(
        Guid id,
        [FromBody] SwapVehicleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SwapTripVehicleCommand(id, request.NewVehicleId);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> CancelTrip(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CancelTripCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}

public sealed record CompleteTripRequest(bool ChecklistCompleted, string? ChecklistNotes);
public sealed record SwapVehicleRequest(Guid NewVehicleId);
