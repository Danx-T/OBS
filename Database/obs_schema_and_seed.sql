/* =====================================================================
   OBS - Öğrenci Bilgi Sistemi
   Veritabanı şeması ve başlangıç (seed) verisi

   Hedef   : Microsoft SQL Server (LocalDB / Express / Developer)
   Kullanım: SSMS veya Azure Data Studio'da bu dosyayı açıp tamamını çalıştırın.
             sqlcmd ile: sqlcmd -S localhost -E -i obs_schema_and_seed.sql
   Not     : Script boş bir veritabanı için tasarlanmıştır. "Kullanici" tablosu
             zaten varsa hiçbir şey çalıştırmadan çıkar (mevcut veri korunur).
   Şema, OBS/Models/ObsContext.cs içindeki EF Core modelinden üretilmiştir.
   ===================================================================== */

IF DB_ID(N'OBS') IS NULL
    CREATE DATABASE [OBS] COLLATE Turkish_CI_AS;
GO

USE [OBS];
GO

IF OBJECT_ID(N'dbo.Kullanici', N'U') IS NOT NULL
BEGIN
    PRINT N'OBS şeması zaten mevcut. Script çalıştırılmadı.';
    SET NOEXEC ON;
END
GO

/* ===================== ŞEMA ===================== */

CREATE TABLE [Donem] (
    [Id] int NOT NULL IDENTITY,
    [AkademikYil] varchar(9) NOT NULL,
    [Donem] varchar(10) NOT NULL,
    [BaslangicTarihi] date NOT NULL,
    [BitisTarihi] date NOT NULL,
    [DersKaydiBaslangic] datetime NOT NULL,
    [DersKaydiBitis] datetime NOT NULL,
    CONSTRAINT [PK_Donem] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Kullanici] (
    [Id] int NOT NULL IDENTITY,
    [Ad] varchar(100) NOT NULL,
    [Soyad] varchar(100) NOT NULL,
    [Eposta] varchar(50) NOT NULL,
    [Telefon] varchar(20) NOT NULL,
    [SifreHash] varchar(255) NULL,
    [IkiFaktorluDogrulama] bit NOT NULL,
    [AktiflikDurumu] bit NOT NULL DEFAULT CAST(0 AS bit),
    [OlusturmaTarihi] datetime NOT NULL DEFAULT ((getdate())),
    [SonGuncellenmeTarihi] datetime NULL,
    CONSTRAINT [PK_Kullanici] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Organizasyon] (
    [Id] int NOT NULL IDENTITY,
    [UstOrganizasyonId] int NULL,
    [Tipi] varchar(10) NOT NULL,
    [Adi] varchar(100) NOT NULL,
    [Kodu] varchar(10) NULL,
    [Durum] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_Organizasyon] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Organizasyon_Ust] FOREIGN KEY ([UstOrganizasyonId]) REFERENCES [Organizasyon] ([Id])
);
GO


CREATE TABLE [Rol] (
    [Id] int NOT NULL IDENTITY,
    [RolAdi] varchar(50) NOT NULL,
    [Aciklama] varchar(255) NULL,
    CONSTRAINT [PK_Rol] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Salon] (
    [Id] int NOT NULL IDENTITY,
    [BinaId] int NULL,
    [SalonTipi] varchar(20) NOT NULL,
    [SalonAdi] varchar(20) NOT NULL,
    [Kapasite] smallint NULL,
    CONSTRAINT [PK_Salon] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Salon_Bina] FOREIGN KEY ([BinaId]) REFERENCES [Salon] ([Id])
);
GO


CREATE TABLE [Yetki] (
    [Id] int NOT NULL IDENTITY,
    [YetkiKodu] varchar(30) NOT NULL,
    [Aciklama] varchar(255) NULL,
    CONSTRAINT [PK_Yetki] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [DenetimKaydi] (
    [Id] int NOT NULL IDENTITY,
    [KullaniciId] int NULL,
    [IslemTuru] varchar(10) NOT NULL,
    [EtkilenenTablo] varchar(50) NOT NULL,
    [EtkilenenKayitId] int NOT NULL,
    [EtkilenenSutun] varchar(50) NOT NULL,
    [EskiDeger] varchar(max) NULL,
    [YeniDeger] varchar(max) NULL,
    [IslemZamani] datetime NOT NULL DEFAULT ((getdate())),
    [IpAdresi] varchar(45) NULL,
    CONSTRAINT [PK_DenetimKaydi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DenetimKaydi_Kullanici] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanici] ([Id]) ON DELETE SET NULL
);
GO


CREATE TABLE [Ders] (
    [Id] int NOT NULL IDENTITY,
    [OrganizasyonId] int NOT NULL,
    [DersKodu] varchar(10) NOT NULL,
    [DersAdi] varchar(100) NOT NULL,
    [Kredi] decimal(3,1) NOT NULL,
    [Akts] decimal(4,1) NOT NULL,
    [Teorik] int NOT NULL,
    [Uygulama] int NOT NULL,
    [DersTuru] varchar(10) NOT NULL,
    [AktiflikDurumu] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_Ders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Ders_Organizasyon] FOREIGN KEY ([OrganizasyonId]) REFERENCES [Organizasyon] ([Id])
);
GO


CREATE TABLE [OgretimUyesi] (
    [Id] int NOT NULL IDENTITY,
    [KullaniciId] int NOT NULL,
    [Cinsiyet] varchar(10) NOT NULL,
    [Unvan] varchar(20) NULL,
    [OrganizasyonId] int NOT NULL,
    [KadroTipi] varchar(20) NULL,
    [GorevBaslangic] date NOT NULL,
    [GorevBitis] date NULL,
    CONSTRAINT [PK_OgretimUyesi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OgretimUyesi_Kullanici] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanici] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OgretimUyesi_Organizasyon] FOREIGN KEY ([OrganizasyonId]) REFERENCES [Organizasyon] ([Id])
);
GO


CREATE TABLE [KullaniciRol] (
    [KullaniciId] int NOT NULL,
    [RolId] int NOT NULL,
    [AktiflikDurumu] bit NOT NULL DEFAULT CAST(1 AS bit),
    [BaslangicTarihi] datetime NOT NULL,
    [BitisTarihi] datetime NULL,
    CONSTRAINT [PK_KullaniciRol] PRIMARY KEY ([KullaniciId], [RolId]),
    CONSTRAINT [FK_KullaniciRol_Kullanici] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanici] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_KullaniciRol_Rol] FOREIGN KEY ([RolId]) REFERENCES [Rol] ([Id])
);
GO


CREATE TABLE [KullaniciYetki] (
    [KullaniciId] int NOT NULL,
    [YetkiId] int NOT NULL,
    [BaslangicTarihi] datetime NOT NULL,
    [IslemYapanKullaniciId] int NULL,
    [BitisTarihi] datetime NULL,
    CONSTRAINT [PK_KullaniciYetki] PRIMARY KEY ([KullaniciId], [YetkiId], [BaslangicTarihi]),
    CONSTRAINT [FK_KullaniciYetki_IslemYapan] FOREIGN KEY ([IslemYapanKullaniciId]) REFERENCES [Kullanici] ([Id]),
    CONSTRAINT [FK_KullaniciYetki_Kullanici] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanici] ([Id]),
    CONSTRAINT [FK_KullaniciYetki_Yetki] FOREIGN KEY ([YetkiId]) REFERENCES [Yetki] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [RolYetki] (
    [RolId] int NOT NULL,
    [YetkiId] int NOT NULL,
    [BaslangicTarihi] datetime NOT NULL,
    [BitisTarihi] datetime NULL,
    CONSTRAINT [PK_RolYetki] PRIMARY KEY ([RolId], [YetkiId]),
    CONSTRAINT [FK_RolYetki_Rol] FOREIGN KEY ([RolId]) REFERENCES [Rol] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolYetki_Yetki] FOREIGN KEY ([YetkiId]) REFERENCES [Yetki] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [DersOnKosul] (
    [DersId] int NOT NULL,
    [OnKosulDersId] int NOT NULL,
    CONSTRAINT [PK_DersOnKosul] PRIMARY KEY ([DersId], [OnKosulDersId]),
    CONSTRAINT [FK_DersOnKosul_Ders] FOREIGN KEY ([DersId]) REFERENCES [Ders] ([Id]),
    CONSTRAINT [FK_DersOnKosul_OnKosul] FOREIGN KEY ([OnKosulDersId]) REFERENCES [Ders] ([Id])
);
GO


CREATE TABLE [AcilanDers] (
    [Id] int NOT NULL IDENTITY,
    [DersId] int NOT NULL,
    [OgretimUyesiId] int NOT NULL,
    [DonemId] int NOT NULL,
    [SubeNo] varchar(5) NOT NULL,
    [Kontenjan] int NOT NULL,
    [Durum] varchar(20) NULL,
    CONSTRAINT [PK_AcilanDers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AcilanDers_Ders] FOREIGN KEY ([DersId]) REFERENCES [Ders] ([Id]),
    CONSTRAINT [FK_AcilanDers_Donem] FOREIGN KEY ([DonemId]) REFERENCES [Donem] ([Id]),
    CONSTRAINT [FK_AcilanDers_OgretimUyesi] FOREIGN KEY ([OgretimUyesiId]) REFERENCES [OgretimUyesi] ([Id])
);
GO


CREATE TABLE [Ogrenci] (
    [Id] int NOT NULL IDENTITY,
    [KullaniciId] int NOT NULL,
    [Cinsiyet] varchar(10) NOT NULL,
    [OgrenciNo] varchar(15) NOT NULL,
    [DanismanId] int NULL,
    [OrganizasyonId] int NOT NULL,
    [GirisTarihi] date NOT NULL,
    [OgrenciTipi] varchar(20) NOT NULL,
    [Durum] varchar(20) NOT NULL,
    [MezuniyetTarihi] date NULL,
    [Sinif] int NOT NULL,
    CONSTRAINT [PK_Ogrenci] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Ogrenci_Danisman] FOREIGN KEY ([DanismanId]) REFERENCES [OgretimUyesi] ([Id]),
    CONSTRAINT [FK_Ogrenci_Kullanici] FOREIGN KEY ([KullaniciId]) REFERENCES [Kullanici] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Ogrenci_Organizasyon] FOREIGN KEY ([OrganizasyonId]) REFERENCES [Organizasyon] ([Id])
);
GO


CREATE TABLE [DersProgrami] (
    [Id] int NOT NULL IDENTITY,
    [AcilanDersId] int NOT NULL,
    [SalonId] int NOT NULL,
    [Gun] varchar(10) NOT NULL,
    [BaslangicSaati] time NOT NULL,
    [BitisSaati] time NOT NULL,
    CONSTRAINT [PK_DersProgrami] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DersProgrami_AcilanDers] FOREIGN KEY ([AcilanDersId]) REFERENCES [AcilanDers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_DersProgrami_Salon] FOREIGN KEY ([SalonId]) REFERENCES [Salon] ([Id])
);
GO


CREATE TABLE [SinavProgrami] (
    [Id] int NOT NULL IDENTITY,
    [AcilanDersId] int NOT NULL,
    [SalonId] int NOT NULL,
    [SinavTipi] varchar(15) NOT NULL,
    [Baslangic] datetime NOT NULL,
    [Bitis] datetime NOT NULL,
    CONSTRAINT [PK_SinavProgrami] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SinavProgrami_AcilanDers] FOREIGN KEY ([AcilanDersId]) REFERENCES [AcilanDers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SinavProgrami_Salon] FOREIGN KEY ([SalonId]) REFERENCES [Salon] ([Id])
);
GO


CREATE TABLE [DersKaydi] (
    [Id] int NOT NULL IDENTITY,
    [OgrenciId] int NOT NULL,
    [AcilanDersId] int NOT NULL,
    [KayitDurumu] varchar(20) NOT NULL,
    [KayitTarihi] datetime NOT NULL,
    [OnayTarihi] datetime NULL,
    CONSTRAINT [PK_DersKaydi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DersKaydi_AcilanDers] FOREIGN KEY ([AcilanDersId]) REFERENCES [AcilanDers] ([Id]),
    CONSTRAINT [FK_DersKaydi_Ogrenci] FOREIGN KEY ([OgrenciId]) REFERENCES [Ogrenci] ([Id])
);
GO


CREATE TABLE [Notlar] (
    [Id] int NOT NULL IDENTITY,
    [DersKaydiId] int NOT NULL,
    [OlcmeTuru] varchar(10) NOT NULL,
    [Puan] decimal(5,2) NOT NULL,
    CONSTRAINT [PK_Notlar] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notlar_DersKaydi] FOREIGN KEY ([DersKaydiId]) REFERENCES [DersKaydi] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_AcilanDers_DonemId] ON [AcilanDers] ([DonemId]);
GO


CREATE INDEX [IX_AcilanDers_OgretimUyesiId] ON [AcilanDers] ([OgretimUyesiId]);
GO


CREATE UNIQUE INDEX [UQ_AcilanDers_Ders_Donem_Sube] ON [AcilanDers] ([DersId], [DonemId], [SubeNo]);
GO


CREATE INDEX [IX_DenetimKaydi_KullaniciId] ON [DenetimKaydi] ([KullaniciId]);
GO


CREATE INDEX [IX_Ders_OrganizasyonId] ON [Ders] ([OrganizasyonId]);
GO


CREATE UNIQUE INDEX [UQ_Ders_DersKodu] ON [Ders] ([DersKodu]);
GO


CREATE INDEX [IX_DersKaydi_AcilanDersId] ON [DersKaydi] ([AcilanDersId]);
GO


CREATE UNIQUE INDEX [UQ_DersKaydi_Ogrenci_AcilanDers] ON [DersKaydi] ([OgrenciId], [AcilanDersId]);
GO


CREATE INDEX [IX_DersOnKosul_OnKosulDersId] ON [DersOnKosul] ([OnKosulDersId]);
GO


CREATE INDEX [IX_DersProgrami_AcilanDersId] ON [DersProgrami] ([AcilanDersId]);
GO


CREATE UNIQUE INDEX [UQ_DersProgrami_Salon_Gun_Saat] ON [DersProgrami] ([SalonId], [Gun], [BaslangicSaati]);
GO


CREATE UNIQUE INDEX [UQ_Donem_Yil_Donem] ON [Donem] ([AkademikYil], [Donem]);
GO


CREATE UNIQUE INDEX [UQ_Kullanici_Eposta] ON [Kullanici] ([Eposta]);
GO


CREATE INDEX [IX_KullaniciRol_RolId] ON [KullaniciRol] ([RolId]);
GO


CREATE INDEX [IX_KullaniciYetki_IslemYapanKullaniciId] ON [KullaniciYetki] ([IslemYapanKullaniciId]);
GO


CREATE INDEX [IX_KullaniciYetki_YetkiId] ON [KullaniciYetki] ([YetkiId]);
GO


CREATE UNIQUE INDEX [UQ_Notlar_DersKaydi_OlcmeTuru] ON [Notlar] ([DersKaydiId], [OlcmeTuru]);
GO


CREATE INDEX [IX_Ogrenci_DanismanId] ON [Ogrenci] ([DanismanId]);
GO


CREATE INDEX [IX_Ogrenci_OrganizasyonId] ON [Ogrenci] ([OrganizasyonId]);
GO


CREATE UNIQUE INDEX [UQ_Ogrenci_Kullanici] ON [Ogrenci] ([KullaniciId]);
GO


CREATE UNIQUE INDEX [UQ_Ogrenci_OgrenciNo] ON [Ogrenci] ([OgrenciNo]);
GO


CREATE INDEX [IX_OgretimUyesi_OrganizasyonId] ON [OgretimUyesi] ([OrganizasyonId]);
GO


CREATE UNIQUE INDEX [UQ_OgretimUyesi_Kullanici] ON [OgretimUyesi] ([KullaniciId]);
GO


CREATE INDEX [IX_Organizasyon_UstOrganizasyonId] ON [Organizasyon] ([UstOrganizasyonId]);
GO


CREATE UNIQUE INDEX [UQ_Organizasyon_Kodu] ON [Organizasyon] ([Kodu]) WHERE [Kodu] IS NOT NULL;
GO


CREATE UNIQUE INDEX [UQ_Rol_RolAdi] ON [Rol] ([RolAdi]);
GO


CREATE INDEX [IX_RolYetki_YetkiId] ON [RolYetki] ([YetkiId]);
GO


CREATE UNIQUE INDEX [UQ_Salon_Bina_Adi] ON [Salon] ([BinaId], [SalonAdi]) WHERE [BinaId] IS NOT NULL;
GO


CREATE INDEX [IX_SinavProgrami_AcilanDersId] ON [SinavProgrami] ([AcilanDersId]);
GO


CREATE UNIQUE INDEX [UQ_SinavProgrami_Salon_Baslangic] ON [SinavProgrami] ([SalonId], [Baslangic]);
GO


CREATE UNIQUE INDEX [UQ_Yetki_YetkiKodu] ON [Yetki] ([YetkiKodu]);
GO



/* ===================== BAŞLANGIÇ VERİSİ ===================== */

-- Roller
INSERT INTO [Rol] ([RolAdi], [Aciklama]) VALUES
    (N'Admin',        N'Sistem yöneticisi'),
    (N'Ogrenci',      N'Öğrenci'),
    (N'OgretimUyesi', N'Öğretim üyesi');
GO

-- Yetkiler
INSERT INTO [Yetki] ([YetkiKodu], [Aciklama]) VALUES
    (N'KULLANICI_YONETIMI', N'Kullanıcı oluşturma, düzenleme ve silme'),
    (N'ROL_YONETIMI',       N'Rol ve yetki atamalarını yönetme'),
    (N'DERS_YONETIMI',      N'Ders ve açılan ders yönetimi'),
    (N'PROGRAM_YONETIMI',   N'Ders ve sınav programı yönetimi'),
    (N'NOT_GIRISI',         N'Öğrenci notu girişi'),
    (N'DERS_KAYDI',         N'Ders kaydı yapma ve onaylama');
GO

-- Admin rolüne tüm yetkiler
INSERT INTO [RolYetki] ([RolId], [YetkiId], [BaslangicTarihi])
SELECT r.[Id], y.[Id], GETDATE()
FROM [Rol] r CROSS JOIN [Yetki] y
WHERE r.[RolAdi] = N'Admin';
GO

-- Yönetici hesabı
-- E-posta: admin@obs.local   Şifre: Admin123!   (İLK GİRİŞTEN SONRA DEĞİŞTİRİN)
-- İki faktörlü doğrulama kapalıdır; SMTP ayarı yapmadan da giriş yapılabilir.
INSERT INTO [Kullanici] ([Ad], [Soyad], [Eposta], [Telefon], [SifreHash], [IkiFaktorluDogrulama], [AktiflikDurumu], [OlusturmaTarihi])
VALUES (N'Sistem', N'Yöneticisi', 'admin@obs.local', '0000000000',
        '$2a$11$l3krr4MS9stJXASQZMdx/.i3FQhUHnLp7GtCGxLR/wWFQVlOx3.NW', 0, 1, GETDATE());
GO

INSERT INTO [KullaniciRol] ([KullaniciId], [RolId], [AktiflikDurumu], [BaslangicTarihi])
SELECT k.[Id], r.[Id], 1, GETDATE()
FROM [Kullanici] k CROSS JOIN [Rol] r
WHERE k.[Eposta] = 'admin@obs.local' AND r.[RolAdi] = N'Admin';
GO

-- Organizasyon: fakülte ve bölüm
INSERT INTO [Organizasyon] ([UstOrganizasyonId], [Tipi], [Adi], [Kodu], [Durum])
VALUES (NULL, 'Fakulte', N'Mühendislik Fakültesi', 'MUH', 1);

INSERT INTO [Organizasyon] ([UstOrganizasyonId], [Tipi], [Adi], [Kodu], [Durum])
SELECT [Id], 'Bolum', N'Bilgisayar Mühendisliği', 'BLM', 1
FROM [Organizasyon] WHERE [Kodu] = 'MUH';
GO

-- Dönemler
INSERT INTO [Donem] ([AkademikYil], [Donem], [BaslangicTarihi], [BitisTarihi], [DersKaydiBaslangic], [DersKaydiBitis]) VALUES
    ('2025-2026', 'Guz',   '2025-09-15', '2026-01-30', '2025-09-08T09:00:00', '2025-09-19T17:00:00'),
    ('2025-2026', 'Bahar', '2026-02-16', '2026-06-12', '2026-02-09T09:00:00', '2026-02-20T17:00:00');
GO

-- Örnek dersler
INSERT INTO [Ders] ([OrganizasyonId], [DersKodu], [DersAdi], [Kredi], [Akts], [Teorik], [Uygulama], [DersTuru], [AktiflikDurumu])
SELECT o.[Id], d.[DersKodu], d.[DersAdi], d.[Kredi], d.[Akts], d.[Teorik], d.[Uygulama], 'Zorunlu', 1
FROM [Organizasyon] o
CROSS JOIN (VALUES
    ('BLM101', N'Programlamaya Giriş',            3.0, 5.0, 2, 2),
    ('BLM102', N'Nesne Yönelimli Programlama',     3.0, 5.0, 2, 2),
    ('BLM201', N'Veri Yapıları ve Algoritmalar',   4.0, 6.0, 3, 2)
) AS d([DersKodu], [DersAdi], [Kredi], [Akts], [Teorik], [Uygulama])
WHERE o.[Kodu] = 'BLM';
GO

-- Ön koşullar: BLM102 <- BLM101, BLM201 <- BLM102
INSERT INTO [DersOnKosul] ([DersId], [OnKosulDersId])
SELECT d.[Id], p.[Id]
FROM [Ders] d
JOIN [Ders] p ON (d.[DersKodu] = 'BLM102' AND p.[DersKodu] = 'BLM101')
              OR (d.[DersKodu] = 'BLM201' AND p.[DersKodu] = 'BLM102');
GO

SET NOEXEC OFF;
GO
PRINT N'OBS veritabanı kurulumu tamamlandı. Giriş: admin@obs.local / Admin123!';
GO
