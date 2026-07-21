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

    public CreateCostCenterCommandHandler(ICostCenterRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var result = CostCenter.Create(_tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId, request.Name, request.Description);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await _repository.AddAsync(result.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);
        return Result.Success(result.Value!.Id);
    }
}

internal sealed class CreateFinancialCategoryCommandHandler : IRequestHandler<CreateFinancialCategoryCommand, Result<Guid>>
{
    private readonly IFinancialCategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateFinancialCategoryCommandHandler(IFinancialCategoryRepository repository, IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateFinancialCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = FinancialCategory.Create(_tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId, request.Name, request.Type);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await _repository.AddAsync(result.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);
        return Result.Success(result.Value!.Id);
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

        var result = FinancialTransaction.Create(_tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId, request.CategoryId, request.CostCenterId, request.Type, request.Amount, request.Date, request.Description, request.ReferenceId);
        if (result.IsFailure) return Result.Failure<Guid>(result.Error);

        await _repository.AddAsync(result.Value!, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);
        
        await _notificationService.NotifyTransactionCreatedAsync(result.Value!.Id, cancellationToken);
        
        return Result.Success(result.Value!.Id);
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
