using FluentValidation;

namespace FleetOS.Application.Fleet.Vehicles.Commands;

public sealed class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
{
    public CreateVehicleCommandValidator()
    {
        RuleFor(x => x.LicensePlate).NotEmpty().MinimumLength(7).MaximumLength(10);
        RuleFor(x => x.Nickname).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Chassi).MaximumLength(50);
        RuleFor(x => x.Brand).MaximumLength(100);
        RuleFor(x => x.Model).MaximumLength(100);
        RuleFor(x => x.Color).MaximumLength(50);
        RuleFor(x => x.Renavam).MaximumLength(20);
        RuleFor(x => x.AnttNumber).MaximumLength(50);
        RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
        RuleFor(x => x.Year).InclusiveBetween(1980, 2100).When(x => x.Year.HasValue);
    }
}
