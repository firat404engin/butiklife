# Azure Deployment Script for ButikLife Project
# Bu script'i çalıştırarak projenizi Azure'a deploy edebilirsiniz

Write-Host "🚀 ButikLife Azure Deployment Başlıyor..." -ForegroundColor Green

# 1. Azure CLI yüklü mü kontrol et
try {
    az --version | Out-Null
    Write-Host "✅ Azure CLI yüklü" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI yüklü değil. Lütfen önce yükleyin: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Red
    exit 1
}

# 2. Azure'a giriş yap
Write-Host "🔐 Azure'a giriş yapılıyor..." -ForegroundColor Yellow
az login

# 3. Resource Group oluştur
Write-Host "📦 Resource Group oluşturuluyor..." -ForegroundColor Yellow
az group create --name butiklife-rg --location westeurope

# 4. SQL Server oluştur
Write-Host "🗄️ SQL Server oluşturuluyor..." -ForegroundColor Yellow
az sql server create --name butiklife-sql-server --resource-group butiklife-rg --location westeurope --admin-user butiklifeadmin --admin-password "ButikLife123!"

# 5. SQL Database oluştur
Write-Host "💾 SQL Database oluşturuluyor..." -ForegroundColor Yellow
az sql db create --resource-group butiklife-rg --server butiklife-sql-server --name butiklife-db --service-objective Basic

# 6. Firewall kuralı ekle (Azure servislerinden erişim)
Write-Host "🔥 Firewall kuralı ekleniyor..." -ForegroundColor Yellow
az sql server firewall-rule create --resource-group butiklife-rg --server butiklife-sql-server --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# 7. App Service Plan oluştur
Write-Host "📋 App Service Plan oluşturuluyor..." -ForegroundColor Yellow
az appservice plan create --name butiklife-plan --resource-group butiklife-rg --sku F1 --is-linux

# 8. Web App oluştur (API)
Write-Host "🌐 Web App oluşturuluyor..." -ForegroundColor Yellow
az webapp create --resource-group butiklife-rg --plan butiklife-plan --name butiklife-api --runtime "DOTNET|9.0"

# 9. Static Web App oluştur (Frontend)
Write-Host "⚡ Static Web App oluşturuluyor..." -ForegroundColor Yellow
az staticwebapp create --name butiklife-frontend --resource-group butiklife-rg --location westeurope --source https://github.com/yourusername/butiklife --branch main --app-location "ButikProjesi.Istemci" --output-location "wwwroot"

# 10. Connection string ayarla
Write-Host "🔗 Connection string ayarlanıyor..." -ForegroundColor Yellow
$connectionString = "Server=tcp:butiklife-sql-server.database.windows.net,1433;Initial Catalog=butiklife-db;Persist Security Info=False;User ID=butiklifeadmin;Password=ButikLife123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
az webapp config connection-string set --resource-group butiklife-rg --name butiklife-api --connection-string-type SQLServer --settings ButikDB="$connectionString"

# 11. CORS ayarla
Write-Host "🌐 CORS ayarlanıyor..." -ForegroundColor Yellow
az webapp cors add --resource-group butiklife-rg --name butiklife-api --allowed-origins "https://butiklife-frontend.azurestaticapps.net"

Write-Host "✅ Deployment tamamlandı!" -ForegroundColor Green
Write-Host "🌐 API URL: https://butiklife-api.azurewebsites.net" -ForegroundColor Cyan
Write-Host "🌐 Frontend URL: https://butiklife-frontend.azurestaticapps.net" -ForegroundColor Cyan
Write-Host "🗄️ SQL Server: butiklife-sql-server.database.windows.net" -ForegroundColor Cyan

Write-Host "`n📋 Sonraki Adımlar:" -ForegroundColor Yellow
Write-Host "1. Projenizi GitHub'a yükleyin" -ForegroundColor White
Write-Host "2. Connection string'i güncelleyin" -ForegroundColor White
Write-Host "3. CORS ayarlarını kontrol edin" -ForegroundColor White
Write-Host "4. Database migration'ları çalıştırın" -ForegroundColor White


