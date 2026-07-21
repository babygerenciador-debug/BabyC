using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Users;
using FleetOS.Domain.Operations.Drivers;
using FleetOS.Shared.Results;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace FleetOS.Application.Operations.Drivers.Commands;

internal sealed class CreateDriverCommandHandler : IRequestHandler<CreateDriverCommand, Result<Guid>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Driver> _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordService _passwordService;
    private readonly IFleetNotificationService _notificationService;

    public CreateDriverCommandHandler(
        IRepository<User> userRepository,
        IRepository<Driver> driverRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        IPasswordService passwordService,
        IFleetNotificationService notificationService)
    {
        _userRepository = userRepository;
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _passwordService = passwordService;
        _notificationService = notificationService;
    }

    public async Task<Result<Guid>> Handle(
        CreateDriverCommand request,
        CancellationToken cancellationToken)
    {
        var cpfHash = HashCpf(request.Cpf);
        var cpfLast4 = request.Cpf.Length >= 4 ? request.Cpf[^4..] : request.Cpf;

        var emailResult = FleetOS.Domain.Common.ValueObjects.Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);

        var passwordHash = _passwordService.HashPassword(request.Password);

        
        var user = User.CreateDriverUser(
            _tenantContext.TenantId,
            _tenantContext.OrganizationId,
            _tenantContext.BusinessUnitId,
            request.Name,
            emailResult.Value!, 
            passwordHash,
            cpfHash,
            cpfLast4);

        var driverResult = Driver.Create(
            _tenantContext.TenantId,
            _tenantContext.OrganizationId,
            _tenantContext.BusinessUnitId,
            user.Id,
            request.CnhNumber,
            request.CnhCategory,
            request.CnhExpirationDate);

        if (driverResult.IsFailure)
            return Result.Failure<Guid>(driverResult.Error);

        var driver = driverResult.Value!;

        await _userRepository.AddAsync(user, cancellationToken);
        await _driverRepository.AddAsync(driver, cancellationToken);
        await _unitOfWork.CommitAsync(_tenantContext.TenantId, cancellationToken);

        await _notificationService.NotifyDriverCreatedAsync(driver.Id, cancellationToken);

        return Result.Success(driver.Id);
    }

    private static string HashCpf(string cpf)
    {
        var normalized = new string(cpf.Where(char.IsDigit).ToArray());
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToBase64String(bytes);
    }
}