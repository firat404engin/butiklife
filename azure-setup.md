# 🚀 ButikLife Azure Setup Rehberi

## Hızlı Başlangıç (5 Dakika)

### 1. Azure Portal'a Giriş
- https://portal.azure.com
- Ücretsiz hesabınızla giriş yapın

### 2. Resource Group Oluştur
```
Name: butiklife-rg
Region: West Europe
```

### 3. SQL Database Oluştur
```
Server Name: butiklife-sql-server
Database Name: butiklife-db
Pricing Tier: Basic (ücretsiz)
Admin: butiklifeadmin
Password: ButikLife123!
```

### 4. App Service Oluştur (API)
```
Name: butiklife-api
Runtime: .NET 9
Pricing Tier: F1 Free
```

### 5. Static Web App Oluştur (Frontend)
```
Name: butiklife-frontend
Source: GitHub
Repository: your-repo
App Location: ButikProjesi.Istemci
Output Location: wwwroot
```

## Proje Hazırlığı

### Connection String Güncelleme
```json
{
  "ConnectionStrings": {
    "ButikDB": "Server=tcp:butiklife-sql-server.database.windows.net,1433;Initial Catalog=butiklife-db;Persist Security Info=False;User ID=butiklifeadmin;Password=ButikLife123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### CORS Ayarları
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://butiklife-frontend.azurestaticapps.net")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

## Deployment Adımları

### 1. GitHub'a Yükle
```bash
git init
git add .
git commit -m "Initial commit"
git remote add origin https://github.com/yourusername/butiklife.git
git push -u origin main
```

### 2. Azure'a Deploy Et
- Azure Portal'da "Deploy from GitHub" seçin
- Repository'nizi seçin
- Otomatik deployment başlayacak

### 3. Database Migration
```bash
# Azure App Service'de
dotnet ef database update
```

## Maliyet Analizi

### Ücretsiz Limitler (12 ay)
- App Service: 1 milyon istek/ay
- SQL Database: 32GB
- Static Web App: Sınırsız
- **Toplam: $0/ay**

### 12 Ay Sonra
- App Service F1: $0/ay (sınırlı)
- SQL Database Basic: ~$5/ay
- **Toplam: ~$5/ay**

## Güvenlik

### Environment Variables
```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__ButikDB=<connection-string>
```

### SSL/HTTPS
- Azure otomatik SSL sağlar
- HTTPS zorunlu
- Custom domain eklenebilir

## Monitoring

### Application Insights
- Ücretsiz 5GB log/ay
- Performance monitoring
- Error tracking
- User analytics

### Logs
- Azure Portal'da görüntüleme
- Real-time monitoring
- Alert kuralları

## Backup

### Database
- Otomatik backup (7 gün)
- Point-in-time restore
- Geo-redundant storage

### Code
- GitHub version control
- Azure DevOps integration
- CI/CD pipeline

## Troubleshooting

### Yaygın Sorunlar
1. **CORS Hatası**: Origin URL'lerini kontrol edin
2. **Database Connection**: Connection string'i kontrol edin
3. **Build Hatası**: .NET 9 runtime'ın yüklü olduğundan emin olun
4. **Static Files**: wwwroot klasörünün doğru yerde olduğundan emin olun

### Log Kontrolü
```bash
# Azure CLI ile
az webapp log tail --name butiklife-api --resource-group butiklife-rg
```

## Sonuç

Azure ile:
- ✅ 12 ay ücretsiz
- ✅ .NET native desteği
- ✅ SQL Server uyumlu
- ✅ Kolay deployment
- ✅ Güvenilir altyapı
- ✅ Otomatik SSL
- ✅ Monitoring dahil

**Toplam Maliyet: $0/ay (12 ay)**


