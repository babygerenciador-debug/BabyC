namespace FleetOS.Shared.Results;

/// <summary>
/// Represents a domain or application error with a code and description.
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static readonly Error NullValue =
        new("Error.NullValue", "A null value was provided.");


    // ─── Generic Errors ──────────────────────────────────────────────

    public static Error NotFound(string entity, object id) =>
        new($"{entity}.NotFound",
            $"{entity} with id '{id}' was not found.");


    public static Error Conflict(string entity, string detail) =>
        new($"{entity}.Conflict", detail);


    public static Error Validation(string field, string message) =>
        new($"Validation.{field}", message);


    public static Error Unauthorized(string detail = "Unauthorized.") =>
        new("Auth.Unauthorized", detail);


    public static Error Forbidden(string detail = "Access denied.") =>
        new("Auth.Forbidden", detail);


    public static Error BusinessRule(string code, string description) =>
        new($"BusinessRule.{code}", description);


    // ─── Domain-specific errors ──────────────────────────────────────

    public static class Auth
    {
        public static readonly Error InvalidCredentials =
            new(
                "Auth.InvalidCredentials",
                "Invalid email/CPF or password."
            );

        public static readonly Error UserBlocked =
            new(
                "Auth.UserBlocked",
                "This account has been blocked. Contact your administrator."
            );

        public static readonly Error UserDisabled =
            new(
                "Auth.UserDisabled",
                "This account has been disabled."
            );

        public static readonly Error TokenExpired =
            new(
                "Auth.TokenExpired",
                "The session token has expired."
            );

        public static readonly Error InvalidToken =
            new(
                "Auth.InvalidToken",
                "Invalid or revoked token."
            );
    }


    public static class Tenant
    {
        public static readonly Error NotFound =
            new(
                "Tenant.NotFound",
                "Tenant not found."
            );

        public static readonly Error Suspended =
            new(
                "Tenant.Suspended",
                "This tenant account has been suspended."
            );
    }


    public static class Driver
    {
        public static readonly Error CpfAlreadyExists =
            new(
                "Driver.CpfAlreadyExists",
                "A driver with this CPF already exists in this company."
            );

        public static readonly Error CnhExpired =
            new(
                "Driver.CnhExpired",
                "Driver's CNH is expired. Cannot start a trip."
            );

        public static readonly Error NotActive =
            new(
                "Driver.NotActive",
                "Driver is not active and cannot be assigned to trips."
            );

        public static readonly Error AlreadyOnTrip =
            new(
                "Driver.AlreadyOnTrip",
                "Driver is already assigned to an ongoing trip."
            );
    }


    public override string ToString() =>
        $"[{Code}] {Description}";
}