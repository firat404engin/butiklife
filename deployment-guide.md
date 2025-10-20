# 🚀 ButikLife Projesi Canlıya Alma Rehberi

## Azure ile Ücretsiz Deployment

### 1. Azure Hesabı Oluşturma
1. https://azure.microsoft.com/tr-tr/free/ adresine gidin
2. "Ücretsiz hesap oluştur" butonuna tıklayın
3. Kredi kartı bilgilerinizi girin (ücret alınmaz)
4. $200 ücretsiz kredi alacaksınız

### 2. SQL Database Oluşturma
1. Azure Portal'da "SQL databases" arayın
2. "Create" butonuna tıklayın
3. Database name: `butiklife-db`
4. Server: Yeni server oluşturun
5. Pricing tier: "Basic" seçin (ücretsiz)
6. Connection string'i kaydedin

### 3. App Service Oluşturma (API)
1. "App Services" arayın
2. "Create" butonuna tıklayın
3. Name: `butiklife-api`
4. Runtime: .NET 9
5. Region: West Europe
6. Pricing tier: "F1 Free" seçin

### 4. Static Web App Oluşturma (Frontend)
1. "Static Web Apps" arayın
2. "Create" butonuna tıklayın
3. Name: `butiklife-frontend`
4. Source: GitHub
5. Repository: Projenizi GitHub'a yükleyin
6. Build details: `ButikProjesi.Istemci` klasörü

### 5. Proje Hazırlığı

#### API Projesi (.csproj)
```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

#### Connection String Güncelleme
```json
{
  "ConnectionStrings": {
    "ButikDB": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=butiklife-db;Persist Security Info=False;User ID=your-username;Password=your-password;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### 6. Deployment Komutları

#### API Deployment
```bash
# API projesini publish et
cd ButikProjesi.API
dotnet publish -c Release -o ./publish

# Azure'a deploy et
az webapp deployment source config-zip --resource-group myResourceGroup --name butiklife-api --src ./publish.zip
```

#### Frontend Deployment
```bash
# Frontend projesini publish et
cd ButikProjesi.Istemci
dotnet publish -c Release -o ./publish

# Static Web App'e deploy et (GitHub Actions otomatik yapacak)
```

### 7. CORS Ayarları
API'de CORS ayarlarını güncelleyin:
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://your-static-web-app.azurestaticapps.net")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### 8. Environment Variables
Azure App Service'de:
- `ASPNETCORE_ENVIRONMENT`: Production
- `ConnectionStrings__ButikDB`: SQL connection string

### 9. Domain ve SSL
- Azure otomatik SSL sertifikası sağlar
- Custom domain ekleyebilirsiniz
- HTTPS otomatik aktif

## Alternatif: Vercel + Railway

### Vercel (Frontend)
1. https://vercel.com adresine gidin
2. GitHub hesabınızla giriş yapın
3. Repository'yi import edin
4. Build command: `cd ButikProjesi.Istemci && dotnet publish -c Release -o ./publish`
5. Output directory: `ButikProjesi.Istemci/publish/wwwroot`

### Railway (Backend + Database)
1. https://railway.app adresine gidin
2. GitHub hesabınızla giriş yapın
3. "Deploy from GitHub" seçin
4. API projenizi seçin
5. PostgreSQL addon ekleyin
6. Environment variables ayarlayın

## Maliyet Analizi

### Azure (12 ay ücretsiz)
- App Service: $0 (F1 Free)
- SQL Database: $0 (Basic tier)
- Static Web App: $0
- **Toplam: $0/ay (12 ay)**

### Vercel + Railway
- Vercel: $0 (sınırsız)
- Railway: $0 (500 saat/ay)
- **Toplam: $0/ay**

## Güvenlik Önerileri

1. **Connection String**: Environment variables kullanın
2. **API Keys**: Azure Key Vault kullanın
3. **HTTPS**: Otomatik aktif
4. **CORS**: Sadece gerekli domain'ler
5. **Authentication**: JWT token kullanın

## Monitoring

1. **Azure Application Insights**: Ücretsiz
2. **Logs**: Azure Portal'da görüntüleyin
3. **Metrics**: CPU, Memory, Requests
4. **Alerts**: Email/SMS bildirimleri

## Backup Stratejisi

1. **Database**: Azure otomatik backup
2. **Code**: GitHub'da version control
3. **Configuration**: Azure Key Vault
4. **Logs**: Azure Log Analytics

## Troubleshooting

### Yaygın Sorunlar
1. **CORS Hatası**: Origin URL'lerini kontrol edin
2. **Database Connection**: Connection string'i kontrol edin
3. **Build Hatası**: .NET 9 runtime'ın yüklü olduğundan emin olun
4. **Static Files**: wwwroot klasörünün doğru yerde olduğundan emin olun

### Log Kontrolü
```bash
# Azure CLI ile log'ları görüntüleyin
az webapp log tail --name butiklife-api --resource-group myResourceGroup
```

## Sonuç

Azure en iyi seçenek çünkü:
- ✅ Tam .NET desteği
- ✅ SQL Server native
- ✅ 12 ay ücretsiz
- ✅ Kolay deployment
- ✅ Güvenilir altyapı

Bu rehberi takip ederek projenizi ücretsiz olarak canlıya alabilirsiniz!


