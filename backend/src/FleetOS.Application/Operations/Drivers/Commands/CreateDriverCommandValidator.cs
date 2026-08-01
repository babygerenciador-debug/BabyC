using FluentValidation;

namespace FleetOS.Application.Operations.Drivers.Commands;

public sealed class CreateDriverCommandValidator : AbstractValidator<CreateDriverCommand>
{
    public CreateDriverCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        RuleFor(x => x.Cpf).NotEmpty().MinimumLength(11).MaximumLength(14);
        RuleFor(x => x.CnhNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CnhCategory).NotEmpty().MaximumLength(5);
        RuleFor(x => x.CnhExpirationDate).NotEmpty().GreaterThan(DateTime.UtcNow);
    }
}
