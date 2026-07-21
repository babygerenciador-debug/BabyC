using FleetOS.Api.Controllers;
using FleetOS.Application.Operations.Drivers.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Drivers;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/[controller]")]
public sealed class DriversController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateDriver(
        [FromBody] CreateDriverCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Created($"/api/v1/drivers/{result.Value}", result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetDrivers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new FleetOS.Application.Operations.Drivers.Queries.GetDriversQuery(page, pageSize, searchTerm, status);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDriverById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new FleetOS.Application.Operations.Drivers.Queries.GetDriverByIdQuery(id), cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDriver(
        Guid id,
        [FromBody] UpdateDriverCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Id != Guid.Empty && id != command.Id)
            return BadRequest(FleetOS.Shared.Results.Error.Validation("Driver.IdMismatch", "The ID in the URL must match the ID in the body."));

        if (command.Id == Guid.Empty)
            command = command with { Id = id };

        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDriver(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteDriverCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
