using FleetOS.Application.Auth.Commands.Login;
using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string Token) : IRequest<Result<LoginResponse>>;
