using FleetOS.Api.Errors;
using FleetOS.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace FleetOS.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(
        this Result result,
        ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok();
        }

        return new ObjectResult(result.Error)
        {
            StatusCode = ErrorStatusCodeMapper.Map(result.Error)
        };
    }


    public static IActionResult ToActionResult<T>(
        this Result<T> result,
        ControllerBase controller)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return new ObjectResult(result.Error)
        {
            StatusCode = ErrorStatusCodeMapper.Map(result.Error)
        };
    }
}