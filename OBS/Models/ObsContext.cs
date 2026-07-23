using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OBS.Models;

public partial class ObsContext : DbContext
{
    public ObsContext()
    {
    }

    public ObsContext(DbContextOptions<ObsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcilanDer> AcilanDers { get; set; }

    public virtual DbSet<DenetimKaydi> DenetimKaydis { get; set; }

    public virtual DbSet<Der> Ders { get; set; }

    public virtual DbSet<DersKaydi> DersKaydis { get; set; }

    public virtual DbSet<DersProgrami> DersProgramis { get; set; }

    public virtual DbSet<Donem> Donems { get; set; }

    public virtual DbSet<Kullanici> Kullanicis { get; set; }

    public virtual DbSet<KullaniciRol> KullaniciRols { get; set; }

    public virtual DbSet<KullaniciYetki> KullaniciYetkis { get; set; }

    public virtual DbSet<Notlar> Notlars { get; set; }

    public virtual DbSet<Ogrenci> Ogrencis { get; set; }

    public virtual DbSet<OgretimUyesi> OgretimUyesis { get; set; }

    public virtual DbSet<Organizasyon> Organizasyons { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<RolYetki> RolYetkis { get; set; }

    public virtual DbSet<Salon> Salons { get; set; }

    public virtual DbSet<SinavProgrami> SinavProgramis { get; set; }

    public virtual DbSet<Yetki> Yetkis { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcilanDer>(entity =>
        {
            entity.HasIndex(e => new { e.DersId, e.DonemId, e.SubeNo }, "UQ_AcilanDers_Ders_Donem_Sube").IsUnique();

            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SubeNo)
                .HasMaxLength(5)
                .IsUnicode(false);

            entity.HasOne(d => d.Ders).WithMany(p => p.AcilanDers)
                .HasForeignKey(d => d.DersId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AcilanDers_Ders");

            entity.HasOne(d => d.Donem).WithMany(p => p.AcilanDers)
                .HasForeignKey(d => d.DonemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AcilanDers_Donem");

            entity.HasOne(d => d.OgretimUyesi).WithMany(p => p.AcilanDers)
                .HasForeignKey(d => d.OgretimUyesiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AcilanDers_OgretimUyesi");
        });

        modelBuilder.Entity<DenetimKaydi>(entity =>
        {
            entity.ToTable("DenetimKaydi");

            entity.Property(e => e.EskiDeger).IsUnicode(false);
            entity.Property(e => e.EtkilenenSutun)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EtkilenenTablo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IpAdresi)
                .HasMaxLength(45)
                .IsUnicode(false);
            entity.Property(e => e.IslemTuru)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.IslemZamani)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.YeniDeger).IsUnicode(false);

            entity.HasOne(d => d.Kullanici).WithMany(p => p.DenetimKaydis)
                .HasForeignKey(d => d.KullaniciId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_DenetimKaydi_Kullanici");
        });

        modelBuilder.Entity<Der>(entity =>
        {
            entity.HasIndex(e => e.DersKodu, "UQ_Ders_DersKodu").IsUnique();

            entity.Property(e => e.AktiflikDurumu).HasDefaultValue(true);
            entity.Property(e => e.Akts).HasColumnType("decimal(4, 1)");
            entity.Property(e => e.DersAdi)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DersKodu)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DersTuru)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Kredi).HasColumnType("decimal(3, 1)");

            entity.HasOne(d => d.Organizasyon).WithMany(p => p.Ders)
                .HasForeignKey(d => d.OrganizasyonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ders_Organizasyon");

            entity.HasMany(d => d.Ders).WithMany(p => p.OnKosulDers)
                .UsingEntity<Dictionary<string, object>>(
                    "DersOnKosul",
                    r => r.HasOne<Der>().WithMany()
                        .HasForeignKey("DersId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_DersOnKosul_Ders"),
                    l => l.HasOne<Der>().WithMany()
                        .HasForeignKey("OnKosulDersId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_DersOnKosul_OnKosul"),
                    j =>
                    {
                        j.HasKey("DersId", "OnKosulDersId");
                        j.ToTable("DersOnKosul");
                    });

            entity.HasMany(d => d.OnKosulDers).WithMany(p => p.Ders)
                .UsingEntity<Dictionary<string, object>>(
                    "DersOnKosul",
                    r => r.HasOne<Der>().WithMany()
                        .HasForeignKey("OnKosulDersId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_DersOnKosul_OnKosul"),
                    l => l.HasOne<Der>().WithMany()
                        .HasForeignKey("DersId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_DersOnKosul_Ders"),
                    j =>
                    {
                        j.HasKey("DersId", "OnKosulDersId");
                        j.ToTable("DersOnKosul");
                    });
        });

        modelBuilder.Entity<DersKaydi>(entity =>
        {
            entity.ToTable("DersKaydi");

            entity.HasIndex(e => new { e.OgrenciId, e.AcilanDersId }, "UQ_DersKaydi_Ogrenci_AcilanDers").IsUnique();

            entity.Property(e => e.KayitDurumu)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.KayitTarihi).HasColumnType("datetime");
            entity.Property(e => e.OnayTarihi).HasColumnType("datetime");

            entity.HasOne(d => d.AcilanDers).WithMany(p => p.DersKaydis)
                .HasForeignKey(d => d.AcilanDersId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DersKaydi_AcilanDers");

            entity.HasOne(d => d.Ogrenci).WithMany(p => p.DersKaydis)
                .HasForeignKey(d => d.OgrenciId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DersKaydi_Ogrenci");
        });

        modelBuilder.Entity<DersProgrami>(entity =>
        {
            entity.ToTable("DersProgrami");

            entity.HasIndex(e => new { e.SalonId, e.Gun, e.BaslangicSaati }, "UQ_DersProgrami_Salon_Gun_Saat").IsUnique();

            entity.Property(e => e.Gun)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.AcilanDers).WithMany(p => p.DersProgramis)
                .HasForeignKey(d => d.AcilanDersId)
                .HasConstraintName("FK_DersProgrami_AcilanDers");

            entity.HasOne(d => d.Salon).WithMany(p => p.DersProgramis)
                .HasForeignKey(d => d.SalonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DersProgrami_Salon");
        });

        modelBuilder.Entity<Donem>(entity =>
        {
            entity.ToTable("Donem");

            entity.HasIndex(e => new { e.AkademikYil, e.Donem1 }, "UQ_Donem_Yil_Donem").IsUnique();

            entity.Property(e => e.AkademikYil)
                .HasMaxLength(9)
                .IsUnicode(false);
            entity.Property(e => e.DersKaydiBaslangic).HasColumnType("datetime");
            entity.Property(e => e.DersKaydiBitis).HasColumnType("datetime");
            entity.Property(e => e.Donem1)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("Donem");
        });

        modelBuilder.Entity<Kullanici>(entity =>
        {
            entity.ToTable("Kullanici");

            entity.HasIndex(e => e.Eposta, "UQ_Kullanici_Eposta").IsUnique();

            entity.Property(e => e.Ad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.AktiflikDurumu).HasDefaultValue(true);
            entity.Property(e => e.Eposta)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OlusturmaTarihi)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SifreHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.SonGuncellenmeTarihi).HasColumnType("datetime");
            entity.Property(e => e.Soyad)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Telefon)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<KullaniciRol>(entity =>
        {
            entity.HasKey(e => new { e.KullaniciId, e.RolId });

            entity.ToTable("KullaniciRol");

            entity.Property(e => e.AktiflikDurumu).HasDefaultValue(true);
            entity.Property(e => e.BaslangicTarihi).HasColumnType("datetime");
            entity.Property(e => e.BitisTarihi).HasColumnType("datetime");

            entity.HasOne(d => d.Kullanici).WithMany(p => p.KullaniciRols)
                .HasForeignKey(d => d.KullaniciId)
                .HasConstraintName("FK_KullaniciRol_Kullanici");

            entity.HasOne(d => d.Rol).WithMany(p => p.KullaniciRols)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KullaniciRol_Rol");
        });

        modelBuilder.Entity<KullaniciYetki>(entity =>
        {
            entity.HasKey(e => new { e.KullaniciId, e.YetkiId, e.BaslangicTarihi });

            entity.ToTable("KullaniciYetki");

            entity.Property(e => e.BaslangicTarihi).HasColumnType("datetime");
            entity.Property(e => e.BitisTarihi).HasColumnType("datetime");

            entity.HasOne(d => d.IslemYapanKullanici).WithMany(p => p.KullaniciYetkiIslemYapanKullanicis)
                .HasForeignKey(d => d.IslemYapanKullaniciId)
                .HasConstraintName("FK_KullaniciYetki_IslemYapan");

            entity.HasOne(d => d.Kullanici).WithMany(p => p.KullaniciYetkiKullanicis)
                .HasForeignKey(d => d.KullaniciId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_KullaniciYetki_Kullanici");

            entity.HasOne(d => d.Yetki).WithMany(p => p.KullaniciYetkis)
                .HasForeignKey(d => d.YetkiId)
                .HasConstraintName("FK_KullaniciYetki_Yetki");
        });

        modelBuilder.Entity<Notlar>(entity =>
        {
            entity.ToTable("Notlar");

            entity.HasIndex(e => new { e.DersKaydiId, e.OlcmeTuru }, "UQ_Notlar_DersKaydi_OlcmeTuru").IsUnique();

            entity.Property(e => e.OlcmeTuru)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Puan).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.DersKaydi).WithMany(p => p.Notlars)
                .HasForeignKey(d => d.DersKaydiId)
                .HasConstraintName("FK_Notlar_DersKaydi");
        });

        modelBuilder.Entity<Ogrenci>(entity =>
        {
            entity.ToTable("Ogrenci");

            entity.HasIndex(e => e.KullaniciId, "UQ_Ogrenci_Kullanici").IsUnique();

            entity.HasIndex(e => e.OgrenciNo, "UQ_Ogrenci_OgrenciNo").IsUnique();

            entity.Property(e => e.Cinsiyet)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Durum)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.OgrenciNo)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.OgrenciTipi)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Danisman).WithMany(p => p.Ogrencis)
                .HasForeignKey(d => d.DanismanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ogrenci_Danisman");

            entity.HasOne(d => d.Kullanici).WithOne(p => p.Ogrenci)
                .HasForeignKey<Ogrenci>(d => d.KullaniciId)
                .HasConstraintName("FK_Ogrenci_Kullanici");

            entity.HasOne(d => d.Organizasyon).WithMany(p => p.Ogrencis)
                .HasForeignKey(d => d.OrganizasyonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ogrenci_Organizasyon");
        });

        modelBuilder.Entity<OgretimUyesi>(entity =>
        {
            entity.ToTable("OgretimUyesi");

            entity.HasIndex(e => e.KullaniciId, "UQ_OgretimUyesi_Kullanici").IsUnique();

            entity.Property(e => e.Cinsiyet)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.KadroTipi)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Unvan)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Kullanici).WithOne(p => p.OgretimUyesi)
                .HasForeignKey<OgretimUyesi>(d => d.KullaniciId)
                .HasConstraintName("FK_OgretimUyesi_Kullanici");

            entity.HasOne(d => d.Organizasyon).WithMany(p => p.OgretimUyesis)
                .HasForeignKey(d => d.OrganizasyonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OgretimUyesi_Organizasyon");
        });

        modelBuilder.Entity<Organizasyon>(entity =>
        {
            entity.ToTable("Organizasyon");

            entity.HasIndex(e => e.Kodu, "UQ_Organizasyon_Kodu").IsUnique();

            entity.Property(e => e.Adi)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Durum).HasDefaultValue(true);
            entity.Property(e => e.Kodu)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Tipi)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.UstOrganizasyon).WithMany(p => p.InverseUstOrganizasyon)
                .HasForeignKey(d => d.UstOrganizasyonId)
                .HasConstraintName("FK_Organizasyon_Ust");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Rol");

            entity.HasIndex(e => e.RolAdi, "UQ_Rol_RolAdi").IsUnique();

            entity.Property(e => e.Aciklama)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.RolAdi)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RolYetki>(entity =>
        {
            entity.HasKey(e => new { e.RolId, e.YetkiId });

            entity.ToTable("RolYetki");

            entity.Property(e => e.BaslangicTarihi).HasColumnType("datetime");
            entity.Property(e => e.BitisTarihi).HasColumnType("datetime");

            entity.HasOne(d => d.Rol).WithMany(p => p.RolYetkis)
                .HasForeignKey(d => d.RolId)
                .HasConstraintName("FK_RolYetki_Rol");

            entity.HasOne(d => d.Yetki).WithMany(p => p.RolYetkis)
                .HasForeignKey(d => d.YetkiId)
                .HasConstraintName("FK_RolYetki_Yetki");
        });

        modelBuilder.Entity<Salon>(entity =>
        {
            entity.ToTable("Salon");

            entity.HasIndex(e => new { e.BinaId, e.SalonAdi }, "UQ_Salon_Bina_Adi").IsUnique();

            entity.Property(e => e.SalonAdi)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SalonTipi)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Bina).WithMany(p => p.InverseBina)
                .HasForeignKey(d => d.BinaId)
                .HasConstraintName("FK_Salon_Bina");
        });

        modelBuilder.Entity<SinavProgrami>(entity =>
        {
            entity.ToTable("SinavProgrami");

            entity.HasIndex(e => new { e.SalonId, e.Baslangic }, "UQ_SinavProgrami_Salon_Baslangic").IsUnique();

            entity.Property(e => e.Baslangic).HasColumnType("datetime");
            entity.Property(e => e.Bitis).HasColumnType("datetime");
            entity.Property(e => e.SinavTipi)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.AcilanDers).WithMany(p => p.SinavProgramis)
                .HasForeignKey(d => d.AcilanDersId)
                .HasConstraintName("FK_SinavProgrami_AcilanDers");

            entity.HasOne(d => d.Salon).WithMany(p => p.SinavProgramis)
                .HasForeignKey(d => d.SalonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SinavProgrami_Salon");
        });

        modelBuilder.Entity<Yetki>(entity =>
        {
            entity.ToTable("Yetki");

            entity.HasIndex(e => e.YetkiKodu, "UQ_Yetki_YetkiKodu").IsUnique();

            entity.Property(e => e.Aciklama)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.YetkiKodu)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
