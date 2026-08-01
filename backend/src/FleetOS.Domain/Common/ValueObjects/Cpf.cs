using FleetOS.Shared.Results;

namespace FleetOS.Domain.Common.ValueObjects;

/// <summary>CPF Value Object — validates and normalizes Brazilian CPF.</summary>
public sealed class Cpf : ValueObject
{
    public string Value { get; }

    private Cpf(string value) => Value = value;

    public static Result<Cpf> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Result.Failure<Cpf>(Error.Validation("CPF", "CPF is required."));

        var digits = new string(input.Where(char.IsDigit).ToArray());

        if (digits.Length != 11)
            return Result.Failure<Cpf>(Error.Validation("CPF", "CPF must contain 11 digits."));

        if (digits.Distinct().Count() == 1)
            return Result.Failure<Cpf>(Error.Validation("CPF", "Invalid CPF."));

        if (!IsValidCpf(digits))
            return Result.Failure<Cpf>(Error.Validation("CPF", "Invalid CPF checksum."));

        return Result.Success<Cpf>(new Cpf(digits));
    }

    public string Formatted => $"{Value[..3]}.{Value[3..6]}.{Value[6..9]}-{Value[9..11]}";

    private static bool IsValidCpf(string digits)
    {
        static int CalcDigit(string d, int len)
        {
            int sum = 0;
            for (int i = 0; i < len; i++)
                sum += (d[i] - '0') * (len + 1 - i);
            int rem = (sum * 10) % 11;
            return rem == 10 ? 0 : rem;
        }
        return CalcDigit(digits, 9) == digits[9] - '0'
            && CalcDigit(digits, 10) == digits[10] - '0';
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Formatted;
}
