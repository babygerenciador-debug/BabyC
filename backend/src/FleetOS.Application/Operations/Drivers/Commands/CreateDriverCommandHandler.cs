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
    private readonly IUserRepository _userRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordService _passwordService;
    private readonly IFleetNotificationService _notificationService;

    public CreateDriverCommandHandler(
        IUserRepository userRepository,
        IDriverRepository driverRepository,
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

        var existingEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmail is not null)
        {
            var existingDriver = await _driverRepository.GetByUserIdAsync(existingEmail.Id, cancellationToken);
            if (existingDriver is not null && existingDriver.Status == DriverStatus.Active)
                return Result.Failure<Guid>(Error.Validation("Driver.EmailAlreadyExists", "Este email já está em uso por outro motorista."));

            if (existingDriver is not null)
                _driverRepository.Remove(existingDriver);
            _userRepository.Remove(existingEmail);
        }

        var existingCpf = await _userRepository.GetByCpfHashAsync(_tenantContext.TenantId, cpfHash, cancellationToken);
        if (existingCpf is not null)
        {
            var existingDriver = await _driverRepository.GetByUserIdAsync(existingCpf.Id, cancellationToken);
            if (existingDriver is not null && existingDriver.Status == DriverStatus.Active)
                return Result.Failure<Guid>(Error.Validation("Driver.CpfAlreadyExists", "Este CPF já está cadastrado."));

            if (existingDriver is not null)
                _driverRepository.Remove(existingDriver);
            _userRepository.Remove(existingCpf);
        }

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