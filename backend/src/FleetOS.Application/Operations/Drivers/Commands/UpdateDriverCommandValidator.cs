using FluentValidation;

namespace FleetOS.Application.Operations.Drivers.Commands;

public sealed class UpdateDriverCommandValidator : AbstractValidator<UpdateDriverCommand>
{
    public UpdateDriverCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CnhNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.CnhCategory).NotEmpty().MaximumLength(5);
        RuleFor(x => x.CnhExpirationDate).NotEmpty().GreaterThan(DateTime.UtcNow);
    }
}
