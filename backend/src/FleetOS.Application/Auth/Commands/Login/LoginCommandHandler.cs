using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Tenants;
using FleetOS.Domain.Core.Users;
using FleetOS.Shared.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace FleetOS.Application.Auth.Commands.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IPasswordService passwordService,
    IJwtService jwtService,
    IUnitOfWork unitOfWork,
    IConfiguration configuration)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var isCpfLogin = IsCpf(request.Identifier);
        User? user;

        if (isCpfLogin)
        {
            if (string.IsNullOrWhiteSpace(request.TenantSlug))
                return Result.Failure<LoginResponse>(Error.Auth.InvalidCredentials);

            var tenant = await tenantRepository.GetBySlugAsync(request.TenantSlug, cancellationToken);
            if (tenant == null)
                return Result.Failure<LoginResponse>(Error.Auth.InvalidCredentials);

            var cpfHash = HashCpf(request.Identifier);
            user = await userRepository.GetByCpfHashAsync(tenant.Id, cpfHash, cancellationToken);
        }
        else
        {
            user = await userRepository.GetByEmailAsync(request.Identifier, cancellationToken);
        }

        if (user == null)
            return Result.Failure<LoginResponse>(Error.Auth.InvalidCredentials);

        if (user.IsLockedOut())
            return Result.Failure<LoginResponse>(Error.Auth.UserBlocked);

        if (!passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            var failedResult = user.RecordFailedLogin();
            await unitOfWork.CommitAsync(user.TenantId, user.Id, cancellationToken);
            if (failedResult.IsFailure && failedResult.Error.Code == "Auth.UserBlocked")
                return Result.Failure<LoginResponse>(failedResult.Error);
            return Result.Failure<LoginResponse>(Error.Auth.InvalidCredentials);
        }

        var loginResult = user.RecordLoginSuccess();
        if (loginResult.IsFailure)
            return Result.Failure<LoginResponse>(loginResult.Error);

        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        var refreshExpiryDays = int.Parse(configuration["Jwt:RefreshExpiryDays"] ?? "7");
        var accessExpiryMinutes = int.Parse(configuration["Jwt:AccessExpiryMinutes"] ?? "60");

        user.AddRefreshToken(refreshToken, DateTimeOffset.UtcNow.AddDays(refreshExpiryDays));
        await unitOfWork.CommitAsync(user.TenantId, user.Id, cancellationToken);

        var userDto = new UserDto(
            user.Id,
            user.Name,
            user.EmailAddress,
            user.Role.ToString(),
            user.TenantId,
            user.OrganizationId,
            user.BusinessUnitId,
            user.Theme,
            user.Language,
            user.IsDriverAccount,
            user.CpfLast4);

        // TODO: Get Fuel Alerts if user is Driver
        List<FuelAlertDto>? fuelAlerts = null;

        var response = new LoginResponse(
            accessToken,
            refreshToken,
            DateTimeOffset.UtcNow.AddMinutes(accessExpiryMinutes),
            userDto,
            fuelAlerts
        );

        return Result.Success(response);
    }

    private static bool IsCpf(string identifier)
    {
        var digitsOnly = new string(identifier.Where(char.IsDigit).ToArray());
        return digitsOnly.Length == 11;
    }

    private static string HashCpf(string cpf)
    {
        var digitsOnly = new string(cpf.Where(char.IsDigit).ToArray());
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(digitsOnly));
        return Convert.ToBase64String(hashBytes);
    }
}
