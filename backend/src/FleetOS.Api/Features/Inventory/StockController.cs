using FleetOS.Api.Controllers;
using FleetOS.Application.Inventory.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Inventory;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/inventory/[controller]")]
public sealed class StockController : BaseController
{
    [HttpGet("main")]
    public async Task<IActionResult> GetMainStock(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMainStockQuery(page, pageSize, searchTerm);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("vehicle/{vehicleId:guid}")]
    public async Task<IActionResult> GetVehicleStock(
        Guid vehicleId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVehicleStockQuery(vehicleId, page, pageSize, searchTerm);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetStockAlerts(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetStockAlertsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
