using FleetOS.Api.Controllers;
using FleetOS.Application.Finance.Commands;
using FleetOS.Application.Finance.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Finance;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/finance/cost-centers")]
public sealed class CostCentersController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateCostCenter(
        [FromBody] CreateCostCenterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Created($"/api/v1/finance/cost-centers/{result.Value}", result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetCostCenters(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetCostCentersQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCostCenter(
        Guid id,
        [FromBody] UpdateCostCenterCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(FleetOS.Shared.Results.Error.Validation("CostCenter.IdMismatch", "The ID in the URL must match the ID in the body."));

        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
