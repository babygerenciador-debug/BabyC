using FluentValidation;

namespace FleetOS.Application.Fleet.Maintenance.Commands;

public sealed class CreateMaintenanceCommandValidator : AbstractValidator<CreateMaintenanceCommand>
{
    public CreateMaintenanceCommandValidator()
    {
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Odometer).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TotalCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ProviderName).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
