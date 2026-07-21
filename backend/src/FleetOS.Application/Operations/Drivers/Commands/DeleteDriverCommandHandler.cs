using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Operations.Drivers;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Commands;

internal sealed class DeleteDriverCommandHandler : IRequestHandler<DeleteDriverCommand, Result>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteDriverCommandHandler(
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(DeleteDriverCommand request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByIdAsync(request.Id, cancellationToken);
        if (driver is null)
            return Result.Failure(Error.NotFound("Driver.NotFound", "Driver not found."));

        driver.UpdateStatus(DriverStatus.Inactive);
        
        _driverRepository.Update(driver);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        return Result.Success();
    }
}
