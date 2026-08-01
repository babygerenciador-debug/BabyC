using FleetOS.Api.Controllers;
using FleetOS.Application.Finance.Commands;
using FleetOS.Application.Finance.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Finance;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/finance/settings")]
public sealed class SettingsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetFinanceSettingsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateSettings(
        [FromBody] UpdateFinanceSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
