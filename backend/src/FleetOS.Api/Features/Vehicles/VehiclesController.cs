using FleetOS.Api.Controllers;
using FleetOS.Application.Fleet.Vehicles.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Vehicles;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/[controller]")]
public sealed class VehiclesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateVehicle(
        [FromBody] CreateVehicleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Created($"/api/v1/vehicles/{result.Value}", result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetVehicles(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new FleetOS.Application.Fleet.Vehicles.Queries.GetVehiclesQuery(page, pageSize, searchTerm, status);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetVehicleById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new FleetOS.Application.Fleet.Vehicles.Queries.GetVehicleByIdQuery(id), cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateVehicle(
        Guid id,
        [FromBody] UpdateVehicleCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(FleetOS.Shared.Results.Error.Validation("Vehicle.IdMismatch", "The ID in the URL must match the ID in the body."));

        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVehicle(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteVehicleCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
