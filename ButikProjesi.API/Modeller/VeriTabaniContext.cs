using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace ButikProjesi.API.Modeller
{
    /// <summary>
    /// Entity Framework veritabanı bağlam sınıfı
    /// Identity desteği ile genişletilmiş - ApplicationUser kullanıyor
    /// </summary>
    public class VeriTabaniContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// VeriTabaniContext constructor
        /// </summary>
        /// <param name="options">DbContext seçenekleri</param>
        public VeriTabaniContext(DbContextOptions<VeriTabaniContext> options) : base(options)
        {
        }

        /// <summary>
        /// Ürünler tablosu
        /// </summary>
        public DbSet<Urun> Urunler { get; set; }

        /// <summary>
        /// Kategoriler tablosu
        /// </summary>
        public DbSet<Kategori> Kategoriler { get; set; }

        /// <summary>
        /// Siparişler tablosu
        /// </summary>
        public DbSet<Siparis> Siparisler { get; set; }

        /// <summary>
        /// Sipariş kalemleri tablosu
        /// </summary>
        public DbSet<SiparisKalemi> SiparisKalemleri { get; set; }

        /// <summary>
        /// Yorumlar tablosu
        /// </summary>
        public DbSet<Yorum> Yorumlar { get; set; }

        /// <summary>
        /// Favoriler tablosu
        /// </summary>
        public DbSet<Favori> Favoriler { get; set; }

        /// <summary>
        /// Kuponlar tablosu
        /// </summary>
        public DbSet<Kupon> Kuponlar { get; set; }

        /// <summary>
        /// Bildirimler tablosu
        /// </summary>
        public DbSet<Bildirim> Bildirimler { get; set; }

        /// <summary>
        /// Model yapılandırması
        /// </summary>
        /// <param name="modelBuilder">Model builder</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Kategori-Ürün ilişkisi (One-to-Many)
            modelBuilder.Entity<Urun>()
                .HasOne(u => u.Kategori)
                .WithMany(k => k.Urunler)
                .HasForeignKey(u => u.KategoriId)
                .OnDelete(DeleteBehavior.Restrict);

            // Sipariş-SiparişKalemi ilişkisi (One-to-Many)
            modelBuilder.Entity<SiparisKalemi>()
                .HasOne(sk => sk.Siparis)
                .WithMany(s => s.SiparisKalemleri)
                .HasForeignKey(sk => sk.SiparisId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ürün-SiparişKalemi ilişkisi (One-to-Many)
            modelBuilder.Entity<SiparisKalemi>()
                .HasOne(sk => sk.Urun)
                .WithMany(u => u.SiparisKalemleri)
                .HasForeignKey(sk => sk.UrunId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ürün-Yorum ilişkisi (One-to-Many)
            modelBuilder.Entity<Yorum>()
                .HasOne(y => y.Urun)
                .WithMany()
                .HasForeignKey(y => y.UrunId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ürün-Favori ilişkisi (One-to-Many)
            modelBuilder.Entity<Favori>()
                .HasOne(f => f.Urun)
                .WithMany()
                .HasForeignKey(f => f.UrunId)
                .OnDelete(DeleteBehavior.Cascade);

            // Kullanıcı-Favori için unique constraint (Bir kullanıcı aynı ürünü birden fazla kez favoriye ekleyemez)
            modelBuilder.Entity<Favori>()
                .HasIndex(f => new { f.KullaniciId, f.UrunId })
                .IsUnique();

            // Kategori tablosu için örnek veriler
            modelBuilder.Entity<Kategori>().HasData(
                new Kategori { Id = 1, Ad = "Tişörtler", GorselUrl = "https://images.pexels.com/photos/1232459/pexels-photo-1232459.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1" },
                new Kategori { Id = 2, Ad = "Pantolonlar", GorselUrl = "https://images.pexels.com/photos/934070/pexels-photo-934070.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1" }
            );

            // Ürün tablosu için örnek veriler
            modelBuilder.Entity<Urun>().HasData(
                new Urun 
                { 
                    Id = 1, 
                    Ad = "Pamuklu Beyaz Tişört", 
                    Aciklama = "Yumuşak pamuklu beyaz tişört", 
                    Fiyat = 500.00m, 
                    StokAdedi = 50, 
                    GorselUrl = "https://images.pexels.com/photos/428338/pexels-photo-428338.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1", 
                    KategoriId = 1 
                },
                new Urun 
                { 
                    Id = 2, 
                    Ad = "Baskılı Siyah Tişört", 
                    Aciklama = "Modern baskılı siyah tişört", 
                    Fiyat = 650.00m, 
                    StokAdedi = 30, 
                    GorselUrl = "https://images.pexels.com/photos/769733/pexels-photo-769733.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1", 
                    KategoriId = 1 
                },
                new Urun 
                { 
                    Id = 3, 
                    Ad = "Kot Pantolon", 
                    Aciklama = "Klasik mavi kot pantolon", 
                    Fiyat = 1200.00m, 
                    StokAdedi = 20, 
                    GorselUrl = "https://images.pexels.com/photos/52574/jeans-pants-blue-shop-52574.jpeg?auto=compress&cs=tinysrgb&w=1260&h=750&dpr=1", 
                    KategoriId = 2 
                }
            );
        }
    }
}
