using FleetOS.Api.Controllers;
using FleetOS.Application.VehicleIssues.Commands;
using FleetOS.Application.VehicleIssues.Queries;
using FleetOS.Domain.Fleet.VehicleIssues;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.VehicleIssues;

[Authorize]
[Route("api/v1/[controller]")]
public sealed class VehicleIssuesController : BaseController
{

    [HttpGet]
    [Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetVehicleIssuesQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> ReportIssue([FromBody] ReportIssueRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new ReportVehicleIssueCommand(request.VehicleId, request.Description), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateIssueStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new UpdateIssueStatusCommand(id, request.Status, request.ExpenseAmount, request.ExpenseDescription), cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}

public record ReportIssueRequest(Guid VehicleId, string Description);
public record UpdateIssueStatusRequest(IssueStatus Status, decimal? ExpenseAmount = null, string? ExpenseDescription = null);
