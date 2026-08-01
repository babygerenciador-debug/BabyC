using FluentValidation;

namespace FleetOS.Application.Auth.Commands.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Identifier is required.")
            .MaximumLength(256).WithMessage("Identifier must not exceed 256 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.");

        // If identifier looks like a CPF (all digits, length 11), TenantSlug must be provided
        RuleFor(x => x.TenantSlug)
            .NotEmpty()
            .When(x => IsCpf(x.Identifier))
            .WithMessage("Tenant is required for driver login.");
    }

    private static bool IsCpf(string identifier)
    {
        var digitsOnly = new string(identifier.Where(char.IsDigit).ToArray());
        return digitsOnly.Length == 11;
    }
}
