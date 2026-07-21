using FleetOS.Api.Controllers;
using FleetOS.Application.Finance.Commands;
using FleetOS.Application.Finance.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Finance;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/finance/categories")]
public sealed class FinanceCategoriesController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateFinancialCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Created($"/api/v1/finance/categories/{result.Value}", result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetFinancialCategoriesQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetFinancialCategoryByIdQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateFinancialCategoryCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(FleetOS.Shared.Results.Error.Validation("Category.IdMismatch", "The ID in the URL must match the ID in the body."));

        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteFinancialCategoryCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
