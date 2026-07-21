using FleetOS.Api.Controllers;
using FleetOS.Application.Inventory.Commands;
using FleetOS.Application.Inventory.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Inventory;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/inventory/[controller]")]
public sealed class MovementsController : BaseController
{
    [HttpPost("receive")]
    public async Task<IActionResult> ReceiveStock(
        [FromBody] ReceiveStockCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Ok(new { MovementId = result.Value })
            : BadRequest(result.Error);
    }

    [HttpPost("consume")]
    public async Task<IActionResult> ConsumeStock(
        [FromBody] ConsumeStockCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Ok(new { MovementId = result.Value })
            : BadRequest(result.Error);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> TransferStock(
        [FromBody] TransferStockCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Ok(new { MovementId = result.Value })
            : BadRequest(result.Error);
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetMovementsByProduct(
        Guid productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMovementsByProductQuery(productId, page, pageSize);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
