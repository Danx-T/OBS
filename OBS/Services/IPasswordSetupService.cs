namespace OBS.Services;

public interface IPasswordSetupService
{
    /// <summary>Kullanıcı ID'si için yeni bir şifre kurma tokeni üretir.</summary>
    string GenerateToken(int kullaniciId);

    /// <summary>Tokenin hangi kullanıcıya ait olduğunu döndürür. Geçersizse null döner.</summary>
    int? GetKullaniciId(string token);

    /// <summary>Tokeni önbellekten siler.</summary>
    void InvalidateToken(string token);

    /// <summary>Kullanıcıya 'Şifrenizi Oluşturun' konulu hoş geldin e-postası gönderir.</summary>
    Task SendSetupEmailAsync(string eposta, string adSoyad, string setupLink);
}
