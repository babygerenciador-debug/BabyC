using FluentValidation;

namespace FleetOS.Application.Fleet.Maintenance.Commands;

public sealed class UpdateMaintenanceCommandValidator : AbstractValidator<UpdateMaintenanceCommand>
{
    public UpdateMaintenanceCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TotalCost).GreaterThanOrEqualTo(0);
    }
}
