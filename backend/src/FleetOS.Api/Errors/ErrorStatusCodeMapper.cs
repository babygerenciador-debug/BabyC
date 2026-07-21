using FleetOS.Shared.Results;

namespace FleetOS.Api.Errors;

public static class ErrorStatusCodeMapper
{
    public static int Map(Error error)
    {
        return error.Code switch
        {
            // Authentication
            "Auth.InvalidCredentials" => StatusCodes.Status401Unauthorized,
            "Auth.InvalidToken" => StatusCodes.Status401Unauthorized,
            "Auth.TokenExpired" => StatusCodes.Status401Unauthorized,

            "Auth.UserBlocked" => StatusCodes.Status403Forbidden,
            "Auth.UserDisabled" => StatusCodes.Status403Forbidden,


            // Authorization
            "Auth.Forbidden" => StatusCodes.Status403Forbidden,


            // Not Found
            var code when code.EndsWith(".NotFound") =>
                StatusCodes.Status404NotFound,


            // Conflicts
            var code when code.EndsWith(".Conflict") =>
                StatusCodes.Status409Conflict,


            // Validation
            var code when code.StartsWith("Validation.") =>
                StatusCodes.Status400BadRequest,


            // Default
            _ =>
                StatusCodes.Status400BadRequest
        };
    }
}