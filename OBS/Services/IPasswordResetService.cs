namespace OBS.Services;

public interface IPasswordResetService
{
    /// <summary>Kullanıcı ID'si için bir sıfırlama tokeni üretir ve önbelleğe kaydeder.</summary>
    string GenerateToken(int kullaniciId);

    /// <summary>Token'ın hangi kullanıcıya ait olduğunu döndürür. Geçersizse null döner.</summary>
    int? GetKullaniciId(string token);

    /// <summary>Tokeni önbellekten siler (kullanıldıktan sonra).</summary>
    void InvalidateToken(string token);
}
