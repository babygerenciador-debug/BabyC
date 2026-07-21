namespace FleetOS.Domain.Finance;

public enum TransactionType
{
    Revenue = 1,
    Expense = 2
}

public enum TransactionStatus
{
    Pending = 1,
    Paid = 2,
    Cancelled = 3
}
