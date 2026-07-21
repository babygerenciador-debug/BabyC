using FleetOS.Api.Controllers;
using FleetOS.Application.Finance.Commands;
using FleetOS.Application.Finance.Queries;
using FleetOS.Domain.Finance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.Finance;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/finance/[controller]")]
public sealed class TransactionsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> RegisterTransaction(
        [FromBody] RegisterTransactionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Created($"/api/v1/finance/transactions/{result.Value}", result.Value)
            : BadRequest(result.Error);
    }

    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] TransactionStatus? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] TransactionType? type = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTransactionsQuery(page, pageSize, status, startDate, endDate, type);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] decimal ownerSalary,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCashFlowSummaryQuery(startDate, endDate, ownerSalary);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPatch("{id:guid}/pay")]
    public async Task<IActionResult> PayTransaction(
        Guid id,
        [FromBody] PayTransactionCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(FleetOS.Shared.Results.Error.Validation("Transaction.IdMismatch", "The ID in the URL must match the ID in the body."));

        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelTransaction(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CancelTransactionCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTransaction(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteTransactionCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
