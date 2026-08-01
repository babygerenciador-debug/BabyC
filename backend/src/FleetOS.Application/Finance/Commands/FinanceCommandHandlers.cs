using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Finance;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Finance.Commands;

internal sealed class CreateCostCenterCommandHandler : IRequestHandler<CreateCostCenterCommand, Result<Guid>>
{
    private readonly ICostCenterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public CreateCostCenterCommandHandler(ICostCenterRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext, IFleetNotificationService notificationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(CreateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var result = CostCenter.Create(_tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId, request.Name, request.Description);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await _repository.AddAsync(result.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyDashboardUpdateAsync(cancellationToken);

        return Result.Success(result.Value!.Id);
    }
}

internal sealed class CreateFinancialCategoryCommandHandler : IRequestHandler<CreateFinancialCategoryCommand, Result<Guid>>
{
    private readonly IFinancialCategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public CreateFinancialCategoryCommandHandler(IFinancialCategoryRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext, IFleetNotificationService notificationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(CreateFinancialCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = FinancialCategory.Create(_tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId, request.Name, request.Type);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await _repository.AddAsync(result.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyDashboardUpdateAsync(cancellationToken);

        return Result.Success(result.Value!.Id);
    }
}

internal sealed class OpenFinancialMonthCommandHandler : IRequestHandler<OpenFinancialMonthCommand, Result<Guid>>
{
    private readonly IFinancialMonthRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public OpenFinancialMonthCommandHandler(IFinancialMonthRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext, IFleetNotificationService notificationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(OpenFinancialMonthCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetOpenMonthAsync(cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>(Error.Validation("Month.AlreadyOpen", "There is already an open month. Close it before opening a new one."));

        var month = FinancialMonth.Open(
            _tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId,
            request.Year, request.Month, request.OwnerSalary);

        await _repository.AddAsync(month, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyDashboardUpdateAsync(cancellationToken);

        return Result.Success(month.Id);
    }
}

internal sealed class ActivateFinancialMonthCommandHandler : IRequestHandler<ActivateFinancialMonthCommand, Result>
{
    private readonly IFinancialMonthRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public ActivateFinancialMonthCommandHandler(IFinancialMonthRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext, IFleetNotificationService notificationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(ActivateFinancialMonthCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetOpenMonthAsync(cancellationToken);
        if (existing is not null)
            return Result.Failure(Error.Validation("Month.AlreadyOpen", "There is already an open month. Close it before opening a new one."));

        var month = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (month is null)
            return Result.Failure(Error.NotFound("Month.NotFound", "Financial month not found."));

        if (month.OwnerSalary <= 0)
            return Result.Failure(Error.Validation("Month.NoSalary", "Set the owner salary before opening the month."));

        month.Activate();
        _repository.Update(month);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyDashboardUpdateAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class CloseFinancialMonthCommandHandler : IRequestHandler<CloseFinancialMonthCommand, Result>
{
    private readonly IFinancialMonthRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public CloseFinancialMonthCommandHandler(IFinancialMonthRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext, IFleetNotificationService notificationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(CloseFinancialMonthCommand request, CancellationToken cancellationToken)
    {
        var month = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (month is null)
            return Result.Failure(Error.NotFound("Month.NotFound", "Financial month not found."));

        month.Close();
        _repository.Update(month);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyDashboardUpdateAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class RegisterTransactionCommandHandler : IRequestHandler<RegisterTransactionCommand, Result<Guid>>
{
    private readonly IFinancialTransactionRepository _repository;
    private readonly IFinancialCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public RegisterTransactionCommandHandler(IFinancialTransactionRepository repository, IFinancialCategoryRepository categoryRepository, IUnitOfWork unitOfWork, ITenantContext tenantContext, IFleetNotificationService notificationService)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(RegisterTransactionCommand request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category is null) return Result.Failure<Guid>(Error.NotFound("Category.NotFound", "Category not found."));

        if (category.Type != request.Type)
            return Result.Failure<Guid>(Error.Validation("Transaction.TypeMismatch", "Transaction type must match category type."));

        var result = FinancialTransaction.Create(_tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId, request.CategoryId, request.CostCenterId, request.FinancialMonthId, request.Type, request.Amount, request.Date, request.Description, request.ReferenceId);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        var transaction = result.Value!;

        if (request.Status == TransactionStatus.Paid)
        {
            var payResult = transaction.Pay(request.Date);
            if (payResult.IsFailure) return Result.Failure<Guid>(payResult.Error);
        }

        await _repository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);
        
        await _notificationService.NotifyTransactionCreatedAsync(transaction.Id, cancellationToken);
        
        return Result.Success(transaction.Id);
    }
}

internal sealed class PayTransactionCommandHandler : IRequestHandler<PayTransactionCommand, Result>
{
    private readonly IFinancialTransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public PayTransactionCommandHandler(IFinancialTransactionRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext, IFleetNotificationService notificationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(PayTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (transaction is null) return Result.Failure(Error.NotFound("Transaction.NotFound", "Transaction not found."));

        var payResult = transaction.Pay(request.PaymentDate);
        if (payResult.IsFailure) return payResult;

        _repository.Update(transaction);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);
        
        await _notificationService.NotifyTransactionUpdatedAsync(transaction.Id, cancellationToken);
        
        return Result.Success();
    }
}

internal sealed class DeleteFinancialCategoryCommandHandler : IRequestHandler<DeleteFinancialCategoryCommand, Result>
{
    private readonly IFinancialCategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public DeleteFinancialCategoryCommandHandler(IFinancialCategoryRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext, IFleetNotificationService notificationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(DeleteFinancialCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (category is null) return Result.Failure(Error.NotFound("Category.NotFound", "Category not found."));

        _repository.Remove(category);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyDashboardUpdateAsync(cancellationToken);

        return Result.Success();
    }
}

internal sealed class DeleteTransactionCommandHandler : IRequestHandler<DeleteTransactionCommand, Result>
{
    private readonly IFinancialTransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public DeleteTransactionCommandHandler(IFinancialTransactionRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext, IFleetNotificationService notificationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (transaction is null) return Result.Failure(Error.NotFound("Transaction.NotFound", "Transaction not found."));

        _repository.Remove(transaction);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyTransactionUpdatedAsync(transaction.Id, cancellationToken);

        return Result.Success();
    }
}

internal sealed class CancelTransactionCommandHandler : IRequestHandler<CancelTransactionCommand, Result>
{
    private readonly IFinancialTransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public CancelTransactionCommandHandler(IFinancialTransactionRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext, IFleetNotificationService notificationService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (transaction is null) return Result.Failure(Error.NotFound("Transaction.NotFound", "Transaction not found."));

        var cancelResult = transaction.Cancel();
        if (cancelResult.IsFailure) return cancelResult;

        _repository.Update(transaction);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);
        
        await _notificationService.NotifyTransactionUpdatedAsync(transaction.Id, cancellationToken);
        
        return Result.Success();
    }
}
