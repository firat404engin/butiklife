# 🛍️ ButikLife E-Ticaret Projesi - Teknik Dokümantasyon

## 📋 Proje Genel Bakış

**ButikLife**, modern e-ticaret ihtiyaçlarını karşılamak üzere geliştirilmiş, tam özellikli bir online mağaza uygulamasıdır. Proje, müşteri deneyimini ön planda tutarak, kullanıcı dostu arayüzü ve güçlü backend altyapısı ile dikkat çeker.

### 🎯 Proje Amacı
- Modern e-ticaret deneyimi sunmak
- Kullanıcı dostu arayüz ile kolay alışveriş imkanı
- Admin paneli ile kolay ürün yönetimi
- Favori ürünler ve bildirim sistemi
- Güvenli ödeme ve sipariş takibi

---

## 🏗️ Mimari Yapı

### Genel Mimari
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Blazor WASM   │    │   .NET 9 API    │    │   SQLite DB     │
│   (Frontend)    │◄──►│   (Backend)     │◄──►│   (Database)    │
│                 │    │                 │    │                 │
│ • MudBlazor UI  │    │ • REST API      │    │ • Entity Models │
│ • Authentication│    │ • Identity      │    │ • Migrations    │
│ • State Mgmt    │    │ • CORS          │    │ • Relationships │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Proje Yapısı
```
ButikProjesi/
├── ButikProjesi.API/           # Backend API (.NET 9)
│   ├── Controllers/            # REST API Controller'ları
│   ├── Modeller/              # Entity Framework Modelleri
│   ├── Migrations/            # Veritabanı Migrations
│   └── Program.cs             # API Konfigürasyonu
├── ButikProjesi.Istemci/      # Frontend (Blazor WASM)
│   ├── Pages/                 # Razor Sayfaları
│   ├── Layout/                # Layout Bileşenleri
│   ├── Servisler/             # API Servisleri
│   ├── Tema/                  # MudBlazor Tema
│   └── wwwroot/               # Statik Dosyalar
├── ButikProjesi.Shared/       # Paylaşılan Modeller
└── Dockerfile                 # Container Konfigürasyonu
```

---

## 🛠️ Kullanılan Teknolojiler

### Frontend Teknolojileri
- **Blazor WebAssembly (.NET 9)** - Modern web uygulaması framework'ü
- **MudBlazor** - Material Design bileşen kütüphanesi
- **Blazored.LocalStorage** - Tarayıcı localStorage yönetimi
- **Microsoft.AspNetCore.Components.Authorization** - Kimlik doğrulama
- **CSS3** - Özel stil ve animasyonlar

### Backend Teknolojileri
- **ASP.NET Core 9** - Web API framework'ü
- **Entity Framework Core** - ORM (Object-Relational Mapping)
- **SQLite** - Hafif, dosya tabanlı veritabanı
- **ASP.NET Core Identity** - Kullanıcı yönetimi ve kimlik doğrulama
- **Swagger/OpenAPI** - API dokümantasyonu

### DevOps & Deployment
- **Docker** - Containerization
- **Render.com** - Cloud hosting (ücretsiz tier)
- **GitHub** - Version control
- **SQLite** - Veritabanı (ücretsiz)

---

## 🗄️ Veritabanı Yapısı

### Ana Tablolar

#### 1. ApplicationUser (Kullanıcılar)
```sql
- Id (string, PK)
- UserName (string)
- Email (string)
- AdSoyad (string)
- EmailConfirmed (bool)
- PasswordHash (string)
- SecurityStamp (string)
```

#### 2. Urun (Ürünler)
```sql
- Id (int, PK)
- Ad (string, 200 char)
- Aciklama (string, 1000 char)
- Fiyat (decimal)
- EskiFiyat (decimal, nullable)
- StokAdedi (int)
- GorselUrl (string, 500 char)
- KategoriId (int, FK)
```

#### 3. Kategori (Kategoriler)
```sql
- Id (int, PK)
- Ad (string, 100 char)
- Aciklama (string, 500 char)
- GorselUrl (string, 500 char)
```

#### 4. Siparis (Siparişler)
```sql
- Id (int, PK)
- KullaniciId (string, FK)
- ToplamTutar (decimal)
- SiparisTarihi (datetime)
- Durum (string)
- Adres (string)
- Telefon (string)
```

#### 5. SiparisKalemi (Sipariş Kalemleri)
```sql
- Id (int, PK)
- SiparisId (int, FK)
- UrunId (int, FK)
- Miktar (int)
- BirimFiyat (decimal)
```

#### 6. Favori (Favoriler)
```sql
- Id (int, PK)
- KullaniciId (string, FK)
- UrunId (int, FK)
- FiyatEklendiginde (decimal)
```

#### 7. Yorum (Yorumlar)
```sql
- Id (int, PK)
- KullaniciId (string, FK)
- UrunId (int, FK)
- Metin (string, 1000 char)
- Puan (int)
- Tarih (datetime)
```

#### 8. Kupon (Kuponlar)
```sql
- Id (int, PK)
- Kod (string, 50 char)
- IndirimOrani (decimal)
- MinTutar (decimal)
- BitisTarihi (datetime)
- KullanilmaSayisi (int)
```

#### 9. Bildirim (Bildirimler)
```sql
- Id (int, PK)
- KullaniciId (string, FK)
- UrunId (int, FK)
- Mesaj (string, 500 char)
- OkunduMu (bool)
- OlusturmaTarihi (datetime)
```

---

## 🎨 Kullanıcı Arayüzü Özellikleri

### Ana Sayfa
- **Hero Section** - Modern görsel tasarım
- **En Çok Satanlar** - Tıklanabilir ürün kartları
- **Koleksiyon Butonları** - Modern CTA tasarımı
- **Responsive Design** - Mobil uyumlu

### Ürün Sayfaları
- **Ürün Listesi** - Filtreleme ve arama
- **Ürün Detayı** - Görsel galeri, yorumlar
- **Favori Ekleme** - Kullanıcı favorileri
- **Sepet İşlemleri** - Ekleme/çıkarma

### Admin Paneli
- **Ürün Yönetimi** - CRUD işlemleri
- **Kategori Yönetimi** - Kategori düzenleme
- **Sipariş Takibi** - Sipariş durumu güncelleme
- **Kupon Yönetimi** - İndirim kuponları

### Kullanıcı Özellikleri
- **Kayıt/Giriş** - Güvenli kimlik doğrulama
- **Profil Yönetimi** - Kişisel bilgi güncelleme
- **Sipariş Geçmişi** - Geçmiş siparişler
- **Favori Listesi** - Kayıtlı ürünler
- **Bildirim Sistemi** - Fiyat düşüş bildirimleri

---

## 🔧 API Endpoints

### Ürün Yönetimi
```
GET    /api/urunler              # Tüm ürünleri listele
GET    /api/urunler/{id}         # Ürün detayı
POST   /api/urunler              # Yeni ürün ekle (Admin)
PUT    /api/urunler/{id}         # Ürün güncelle (Admin)
DELETE /api/urunler/{id}         # Ürün sil (Admin)
GET    /api/urunler/kategori/{kategoriId}  # Kategoriye göre ürünler
```

### Sipariş Yönetimi
```
GET    /api/siparisler           # Kullanıcı siparişleri
POST   /api/siparisler           # Yeni sipariş oluştur
GET    /api/siparisler/{id}      # Sipariş detayı
PUT    /api/siparisler/{id}/durum # Sipariş durumu güncelle (Admin)
```

### Kullanıcı Yönetimi
```
POST   /api/hesap/kayit          # Kullanıcı kaydı
POST   /api/hesap/giris          # Kullanıcı girişi
POST   /api/hesap/cikis          # Kullanıcı çıkışı
GET    /api/hesap/profil         # Profil bilgileri
PUT    /api/hesap/profil         # Profil güncelleme
```

### Favori Yönetimi
```
GET    /api/favoriler            # Kullanıcı favorileri
POST   /api/favoriler            # Favori ekle
DELETE /api/favoriler/{id}       # Favori sil
```

---

## 🚀 Deployment Süreci

### 1. Gereksinimler
- .NET 9 SDK
- Docker (opsiyonel)
- GitHub hesabı
- Render.com hesabı (ücretsiz)

### 2. Local Development
```bash
# Projeyi klonla
git clone <repository-url>
cd butiklife

# API'yi çalıştır
cd ButikProjesi.API
dotnet run

# Client'ı çalıştır
cd ../ButikProjesi.Istemci
dotnet run
```

### 3. Production Deployment
```bash
# Docker ile build
docker build -t butiklife-api .

# Render.com'a deploy
# 1. GitHub'a push
git add .
git commit -m "Deploy to production"
git push origin main

# 2. Render.com'da Web Service oluştur
# 3. Environment variables ayarla
# 4. Auto-deploy aktif
```

---

## 🔒 Güvenlik Özellikleri

### Kimlik Doğrulama
- **ASP.NET Core Identity** - Güvenli kullanıcı yönetimi
- **JWT Token** - Stateless authentication
- **Password Hashing** - Güvenli şifre saklama
- **Role-based Authorization** - Admin/Müşteri rolleri

### Veri Güvenliği
- **SQL Injection Koruması** - Entity Framework ORM
- **CORS Konfigürasyonu** - Cross-origin güvenlik
- **Input Validation** - Model validation attributes
- **HTTPS Zorunluluğu** - Güvenli iletişim

---

## 📊 Performans Optimizasyonları

### Frontend
- **Blazor WebAssembly** - Client-side rendering
- **Lazy Loading** - Sayfa bazlı yükleme
- **Local Storage** - Offline veri saklama
- **Responsive Images** - Optimize edilmiş görseller

### Backend
- **Entity Framework** - Optimized queries
- **Async/Await** - Non-blocking operations
- **Connection Pooling** - Veritabanı bağlantı yönetimi
- **Caching** - Memory-based caching

---

## 🧪 Test Stratejisi

### Unit Tests
- Model validation tests
- Service layer tests
- Controller action tests

### Integration Tests
- API endpoint tests
- Database integration tests
- Authentication flow tests

### Manual Testing
- User journey testing
- Cross-browser compatibility
- Mobile responsiveness

---

## 📈 Gelecek Geliştirmeler

### Kısa Vadeli (1-2 ay)
- [ ] Ödeme sistemi entegrasyonu
- [ ] Email bildirim sistemi
- [ ] Gelişmiş arama ve filtreleme
- [ ] Ürün yorum sistemi iyileştirmeleri

### Orta Vadeli (3-6 ay)
- [ ] Mobil uygulama (React Native/Flutter)
- [ ] Çoklu dil desteği
- [ ] Gelişmiş admin dashboard
- [ ] Raporlama sistemi

### Uzun Vadeli (6+ ay)
- [ ] Mikroservis mimarisi
- [ ] Machine Learning önerileri
- [ ] Real-time chat sistemi
- [ ] Blockchain entegrasyonu

---

## 🐛 Bilinen Sorunlar ve Çözümler

### Status 139 Hatası (Render Deployment)
**Sorun:** Docker container'da SQLite native library eksikliği
**Çözüm:** Dockerfile'da gerekli kütüphaneleri yükleme

### CORS Hatası
**Sorun:** Frontend-Backend arası iletişim sorunu
**Çözüm:** CORS policy konfigürasyonu

### Memory Issues
**Sorun:** Büyük veri setlerinde performans
**Çözüm:** Pagination ve lazy loading

---

## 📞 Destek ve İletişim

### Teknik Destek
- **GitHub Issues** - Bug raporları ve özellik istekleri
- **Documentation** - Detaylı API dokümantasyonu
- **Swagger UI** - Interactive API testing

### Geliştirici Notları
- Proje .NET 9 ile geliştirilmiştir
- SQLite veritabanı kullanılmaktadır
- Render.com ücretsiz tier için optimize edilmiştir
- MudBlazor UI framework'ü kullanılmaktadır

---

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için LICENSE dosyasına bakınız.

---

*Son güncelleme: Ocak 2025*
*Versiyon: 1.0.0*
*Geliştirici: Firat*
