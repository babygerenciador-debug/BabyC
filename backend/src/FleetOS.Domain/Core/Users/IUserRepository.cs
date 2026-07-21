using FleetOS.Domain.Common.Interfaces;

namespace FleetOS.Domain.Core.Users;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    
    
    // Looks up driver user by TenantId and CPF Hash
    Task<User?> GetByCpfHashAsync(Guid tenantId, string cpfHash, CancellationToken cancellationToken = default);

    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
