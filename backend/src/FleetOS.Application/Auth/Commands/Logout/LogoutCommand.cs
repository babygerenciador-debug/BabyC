using FleetOS.Shared.Results;
using MediatR;

namespace FleetOS.Application.Auth.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<Result>;
