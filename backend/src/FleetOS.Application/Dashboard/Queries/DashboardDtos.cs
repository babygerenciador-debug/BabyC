namespace FleetOS.Application.Dashboard.Queries;

public sealed record DashboardSummaryDto(
    int TotalVehicles,
    int AvailableVehicles,
    int InTripVehicles,
    int InMaintenanceVehicles,
    int TotalTripsThisMonth,
    int OngoingTrips,
    int LowStockItemsCount,
    decimal MonthRevenues,
    decimal MonthExpenses,
    decimal MonthBalance,
    decimal MonthRealProfit
);
