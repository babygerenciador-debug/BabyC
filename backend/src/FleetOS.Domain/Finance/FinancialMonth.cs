using FleetOS.Domain.Common;

namespace FleetOS.Domain.Finance;

public sealed class FinancialMonth : AggregateRoot
{
    private FinancialMonth() { }

    private FinancialMonth(
        Guid id,
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        int year,
        int monthNumber,
        decimal ownerSalary)
        : base(id, tenantId, organizationId, businessUnitId)
    {
        Year = year;
        MonthNumber = monthNumber;
        OwnerSalary = ownerSalary;
        Status = MonthStatus.Open;
        OpenedAt = DateTimeOffset.UtcNow;
    }

    public int Year { get; private set; }
    public int MonthNumber { get; private set; }
    public decimal OwnerSalary { get; private set; }
    public MonthStatus Status { get; private set; }
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }

    private readonly List<FinancialTransaction> _transactions = [];
    public IReadOnlyList<FinancialTransaction> Transactions => _transactions.AsReadOnly();

    public static FinancialMonth Open(
        Guid tenantId,
        Guid organizationId,
        Guid businessUnitId,
        int year,
        int monthNumber,
        decimal ownerSalary)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(monthNumber, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(monthNumber, 12);
        ArgumentOutOfRangeException.ThrowIfNegative(ownerSalary);

        return new FinancialMonth(
            Guid.NewGuid(), tenantId, organizationId, businessUnitId,
            year, monthNumber, ownerSalary);
    }

    public void Close()
    {
        if (Status == MonthStatus.Closed)
            throw new InvalidOperationException("Month is already closed.");

        Status = MonthStatus.Closed;
        ClosedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public string Label => $"{MonthName(MonthNumber)}/{Year}";

    private static string MonthName(int month) => month switch
    {
        1 => "Janeiro", 2 => "Fevereiro", 3 => "Março",
        4 => "Abril", 5 => "Maio", 6 => "Junho",
        7 => "Julho", 8 => "Agosto", 9 => "Setembro",
        10 => "Outubro", 11 => "Novembro", 12 => "Dezembro",
        _ => throw new ArgumentOutOfRangeException(nameof(month))
    };
}

public enum MonthStatus
{
    Open = 1,
    Closed = 2
}
