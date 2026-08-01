using FluentValidation;

namespace FleetOS.Application.Operations.Trips.Commands;

public sealed class SwapTripVehicleCommandValidator : AbstractValidator<SwapTripVehicleCommand>
{
    public SwapTripVehicleCommandValidator()
    {
        RuleFor(x => x.TripId).NotEmpty();
        RuleFor(x => x.NewVehicleId).NotEmpty();
    }
}
