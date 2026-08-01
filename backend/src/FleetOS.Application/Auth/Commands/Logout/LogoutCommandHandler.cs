using FleetOS.Domain.Common.Interfaces;
using FleetOS.Domain.Core.Users;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Auth.Commands.Logout;

public sealed class LogoutCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(tenantContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure(Error.Auth.InvalidCredentials);

        user.RevokeRefreshToken(request.RefreshToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
