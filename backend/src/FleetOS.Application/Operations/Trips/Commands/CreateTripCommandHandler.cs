using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Finance;
using FleetOS.Domain.Operations.Trips;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Commands;

internal sealed class CreateTripCommandHandler : IRequestHandler<CreateTripCommand, Result<Guid>>
{
    private readonly ITripRepository _tripRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IFinancialTransactionRepository _transactionRepository;
    private readonly IFinancialCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public CreateTripCommandHandler(
        ITripRepository tripRepository,
        IDriverRepository driverRepository,
        IVehicleRepository vehicleRepository,
        IFinancialTransactionRepository transactionRepository,
        IFinancialCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _tripRepository = tripRepository;
        _driverRepository = driverRepository;
        _vehicleRepository = vehicleRepository;
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(CreateTripCommand request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByIdAsync(request.DriverId, cancellationToken);
        if (driver is null)
            return Result.Failure<Guid>(Error.NotFound("Driver.NotFound", "Driver not found."));

        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
        if (vehicle is null)
            return Result.Failure<Guid>(Error.NotFound("Vehicle.NotFound", "Vehicle not found."));

        var result = Trip.Create(
            _tenantContext.TenantId,
            _tenantContext.OrganizationId,
            _tenantContext.BusinessUnitId,
            request.DriverId,
            request.VehicleId,
            request.Origin,
            request.Destination,
            request.ScheduledStartDate,
            request.ScheduledEndDate,
            request.TripValue,
            request.PaymentStatus,
            request.Notes);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        await _tripRepository.AddAsync(result.Value!, cancellationToken);

        // Se a viagem já foi paga, criar transação financeira automaticamente
        if (request.PaymentStatus == PaymentStatus.Paid && request.TripValue > 0)
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            var revenueCategory = categories.FirstOrDefault(c => c.Type == TransactionType.Revenue && c.Name.ToLower().Contains("viagem"));

            if (revenueCategory is null)
            {
                var catResult = FinancialCategory.Create(
                    _tenantContext.TenantId,
                    _tenantContext.OrganizationId,
                    _tenantContext.BusinessUnitId,
                    "Viagens",
                    TransactionType.Revenue);
                if (catResult.IsSuccess && catResult.Value is not null)
                {
                    revenueCategory = catResult.Value;
                    await _categoryRepository.AddAsync(revenueCategory, cancellationToken);
                }
            }

            if (revenueCategory is not null)
            {
                var transaction = FinancialTransaction.Create(
                    _tenantContext.TenantId,
                    _tenantContext.OrganizationId,
                    _tenantContext.BusinessUnitId,
                    revenueCategory.Id,
                    null,
                    TransactionType.Revenue,
                    request.TripValue,
                    request.ScheduledEndDate,
                    $"Viagem: {request.Origin} → {request.Destination}",
                    result.Value!.Id);

                if (transaction.IsSuccess)
                {
                    var tx = transaction.Value!;
                    tx.Pay(request.ScheduledEndDate);
                    await _transactionRepository.AddAsync(tx, cancellationToken);
                }
            }
        }

        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyTripCreatedAsync(result.Value!.Id, cancellationToken);

        return Result.Success(result.Value!.Id);
    }
}
