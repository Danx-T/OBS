using Microsoft.Extensions.Caching.Memory;

namespace OBS.Services;

public class PasswordSetupService : IPasswordSetupService
{
    private readonly IMemoryCache _cache;
    private readonly IEmailService _emailService;
    private readonly ILogger<PasswordSetupService> _logger;

    // Şifre oluşturma linkinin geçerlilik süresi: 48 saat
    private static readonly TimeSpan TokenExpiry = TimeSpan.FromHours(48);

    public PasswordSetupService(IMemoryCache cache, IEmailService emailService, ILogger<PasswordSetupService> logger)
    {
        _cache = cache;
        _emailService = emailService;
        _logger = logger;
    }

    public string GenerateToken(int kullaniciId)
    {
        var token = Guid.NewGuid().ToString("N");
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

    public async Task SendSetupEmailAsync(string eposta, string adSoyad, string setupLink)
    {
        var subject = "OBS - Hoş Geldiniz! Şifrenizi Oluşturun";
        var htmlBody = $@"
        <div style='font-family: Arial, sans-serif; max-width: 550px; margin: 0 auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px; background-color: #ffffff;'>
            <div style='text-align: center; margin-bottom: 24px;'>
                <h2 style='color: #1e293b; margin: 0;'>🎓 Üniversite Bilgi Sistemi</h2>
                <p style='color: #64748b; font-size: 14px;'>Hesap Kurulumu ve Şifre Belirleme</p>
            </div>
            <p style='color: #334155; font-size: 16px;'>Sayın <strong>{adSoyad}</strong>,</p>
            <p style='color: #475569; font-size: 15px; line-height: 1.5;'>
                Üniversite Bilgi Sistemi'nde hesabınız başarıyla oluşturulmuştur. Sisteme giriş yapabilmek için aşağıdaki butona tıklayarak şifrenizi belirlemeniz gerekmektedir:
            </p>
            <div style='text-align: center; margin: 32px 0;'>
                <a href='{setupLink}' style='background: linear-gradient(90deg, #2563eb, #1d4ed8); color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 8px; font-weight: bold; display: inline-block;'>
                    🔑 Şifremi Oluştur
                </a>
            </div>
            <p style='color: #64748b; font-size: 13px;'>
                Eğer buton çalışmazsa, aşağıdaki linki kopyalayıp tarayıcınızın adres çubuğuna yapıştırabilirsiniz:<br/>
                <a href='{setupLink}' style='color: #2563eb;'>{setupLink}</a>
            </p>
            <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 24px 0;'/>
            <p style='color: #94a3b8; font-size: 12px; text-align: center;'>
                Bu bağlantı 48 saat boyunca geçerlidir. Güvenliğiniz için lütfen şifrenizi kimseyle paylaşmayınız.
            </p>
        </div>";

        try
        {
            await _emailService.SendAsync(eposta, adSoyad, subject, htmlBody);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kurulum e-postası gönderilemedi (SMTP ayarlanmamış olabilir). Link: {Link}", setupLink);
        }
    }

    private static string GetCacheKey(string token) => $"pwd_setup_{token}";
}
