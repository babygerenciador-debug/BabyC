using FleetOS.Shared.Results;
using System.Text.RegularExpressions;

namespace FleetOS.Domain.Common.ValueObjects;

/// <summary>Email Value Object with format validation.</summary>
public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value.ToLowerInvariant().Trim();

    public static Result<Email> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result.Failure<Email>(Error.Validation("Email", "Email is required."));

        if (input.Length > 256)
            return Result.Failure<Email>(Error.Validation("Email", "Email must not exceed 256 characters."));

        if (!EmailRegex.IsMatch(input))
            return Result.Failure<Email>(Error.Validation("Email", "Invalid email format."));

        return Result.Success<Email>(new Email(input));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
