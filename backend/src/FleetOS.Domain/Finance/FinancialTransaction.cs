using FleetOS.Domain.Common;
using FleetOS.Shared.Results;

namespace FleetOS.Domain.Finance;

public sealed class FinancialTransaction : AggregateRoot
{
    private FinancialTransaction() { }

    private FinancialTransaction(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid categoryId,
        Guid? costCenterId,
        Guid financialMonthId,
        TransactionType type,
        decimal amount,
        DateTime date,
        string description,
        TransactionStatus status,
        Guid? referenceId)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        CategoryId = categoryId;
        CostCenterId = costCenterId;
        FinancialMonthId = financialMonthId;
        Type = type;
        Amount = amount;
        Date = date;
        Description = description;
        Status = status;
        ReferenceId = referenceId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid CategoryId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public Guid FinancialMonthId { get; private set; }
    public TransactionType Type { get; private set; }
    
    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }
    public DateTime? PaymentDate { get; private set; }
    
    public string Description { get; private set; } = default!;
    public TransactionStatus Status { get; private set; }
    
    public Guid? ReferenceId { get; private set; }

    public static Result<FinancialTransaction> Create(
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        Guid categoryId,
        Guid? costCenterId,
        Guid financialMonthId,
        TransactionType type,
        decimal amount,
        DateTime date,
        string description,
        Guid? referenceId = null)
    {
        if (amount <= 0)
            return Result.Failure<FinancialTransaction>(Error.Validation("FinancialTransaction.InvalidAmount", "Amount must be greater than zero."));

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<FinancialTransaction>(Error.Validation("FinancialTransaction.DescriptionRequired", "Description is required."));

        var transaction = new FinancialTransaction(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            categoryId, costCenterId, financialMonthId, type, amount, date, description.Trim(),
            TransactionStatus.Pending, referenceId);

        return Result.Success(transaction);
    }

    public Result Pay(DateTime paymentDate)
    {
        if (Status != TransactionStatus.Pending)
            return Result.Failure(Error.Validation("FinancialTransaction.InvalidStatus", "Only pending transactions can be paid."));

        Status = TransactionStatus.Paid;
        PaymentDate = paymentDate;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == TransactionStatus.Cancelled)
            return Result.Failure(Error.Validation("FinancialTransaction.InvalidStatus", "Transaction is already cancelled."));

        Status = TransactionStatus.Cancelled;
        UpdatedAt = DateTimeOffset.UtcNow;

        return Result.Success();
    }
}
