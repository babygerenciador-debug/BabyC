using FluentValidation;

namespace FleetOS.Application.Fleet.Fuel.Commands;

public sealed class CreateFuelLogCommandValidator : AbstractValidator<CreateFuelLogCommand>
{
    public CreateFuelLogCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Odometer).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Liters).GreaterThan(0);
        RuleFor(x => x.TotalCost).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}
