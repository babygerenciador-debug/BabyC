using FleetOS.Domain.Finance;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Finance.Commands;

public sealed record CreateCostCenterCommand(string Name, string? Description) : IRequest<Result<Guid>>;
public sealed record UpdateCostCenterCommand(Guid Id, string Name, string? Description) : IRequest<Result>;

public sealed record CreateFinancialCategoryCommand(string Name, TransactionType Type) : IRequest<Result<Guid>>;
public sealed record UpdateFinancialCategoryCommand(Guid Id, string Name) : IRequest<Result>;

public sealed record RegisterTransactionCommand(
    Guid CategoryId,
    Guid? CostCenterId,
    TransactionType Type,
    decimal Amount,
    DateTime Date,
    string Description,
    Guid? ReferenceId) : IRequest<Result<Guid>>;

public sealed record PayTransactionCommand(Guid Id, DateTime PaymentDate) : IRequest<Result>;
public sealed record CancelTransactionCommand(Guid Id) : IRequest<Result>;
