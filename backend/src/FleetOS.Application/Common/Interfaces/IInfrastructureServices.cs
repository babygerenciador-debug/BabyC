using FleetOS.Domain.Core.Users;

namespace FleetOS.Application.Common.Interfaces;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Guid? ValidateToken(string token);
}

public interface IStorageService
{
    Task<string> UploadFileAsync(string bucketName, string fileName, Stream fileStream, string contentType);
    Task DeleteFileAsync(string bucketName, string fileName);
    string GetPublicUrl(string bucketName, string fileName);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
