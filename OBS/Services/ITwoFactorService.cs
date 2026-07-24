namespace OBS.Services;

public interface ITwoFactorService
{
    /// <summary>Yeni bir 6 haneli kod üretir, önbelleğe kaydeder ve döndürür.</summary>
    string GenerateCode(int kullaniciId);

    /// <summary>Girilen kodun geçerli olup olmadığını kontrol eder. Geçerliyse önbellekten siler.</summary>
    bool ValidateCode(int kullaniciId, string code);
}
