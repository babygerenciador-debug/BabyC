using FleetOS.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Supabase.Storage;
using Supabase.Storage.Interfaces;
using FileOptions = Supabase.Storage.FileOptions;

namespace FleetOS.Infrastructure.Services;

public sealed class SupabaseStorageService : IStorageService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly string _defaultBucket;

    public SupabaseStorageService(IConfiguration configuration)
    {
        var url = configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase:Url is required");
        var key = configuration["Supabase:ServiceKey"] ?? throw new InvalidOperationException("Supabase:ServiceKey is required");
        _defaultBucket = configuration["Supabase:StorageBucket"] ?? "fleetos";

        _supabaseClient = new Supabase.Client(url, key);
        _supabaseClient.InitializeAsync().GetAwaiter().GetResult();
    }

    public async Task<string> UploadFileAsync(string bucketName, string fileName, Stream fileStream, string contentType)
    {
        var bucket = _supabaseClient.Storage.From(bucketName);
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var result = await bucket.Upload(memoryStream.ToArray(), fileName, new FileOptions { ContentType = contentType });
        return result;
    }

    public async Task DeleteFileAsync(string bucketName, string fileName)
    {
        var bucket = _supabaseClient.Storage.From(bucketName);
        await bucket.Remove([fileName]);
    }

    public string GetPublicUrl(string bucketName, string fileName)
    {
        var bucket = _supabaseClient.Storage.From(bucketName);
        return bucket.GetPublicUrl(fileName);
    }
}
