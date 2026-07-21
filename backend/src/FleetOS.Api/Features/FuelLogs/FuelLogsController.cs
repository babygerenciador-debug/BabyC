using FleetOS.Api.Controllers;
using FleetOS.Application.Fleet.Fuel.Commands;
using FleetOS.Application.Fleet.Fuel.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Features.FuelLogs;

[Authorize(Roles = "SystemAdmin,TenantAdmin,Manager")]
[Route("api/v1/[controller]")]
public sealed class FuelLogsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateFuelLog(
        [FromBody] CreateFuelLogCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess 
            ? Created($"/api/v1/fuellogs/{result.Value}", result.Value)
            : BadRequest(result.Error);
    }

    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".pdf"];

    [HttpPost("upload-receipt")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadReceipt(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(FleetOS.Shared.Results.Error.Validation("File.Empty", "File cannot be empty."));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            return BadRequest(FleetOS.Shared.Results.Error.Validation("File.InvalidExtension", "Only .jpg, .jpeg, .png and .pdf files are allowed."));

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(FleetOS.Shared.Results.Error.Validation("File.TooLarge", "File size must be less than 10MB."));

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "receipts");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var request = HttpContext.Request;
        var publicUrl = $"{request.Scheme}://{request.Host}/receipts/{uniqueFileName}";
        
        return Ok(new { Url = publicUrl });
    }

    [HttpGet]
    public async Task<IActionResult> GetFuelLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? vehicleId = null,
        [FromQuery] Guid? driverId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFuelLogsQuery(page, pageSize, vehicleId, driverId, startDate, endDate);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFuelLogById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetFuelLogByIdQuery(id), cancellationToken);
        
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateFuelLog(
        Guid id,
        [FromBody] UpdateFuelLogCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(FleetOS.Shared.Results.Error.Validation("FuelLog.IdMismatch", "The ID in the URL must match the ID in the body."));

        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFuelLog(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFuelLogCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }
}
