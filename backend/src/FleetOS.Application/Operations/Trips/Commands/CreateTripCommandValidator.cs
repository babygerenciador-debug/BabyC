using FluentValidation;

namespace FleetOS.Application.Operations.Trips.Commands;

public sealed class CreateTripCommandValidator : AbstractValidator<CreateTripCommand>
{
    public CreateTripCommandValidator()
    {
        RuleFor(x => x.DriverId).NotEmpty();
        RuleFor(x => x.VehicleId).NotEmpty();
        RuleFor(x => x.Origin).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(255);
        RuleFor(x => x.ScheduledStartDate).NotEmpty();
        RuleFor(x => x.ScheduledEndDate).NotEmpty();
        RuleFor(x => x.TripValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x).Must(x => x.ScheduledStartDate < x.ScheduledEndDate)
            .WithMessage("Scheduled end date must be after start date.");
    }
}
