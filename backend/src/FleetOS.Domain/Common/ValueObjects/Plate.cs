using FleetOS.Shared.Results;
using System.Text.RegularExpressions;

namespace FleetOS.Domain.Common.ValueObjects;

/// <summary>
/// Vehicle plate Value Object.
/// Supports Brazilian format: ABC-1234 (old) and ABC1D23 (Mercosul new).
/// </summary>
public sealed class Plate : ValueObject
{
    private static readonly Regex OldFormat = new(@"^[A-Z]{3}-?\d{4}$", RegexOptions.Compiled);
    private static readonly Regex MercosulFormat = new(@"^[A-Z]{3}\d[A-Z]\d{2}$", RegexOptions.Compiled);

    public string Value { get; }

    private Plate(string value) => Value = value;

    public static Result<Plate> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result.Failure<Plate>(Error.Validation("Plate", "Vehicle plate is required."));

        var normalized = input.ToUpperInvariant().Replace("-", "").Trim();

        if (normalized.Length < 7 || normalized.Length > 8)
            return Result.Failure<Plate>(Error.Validation("Plate", "Invalid plate format."));

        var formatted = normalized.Length == 7
            ? $"{normalized[..3]}-{normalized[3..]}"
            : normalized;

        if (!OldFormat.IsMatch(formatted) && !MercosulFormat.IsMatch(normalized))
            return Result.Failure<Plate>(Error.Validation("Plate", "Invalid plate format. Use ABC-1234 or ABC1D23."));

        return Result.Success<Plate>(new Plate(normalized));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
