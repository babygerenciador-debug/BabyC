using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Commands;

internal sealed class StartTripCommandHandler : IRequestHandler<StartTripCommand, Result>
{
    private readonly ITripRepository _tripRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public StartTripCommandHandler(
        ITripRepository tripRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _tripRepository = tripRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(StartTripCommand request, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetByIdAsync(request.Id, cancellationToken);
        if (trip is null)
            return Result.Failure(Error.NotFound("Trip.NotFound", "Trip not found."));

        var result = trip.StartTrip();
        if (result.IsFailure)
            return result;

        _tripRepository.Update(trip);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyTripUpdatedAsync(trip.Id, cancellationToken);

        return Result.Success();
    }
}
