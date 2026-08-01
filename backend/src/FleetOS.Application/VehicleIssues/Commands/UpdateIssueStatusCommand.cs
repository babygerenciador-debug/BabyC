using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Finance;
using FleetOS.Domain.Fleet.VehicleIssues;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.VehicleIssues.Commands;

public record UpdateIssueStatusCommand(
    Guid IssueId, IssueStatus NewStatus,
    decimal? ExpenseAmount = null,
    string? ExpenseDescription = null
) : IRequest<Result>;

internal sealed class UpdateIssueStatusCommandHandler : IRequestHandler<UpdateIssueStatusCommand, Result>
{
    private readonly IVehicleIssueReportRepository _issueRepository;
    private readonly IFinancialTransactionRepository _transactionRepository;
    private readonly IFinancialCategoryRepository _categoryRepository;
    private readonly IFinancialMonthRepository _monthRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateIssueStatusCommandHandler(
        IVehicleIssueReportRepository issueRepository,
        IFinancialTransactionRepository transactionRepository,
        IFinancialCategoryRepository categoryRepository,
        IFinancialMonthRepository monthRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _issueRepository = issueRepository;
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _monthRepository = monthRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateIssueStatusCommand request, CancellationToken cancellationToken)
    {
        var issue = await _issueRepository.GetByIdAsync(request.IssueId, cancellationToken);
        if (issue == null)
            return Result.Failure(Error.NotFound("VehicleIssue.NotFound", "Issue report not found."));

        var updateResult = issue.UpdateStatus(request.NewStatus);
        if (updateResult.IsFailure)
            return updateResult;

        if (request.NewStatus == IssueStatus.Resolved && request.ExpenseAmount.HasValue && !string.IsNullOrWhiteSpace(request.ExpenseDescription))
        {
            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            var expenseCategory = categories.FirstOrDefault(c => c.Type == TransactionType.Expense);

            if (expenseCategory == null)
            {
                var createResult = FinancialCategory.Create(
                    _tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId,
                    "Manutenção/Reparos", TransactionType.Expense);
                if (createResult.IsFailure)
                    return Result.Failure(createResult.Error);
                expenseCategory = createResult.Value!;
                await _categoryRepository.AddAsync(expenseCategory, cancellationToken);
            }

            var openMonth = await _monthRepository.GetOpenMonthAsync(cancellationToken);
            if (openMonth is null) return Result.Failure(Error.Validation("Month.NoOpenMonth", "No open financial month."));

            var txResult = FinancialTransaction.Create(
                _tenantContext.TenantId, _tenantContext.OrganizationId, _tenantContext.BusinessUnitId,
                expenseCategory.Id, null, openMonth.Id, TransactionType.Expense,
                request.ExpenseAmount.Value, DateTime.UtcNow, request.ExpenseDescription,
                issue.Id);
            if (txResult.IsFailure)
                return Result.Failure(txResult.Error);

            var transaction = txResult.Value!;
            var payResult = transaction.Pay(DateTime.UtcNow);
            if (payResult.IsFailure)
                return Result.Failure(payResult.Error);

            await _transactionRepository.AddAsync(transaction, cancellationToken);
        }

        _issueRepository.Update(issue);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
