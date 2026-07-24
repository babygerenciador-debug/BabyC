using FleetOS.Domain.Finance;

namespace FleetOS.Application.Finance;

public sealed record CostCenterDto(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record FinancialCategoryDto(
    Guid Id,
    string Name,
    TransactionType Type,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record FinancialTransactionDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    Guid? CostCenterId,
    string? CostCenterName,
    TransactionType Type,
    decimal Amount,
    DateTime Date,
    DateTime? PaymentDate,
    string Description,
    TransactionStatus Status,
    Guid? ReferenceId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CashFlowSummaryDto(
    decimal OwnerSalary,
    decimal OwnerTaxAmount,
    decimal NetOwnerSalary,
    decimal TotalRevenues,
    decimal TotalExpenses,
    decimal NetBalance);

public sealed record FinanceSettingsDto(
    decimal OwnerSalary);
