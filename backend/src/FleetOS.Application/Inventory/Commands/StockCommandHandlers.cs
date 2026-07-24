using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Inventory;
using FleetOS.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FleetOS.Application.Inventory.Commands;

internal sealed class ReceiveStockCommandHandler : IRequestHandler<ReceiveStockCommand, Result<Guid>>
{
    private readonly IStockBalanceRepository _stockRepository;
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;
    private readonly ILogger<ReceiveStockCommandHandler> _logger;

    public ReceiveStockCommandHandler(
        IStockBalanceRepository stockRepository, 
        IInventoryMovementRepository movementRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork, 
        ITenantContext tenantContext,
        IFleetNotificationService notificationService,
        ILogger<ReceiveStockCommandHandler> logger)
    {
        _stockRepository = stockRepository;
        _movementRepository = movementRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(ReceiveStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure<Guid>(Error.NotFound("Product.NotFound", "Product not found."));

        var balance = await _stockRepository.GetStockBalanceAsync(request.ProductId, request.LocationType, request.VehicleId, cancellationToken);
        bool isNewBalance = balance is null;
        if (balance is null)
            balance = StockBalance.Create(_tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId, request.ProductId, request.LocationType, request.VehicleId, 0, request.LocationType == LocationType.Vehicle ? 2 : 10).Value!;

        balance.AddQuantity(request.Quantity);

        if (isNewBalance)
            await _stockRepository.AddAsync(balance, cancellationToken);
        else
            _stockRepository.Update(balance);

        var movement = InventoryMovement.Create(
            _tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId,
            request.ProductId, MovementType.In, null, null, request.LocationType, request.VehicleId,
            request.Quantity, request.Date, request.Notes, request.ReferenceId).Value!;

        await _movementRepository.AddAsync(movement, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyStockUpdatedAsync(cancellationToken);

        return Result.Success(movement.Id);
    }
}

internal sealed class ConsumeStockCommandHandler : IRequestHandler<ConsumeStockCommand, Result<Guid>>
{
    private readonly IStockBalanceRepository _stockRepository;
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public ConsumeStockCommandHandler(
        IStockBalanceRepository stockRepository, 
        IInventoryMovementRepository movementRepository,
        IUnitOfWork unitOfWork, 
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _stockRepository = stockRepository;
        _movementRepository = movementRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(ConsumeStockCommand request, CancellationToken cancellationToken)
    {
        var balance = await _stockRepository.GetStockBalanceAsync(request.ProductId, request.LocationType, request.VehicleId, cancellationToken);
        if (balance is null)
            return Result.Failure<Guid>(Error.NotFound("StockBalance.NotFound", "Stock balance not found for this location."));

        var removeResult = balance.RemoveQuantity(request.Quantity);
        if (removeResult.IsFailure)
            return Result.Failure<Guid>(removeResult.Error);

        _stockRepository.Update(balance);

        var movement = InventoryMovement.Create(
            _tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId,
            request.ProductId, MovementType.Out, request.LocationType, request.VehicleId, null, null,
            request.Quantity, request.Date, request.Notes, request.ReferenceId).Value!;

        await _movementRepository.AddAsync(movement, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyStockUpdatedAsync(cancellationToken);

        return Result.Success(movement.Id);
    }
}

internal sealed class TransferStockCommandHandler : IRequestHandler<TransferStockCommand, Result<Guid>>
{
    private readonly IStockBalanceRepository _stockRepository;
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;
    private readonly ILogger<TransferStockCommandHandler> _logger;

    public TransferStockCommandHandler(
        IStockBalanceRepository stockRepository, 
        IInventoryMovementRepository movementRepository,
        IUnitOfWork unitOfWork, 
        ITenantContext tenantContext,
        IFleetNotificationService notificationService,
        ILogger<TransferStockCommandHandler> logger)
    {
        _stockRepository = stockRepository;
        _movementRepository = movementRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(TransferStockCommand request, CancellationToken cancellationToken)
    {
        var fromLocationType = request.FromVehicleId.HasValue ? LocationType.Vehicle : LocationType.Main;
        var toLocationType = request.ToVehicleId.HasValue ? LocationType.Vehicle : LocationType.Main;

        _logger.LogInformation("TransferStock: Product={ProductId}, From={FromLoc}/{FromVeh}, To={ToLoc}/{ToVeh}, Qty={Qty}",
            request.ProductId, fromLocationType, request.FromVehicleId, toLocationType, request.ToVehicleId, request.Quantity);

        var fromBalance = await _stockRepository.GetStockBalanceAsync(request.ProductId, fromLocationType, request.FromVehicleId, cancellationToken);
        if (fromBalance is null)
        {
            _logger.LogWarning("TransferStock: Source not found for Product={ProductId}, Loc={Loc}, Veh={Veh}",
                request.ProductId, fromLocationType, request.FromVehicleId);
            return Result.Failure<Guid>(Error.NotFound("StockBalance.SourceNotFound", "Source stock balance not found."));
        }

        _logger.LogInformation("TransferStock: Source found Id={Id}, Qty={Qty}", fromBalance.Id, fromBalance.Quantity);

        var removeResult = fromBalance.RemoveQuantity(request.Quantity);
        if (removeResult.IsFailure)
        {
            _logger.LogWarning("TransferStock: RemoveQuantity failed: {Error}", removeResult.Error);
            return Result.Failure<Guid>(removeResult.Error);
        }

        var toBalance = await _stockRepository.GetStockBalanceAsync(request.ProductId, toLocationType, request.ToVehicleId, cancellationToken);
        bool isNewToBalance = false;
        if (toBalance is null)
        {
            _logger.LogInformation("TransferStock: Destination not found, creating new balance");
            toBalance = StockBalance.Create(_tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId, request.ProductId, toLocationType, request.ToVehicleId, 0, toLocationType == LocationType.Vehicle ? 2 : 10).Value!;
            isNewToBalance = true;
        }
        else
        {
            _logger.LogInformation("TransferStock: Destination found Id={Id}, Qty={Qty}", toBalance.Id, toBalance.Quantity);
        }

        toBalance.AddQuantity(request.Quantity);

        _logger.LogInformation("TransferStock: After move — Source Qty={FromQty}, Dest Qty={ToQty}, isNew={IsNew}",
            fromBalance.Quantity, toBalance.Quantity, isNewToBalance);

        _stockRepository.Update(fromBalance);
        if (isNewToBalance)
            await _stockRepository.AddAsync(toBalance, cancellationToken);
        else
            _stockRepository.Update(toBalance);

        var movement = InventoryMovement.Create(
            _tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId,
            request.ProductId, MovementType.Transfer, fromLocationType, request.FromVehicleId, toLocationType, request.ToVehicleId,
            request.Quantity, request.Date, request.Notes, request.ReferenceId).Value!;

        await _movementRepository.AddAsync(movement, cancellationToken);
        var saved = await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);
        _logger.LogInformation("TransferStock: Saved {Count} rows", saved);

        await _notificationService.NotifyStockUpdatedAsync(cancellationToken);

        return Result.Success(movement.Id);
    }
}
