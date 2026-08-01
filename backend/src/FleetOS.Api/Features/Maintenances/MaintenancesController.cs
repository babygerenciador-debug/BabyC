using FleetOS.Api.Controllers;
using FleetOS.Application.Fleet.Maintenance.Commands;
using FleetOS.Application.Fleet.Maintenance.Queries;
using FleetOS.Domain.Fleet.Maintenance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Maintenances;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/[controller]")]
public sealed class MaintenancesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateMaintenance(
        [FromBody] CreateMaintenanceCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Created($"/api/v1/maintenances/{result.Value}", result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetMaintenances(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? vehicleId = null,
        [FromQuery] MaintenanceType? type = null,
        [FromQuery] MaintenanceStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMaintenancesQuery(page, pageSize, vehicleId, type, status);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMaintenanceById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMaintenanceByIdQuery(id), cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMaintenance(
        Guid id,
        [FromBody] UpdateMaintenanceCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(FleetOS.Shared.Results.Error.Validation("Maintenance.IdMismatch", "The ID in the URL must match the ID in the body."));

        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMaintenance(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteMaintenanceCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
