using FleetOS.Application.Auth.Commands.Login;
using FleetOS.Application.Common.Interfaces;
using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Users;
using FleetOS.Shared.Results;
using MediatR;
using Microsoft.Extensions.Configuration;


namespace FleetOS.Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtService jwtService,
    IUnitOfWork unitOfWork,
    IConfiguration configuration)
    : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Find user by refresh token (need a way to query by refresh token, or validate it directly)
        var user = await userRepository.GetByRefreshTokenAsync(request.Token, cancellationToken);

        if (user == null)
            return Result.Failure<LoginResponse>(Error.Auth.InvalidCredentials);

        if (user.IsLockedOut() || user.Status == UserStatus.Disabled)
            return Result.Failure<LoginResponse>(Error.Auth.UserBlocked);

        var existingToken = user.RefreshTokens.SingleOrDefault(r => r.Token == request.Token);
        if (existingToken == null || existingToken.IsExpired || existingToken.IsRevoked)
            return Result.Failure<LoginResponse>(Error.Auth.InvalidCredentials);

        // 2. Generate new tokens
        var newAccessToken = jwtService.GenerateAccessToken(user);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        // 3. Revoke old and add new
        existingToken.Revoke(newRefreshToken);

        var refreshExpiryDays = int.Parse(configuration["Jwt:RefreshExpiryDays"] ?? "7");
        var accessExpiryMinutes = int.Parse(configuration["Jwt:AccessExpiryMinutes"] ?? "60");

        user.AddRefreshToken(newRefreshToken, DateTimeOffset.UtcNow.AddDays(refreshExpiryDays));
        await unitOfWork.CommitAsync(user.TenantId, cancellationToken);

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

        var response = new LoginResponse(
            newAccessToken,
            newRefreshToken,
            DateTimeOffset.UtcNow.AddMinutes(accessExpiryMinutes),
            userDto,
            null
        );

        return Result.Success(response);
    }
}
