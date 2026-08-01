using FleetOS.Api.Controllers;
using FleetOS.Application.Notifications.Commands;
using FleetOS.Application.Notifications.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Notifications;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager,Driver")]
[Route("api/v1/[controller]")]
public sealed class NotificationsController : BaseController
{

    [HttpGet("my")]
    public async Task<IActionResult> GetMyNotifications(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetMyNotificationsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new MarkNotificationAsReadCommand(id), cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }
}
