<div align="center">

# 🎓 OBS — Öğrenci Bilgi Sistemi

**Bir üniversitenin tüm akademik yaşamını tek çatı altında toplayan, rol tabanlı ve denetlenebilir web uygulaması.**

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET_Core-MVC-5C2D91?style=for-the-badge&logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-10.0-68217A?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Bootstrap](https://img.shields.io/badge/Bootstrap-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)

[Özellikler](#-özellikler) •
[Teknolojiler](#-teknolojiler) •
[Hızlı Başlangıç](#-hızlı-başlangıç) •
[Mimari](#-mimari) •
[Veritabanı](#-veritabanı) •
[Güvenlik](#-güvenlik)

</div>

---

## 📖 Hakkında

**OBS**, bir üniversitede yaşanan akademik süreçlerin uçtan uca yönetildiği bir Öğrenci Bilgi Sistemidir.
Fakülte ve bölüm yapısının kurulmasından, ders açılmasına; ders kaydından danışman onayına;
haftalık ders programından sınav takvimine ve not girişine kadar her adım sistemde karşılık bulur.

Üç farklı kullanıcı deneyimi sunar:

| Kullanıcı | Ne yapar? |
|---|---|
| 🛡️ **Yönetici** | Kullanıcıları, organizasyonu, dersleri, dönemleri, salonları ve programları yönetir; tüm işlemleri denetim kayıtlarından izler. |
| 👨‍🏫 **Öğretim Üyesi** | Verdiği dersleri ve öğrencileri görür, not girer, danışmanlık onaylarını verir, kendi ders ve sınav programını takip eder. |
| 🎓 **Öğrenci** | Ders kaydı yapar, haftalık ders ve sınav programını görür, notlarını inceler. |

---

## ✨ Özellikler

### 🛡️ Yönetim Paneli
- **Kullanıcı yönetimi:** Öğrenci, öğretim üyesi veya personel olarak kullanıcı oluşturma, düzenleme, filtreleme
- **Rol ve yetki sistemi:** Rol tanımlama, rollere yetki bağlama, kullanıcıya doğrudan yetki verme
- **Organizasyon yapısı:** Fakülte → Bölüm hiyerarşisi
- **Ders yönetimi:** Ders kataloğu, kredi/AKTS, teorik-uygulama saatleri, **ön koşullu ders** tanımlama
- **Açılan ders yönetimi:** Dönem bazında şube açma, kontenjan ve durum (aktif/pasif) kontrolü
- **Ders programı:** Gün ve saat bazlı haftalık program (aynı salonda aynı gün ve başlangıç saatine iki ders konamaz)
- **Sınav programı:** Sınav tipi, salon ve zaman planlaması
- **Bina ve salon yönetimi:** Bina → salon/sınıf ilişkisi ve kapasite takibi
- **Dönem yönetimi:** Akademik yıl, Güz/Bahar dönemleri, ders kayıt tarih aralığı
- **Danışman atama:** Öğrencilere öğretim üyesi danışman bağlama
- **Denetim kayıtları (Audit Log):** Kim, ne zaman, hangi kayıtta neyi değiştirdi

### 👨‍🏫 Öğretim Üyesi Paneli
- Verilen dersler ve derse kayıtlı öğrenciler
- Ders bazlı not girişi
- Danışman onay ekranı (öğrencilerin ders kayıtlarını onaylama)
- Kişisel haftalık ders programı ve sınav takvimi

### 🎓 Öğrenci Paneli
- Kontrollü **ders kaydı** (aktif kayıt dönemi, kontenjan ve ders saati çakışması denetimi, danışman onayına sunma)
- Haftalık ders programı
- Sınav programı
- Not görüntüleme

### 🔐 Kimlik Doğrulama
- E-posta + şifre ile giriş
- İsteğe bağlı **iki faktörlü doğrulama** (e-posta ile tek kullanımlık kod)
- Yönetici kullanıcı oluşturduğunda kullanıcıya **şifre belirleme bağlantısı** gönderilir
- **Şifremi unuttum** akışı
- Şifreler **BCrypt** ile hashlenir, düz metin hiçbir yerde tutulmaz

---

## 🧰 Teknolojiler

| Katman | Teknoloji |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| Veri erişimi | Entity Framework Core 10 |
| Veritabanı | Microsoft SQL Server |
| Kimlik doğrulama | Cookie Authentication, rol tabanlı yetkilendirme |
| Parola güvenliği | BCrypt.Net-Next |
| E-posta | MailKit (SMTP) |
| Arayüz | Razor Views, Bootstrap |

---

## 🚀 Hızlı Başlangıç

### Gereksinimler
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Microsoft SQL Server (LocalDB, Express veya Developer sürümü)
- SSMS veya Azure Data Studio (veritabanı scriptini çalıştırmak için)

### 1️⃣ Projeyi indirin

```bash
git clone https://github.com/Danx-T/OBS.git
cd OBS
```

### 2️⃣ Veritabanını kurun

[`Database/obs_schema_and_seed.sql`](Database/obs_schema_and_seed.sql) dosyasını SSMS veya Azure Data Studio ile açıp **tamamını çalıştırın**.
Komut satırından kurmak isterseniz:

```bash
sqlcmd -S localhost -E -i Database/obs_schema_and_seed.sql
```

Script `OBS` veritabanını oluşturur, tüm tabloları kurar ve başlangıç verisini ekler.
Veritabanı zaten kuruluysa hiçbir şey yapmadan çıkar, mevcut verinize dokunmaz.

### 3️⃣ Bağlantı ayarını kontrol edin

`OBS/appsettings.json` içindeki bağlantı dizesi varsayılan olarak yerel SQL Server'ı hedefler:

```json
"ConnectionStrings": {
  "OBSConnection": "Server=localhost;Database=OBS;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Sunucunuz farklıysa (ör. `localhost\SQLEXPRESS` veya `(localdb)\MSSQLLocalDB`) `Server=` kısmını değiştirin.

### 4️⃣ Uygulamayı çalıştırın

```bash
cd OBS
dotnet run
```

`dotnet run` varsayılan olarak `http` profilini kullanır ve tarayıcıyı otomatik açar. Elle açmak isterseniz adres:

**http://localhost:5028**

> HTTPS ile çalıştırmak isterseniz `dotnet run --launch-profile https` komutunu kullanın, adres `https://localhost:7153` olur.

### 5️⃣ Giriş yapın

| | |
|---|---|
| **E-posta** | `admin@obs.local` |
| **Şifre** | `Admin123!` |

> ⚠️ Bu hesap yalnızca geliştirme içindir. **İlk girişten sonra şifreyi değiştirin.**

---

## ✉️ E-posta (SMTP) Ayarları

İki faktörlü doğrulama, şifre sıfırlama ve yeni kullanıcı şifre kurulumu e-posta gönderir.
Bu özellikleri kullanacaksanız SMTP bilgilerinizi girmeniz gerekir. `appsettings.json` içindeki
`SmtpSettings` alanları bilerek boş bırakılmıştır.

Gizli bilgileri depoya göndermemek için ayarları **`OBS/appsettings.Development.json`**
dosyasına yazın. Bu dosya `.gitignore` ile Git'in dışında tutulur:

```json
{
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "ornek@gmail.com",
    "Password": "uygulama-sifresi",
    "FromName": "OBS Sistemi"
  }
}
```

> 💡 Gmail kullanıyorsanız normal şifreniz yerine Google hesabınızdan oluşturacağınız bir **uygulama şifresi** gerekir.

---

## 🏗️ Mimari

```
OBS/
├── Database/
│   └── obs_schema_and_seed.sql      # Şema + başlangıç verisi
├── OBS/
│   ├── Controllers/
│   │   ├── AuthController.cs        # Giriş, 2FA, şifre belirleme/sıfırlama
│   │   ├── AdminController.cs       # Yönetim paneli
│   │   ├── AcademicianController.cs # Öğretim üyesi paneli
│   │   ├── StudentController.cs     # Öğrenci paneli
│   │   └── HomeController.cs        # Rol bazlı yönlendirme
│   ├── Models/                      # EF Core varlıkları ve ObsContext
│   ├── ViewModels/                  # Ekranlara özel modeller
│   ├── Services/                    # E-posta, 2FA, şifre kurulum/sıfırlama
│   ├── Views/                       # Razor görünümleri
│   └── wwwroot/                     # Statik dosyalar
└── OBS.slnx
```

**Öne çıkan tasarım kararları**

- 🧩 **Servis katmanı:** E-posta, iki faktörlü doğrulama ve şifre akışları arayüzleri (`IEmailService`, `ITwoFactorService`, ...) ile ayrılmıştır, bağımlılık enjeksiyonu ile kullanılır.
- 🧾 **Otomatik denetim kaydı:** `ObsContext`, `SaveChanges` sırasında yapılan her ekleme, güncelleme ve silmeyi yakalayıp `DenetimKaydi` tablosuna yazar. Kullanıcı ve IP bilgisi de kaydedilir. Şifre hash'leri kayda **dahil edilmez**.
- 🔗 **İlişkisel bütünlük:** Veritabanı tarafında FK, unique ve filtreli index'lerle tutarlılık garanti edilir (örneğin aynı salonda aynı gün ve saate iki ders konamaz, bir öğrenci aynı şubeye iki kez kayıt olamaz).
- 🔑 **Rol bazlı erişim:** Yönetim paneli yalnızca `Admin` rolüne açıktır. Giriş sonrası kullanıcı rolüne ve tipine göre ilgili panele yönlendirilir.

---

## 🗄️ Veritabanı

Şema 19 tablodan oluşur:

| Alan | Tablolar |
|---|---|
| Kimlik ve yetki | `Kullanici`, `Rol`, `KullaniciRol`, `Yetki`, `RolYetki`, `KullaniciYetki` |
| Kişiler | `Ogrenci`, `OgretimUyesi` |
| Organizasyon | `Organizasyon` (fakülte/bölüm hiyerarşisi) |
| Akademik | `Donem`, `Ders`, `DersOnKosul`, `AcilanDers`, `DersKaydi`, `Notlar` |
| Planlama | `Salon` (bina/salon), `DersProgrami`, `SinavProgrami` |
| İzleme | `DenetimKaydi` |

Başlangıç verisi şunları içerir: `Admin`, `Ogrenci` ve `OgretimUyesi` rolleri, örnek yetkiler,
yönetici hesabı, bir fakülte ve bölüm, 2025-2026 Güz/Bahar dönemleri ve ön koşullu üç örnek ders.

---

## 🔒 Güvenlik

- Şifreler BCrypt ile hashlenir
- Anti-forgery token ile CSRF koruması
- İsteğe bağlı e-posta ile iki faktörlü doğrulama
- Oturum çerezi 8 saat geçerlidir (kayan süre)
- Yönetim paneli `[Authorize(Roles = "Admin")]` ile korunur
- Tüm veri değişiklikleri denetim kaydına alınır
- Gizli ayarlar (`appsettings.Development.json`) Git'e dahil edilmez
