using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Users;
using FleetOS.Domain.Operations.Drivers;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Operations.Drivers.Commands;

internal sealed class DeleteDriverCommandHandler : IRequestHandler<DeleteDriverCommand, Result>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IFleetNotificationService _notificationService;

    public DeleteDriverCommandHandler(
        IDriverRepository driverRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IFleetNotificationService notificationService)
    {
        _driverRepository = driverRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _notificationService = notificationService;
    }

    public async Task<Result> Handle(DeleteDriverCommand request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByIdAsync(request.Id, cancellationToken);
        if (driver is null)
            return Result.Failure(Error.NotFound("Driver.NotFound", "Driver not found."));

        _driverRepository.Remove(driver);

        var user = await _userRepository.GetByIdAsync(driver.UserId, cancellationToken);
        if (user is not null)
            _userRepository.Remove(user);

        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyDriverUpdatedAsync(request.Id, cancellationToken);

        return Result.Success();
    }
}
