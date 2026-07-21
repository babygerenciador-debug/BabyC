using FleetOS.Api.Controllers;
using FleetOS.Application.Inventory.Commands;
using FleetOS.Application.Inventory.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Inventory;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/inventory/[controller]")]
public sealed class ProductsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Created($"/api/v1/inventory/products/{result.Value}", result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(page, pageSize, searchTerm, categoryId);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(FleetOS.Shared.Results.Error.Validation("Product.IdMismatch", "The ID in the URL must match the ID in the body."));

        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteProductCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
