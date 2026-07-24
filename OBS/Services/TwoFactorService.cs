using Microsoft.Extensions.Caching.Memory;

namespace OBS.Services;

public class TwoFactorService : ITwoFactorService
{
    private readonly IMemoryCache _cache;

    // Kodun geçerlilik süresi: 5 dakika
    private static readonly TimeSpan CodeExpiry = TimeSpan.FromMinutes(5);

    // Kullanılabilecek karakterler: büyük harf + rakam
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public TwoFactorService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public string GenerateCode(int kullaniciId)
    {
        var rng  = new Random();
        var code = new string(Enumerable.Range(0, 6).Select(_ => Chars[rng.Next(Chars.Length)]).ToArray());

        var cacheKey = GetCacheKey(kullaniciId);
        _cache.Set(cacheKey, code, CodeExpiry);

        return code;
    }

    public bool ValidateCode(int kullaniciId, string code)
    {
        var cacheKey = GetCacheKey(kullaniciId);

        if (!_cache.TryGetValue(cacheKey, out string? storedCode))
            return false;

        if (!string.Equals(storedCode, code?.Trim().ToUpperInvariant(), StringComparison.Ordinal))
            return false;

        // Tek kullanımlık: başarılı doğrulamada kodu sil
        _cache.Remove(cacheKey);
        return true;
    }

    private static string GetCacheKey(int kullaniciId) => $"2fa_code_{kullaniciId}";
}
