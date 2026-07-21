using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Fleet.VehicleIssues;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.VehicleIssues.Commands;

public record UpdateIssueStatusCommand(Guid IssueId, IssueStatus NewStatus) : IRequest<Result>;

internal sealed class UpdateIssueStatusCommandHandler : IRequestHandler<UpdateIssueStatusCommand, Result>
{
    private readonly IVehicleIssueReportRepository _issueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateIssueStatusCommandHandler(IVehicleIssueReportRepository issueRepository, IUnitOfWork unitOfWork)
    {
        _issueRepository = issueRepository;
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

        _issueRepository.Update(issue);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
