using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Finance;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Trips.Commands;

internal sealed class PayTripCommandHandler : IRequestHandler<PayTripCommand, Result>
{
    private readonly ITripRepository _tripRepository;
    private readonly IFinancialTransactionRepository _transactionRepository;
    private readonly IFinancialCategoryRepository _categoryRepository;
    private readonly IFinancialMonthRepository _monthRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public PayTripCommandHandler(
        ITripRepository tripRepository,
        IFinancialTransactionRepository transactionRepository,
        IFinancialCategoryRepository categoryRepository,
        IFinancialMonthRepository monthRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _tripRepository = tripRepository;
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _monthRepository = monthRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(PayTripCommand request, CancellationToken cancellationToken)
    {
        var trip = await _tripRepository.GetByIdAsync(request.Id, cancellationToken);
        if (trip is null)
            return Result.Failure(Error.NotFound("Trip.NotFound", "Trip not found."));

        var result = trip.MarkAsPaid();
        if (result.IsFailure)
            return result;

        // Criar transação financeira
        if (trip.TripValue > 0)
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
                var openMonth = await _monthRepository.GetOpenMonthAsync(cancellationToken);
                if (openMonth is null) return Result.Failure(Error.Validation("Month.NoOpenMonth", "No open financial month."));

                var transaction = FinancialTransaction.Create(
                    _tenantContext.TenantId,
                    _tenantContext.OrganizationId,
                    _tenantContext.BusinessUnitId,
                    revenueCategory.Id,
                    null,
                    openMonth.Id,
                    TransactionType.Revenue,
                    trip.TripValue,
                    trip.ActualEndDate ?? trip.ScheduledEndDate,
                    $"Viagem: {trip.Origin} → {trip.Destination}",
                    trip.Id);

                if (transaction.IsSuccess)
                {
                    var tx = transaction.Value!;
                    tx.Pay(trip.ActualEndDate ?? trip.ScheduledEndDate);
                    await _transactionRepository.AddAsync(tx, cancellationToken);
                }
            }
        }

        _tripRepository.Update(trip);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyTripUpdatedAsync(trip.Id, cancellationToken);

        return Result.Success();
    }
}
