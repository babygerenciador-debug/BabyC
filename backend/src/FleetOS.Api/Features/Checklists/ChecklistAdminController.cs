using FleetOS.Api.Controllers;
using FleetOS.Application.Operations.Checklists.Commands;
using FleetOS.Application.Operations.Checklists.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Checklists;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/checklist-admin")]
public sealed class ChecklistAdminController : BaseController
{
    [HttpGet("items")]
    public async Task<IActionResult> GetItems(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetChecklistItemsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItem(
        [FromBody] CreateChecklistItemCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? Created($"/api/v1/checklist-admin/items/{result.Value}", result.Value) : BadRequest(result.Error);
    }

    [HttpPut("items/{id:guid}")]
    public async Task<IActionResult> UpdateItem(
        Guid id,
        [FromBody] UpdateChecklistItemCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(FleetOS.Shared.Results.Error.Validation("ChecklistItem.IdMismatch", "ID mismatch."));

        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("items/{id:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteChecklistItemCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpGet("report")]
    public async Task<IActionResult> GetReport(
        [FromQuery] Guid? vehicleId,
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetChecklistReportQuery(vehicleId, startDate, endDate), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
