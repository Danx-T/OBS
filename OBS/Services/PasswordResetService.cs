using Microsoft.Extensions.Caching.Memory;

namespace OBS.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly IMemoryCache _cache;

    // Sıfırlama linkinin geçerlilik süresi: 15 dakika
    private static readonly TimeSpan TokenExpiry = TimeSpan.FromMinutes(15);

    public PasswordResetService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string GenerateToken(int kullaniciId)
    {
        var token = Guid.NewGuid().ToString("N"); // 32 karakterlik temiz GUID
        _cache.Set(GetCacheKey(token), kullaniciId, TokenExpiry);
        return token;
    }

    public int? GetKullaniciId(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        if (_cache.TryGetValue(GetCacheKey(token), out int kullaniciId))
            return kullaniciId;

        return null;
    }

    public void InvalidateToken(string token)
    {
        _cache.Remove(GetCacheKey(token));
    }

    private static string GetCacheKey(string token) => $"pwd_reset_{token}";
}
