using FleetOS.Api.Controllers;
using FleetOS.Application.Finance.Commands;
using FleetOS.Application.Finance.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Finance;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/finance/[controller]")]
public sealed class MonthsController : BaseController
{
    [HttpPost("open")]
    public async Task<IActionResult> OpenMonth(
        [FromBody] OpenFinancialMonthCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? Created($"/api/v1/finance/months/{result.Value}", result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> ActivateMonth(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ActivateFinancialMonthCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> CloseMonth(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new CloseFinancialMonthCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { status = "ok" });

    [HttpGet]
    public async Task<IActionResult> GetMonths(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetFinancialMonthsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("open")]
    public async Task<IActionResult> GetOpenMonth(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetOpenFinancialMonthQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}/report")]
    public async Task<IActionResult> GetMonthReport(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetFinancialMonthReportQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
