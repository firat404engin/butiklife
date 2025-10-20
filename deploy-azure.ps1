# 🚀 ButikLife Azure Deployment Script
# Bu script'i çalıştırarak projenizi Azure'a deploy edebilirsiniz

Write-Host "🚀 ButikLife Azure Deployment Başlıyor..." -ForegroundColor Green

# Azure CLI yüklü mü kontrol et
try {
    az --version | Out-Null
    Write-Host "✅ Azure CLI yüklü" -ForegroundColor Green
} catch {
    Write-Host "❌ Azure CLI yüklü değil. Lütfen önce yükleyin:" -ForegroundColor Red
    Write-Host "https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Yellow
    exit 1
}

# Azure'a giriş yap
Write-Host "🔐 Azure'a giriş yapılıyor..." -ForegroundColor Yellow
az login

# Resource Group oluştur
Write-Host "📦 Resource Group oluşturuluyor..." -ForegroundColor Yellow
az group create --name butiklife-rg --location westeurope

# SQL Server oluştur
Write-Host "🗄️ SQL Server oluşturuluyor..." -ForegroundColor Yellow
az sql server create --name butiklife-sql-server --resource-group butiklife-rg --location westeurope --admin-user butiklifeadmin --admin-password "ButikLife123!"

# SQL Database oluştur
Write-Host "💾 SQL Database oluşturuluyor..." -ForegroundColor Yellow
az sql db create --resource-group butiklife-rg --server butiklife-sql-server --name butiklife-db --service-objective Basic

# Firewall kuralı ekle
Write-Host "🔥 Firewall kuralı ekleniyor..." -ForegroundColor Yellow
az sql server firewall-rule create --resource-group butiklife-rg --server butiklife-sql-server --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# App Service Plan oluştur
Write-Host "📋 App Service Plan oluşturuluyor..." -ForegroundColor Yellow
az appservice plan create --name butiklife-plan --resource-group butiklife-rg --sku F1 --is-linux

# Web App oluştur (API)
Write-Host "🌐 Web App oluşturuluyor..." -ForegroundColor Yellow
az webapp create --resource-group butiklife-rg --plan butiklife-plan --name butiklife-api --runtime "DOTNET|9.0"

# Connection string ayarla
Write-Host "🔗 Connection string ayarlanıyor..." -ForegroundColor Yellow
$connectionString = "Server=tcp:butiklife-sql-server.database.windows.net,1433;Initial Catalog=butiklife-db;Persist Security Info=False;User ID=butiklifeadmin;Password=ButikLife123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
az webapp config connection-string set --resource-group butiklife-rg --name butiklife-api --connection-string-type SQLServer --settings ButikDB="$connectionString"

Write-Host "✅ Azure kaynakları oluşturuldu!" -ForegroundColor Green
Write-Host "🌐 API URL: https://butiklife-api.azurewebsites.net" -ForegroundColor Cyan
Write-Host "🗄️ SQL Server: butiklife-sql-server.database.windows.net" -ForegroundColor Cyan

Write-Host "`n📋 Sonraki Adımlar:" -ForegroundColor Yellow
Write-Host "1. Projenizi GitHub'a yükleyin" -ForegroundColor White
Write-Host "2. Azure Portal'da Static Web App oluşturun" -ForegroundColor White
Write-Host "3. Database migration'ları çalıştırın" -ForegroundColor White
Write-Host "4. CORS ayarlarını güncelleyin" -ForegroundColor White

Write-Host "`n🔗 Azure Portal: https://portal.azure.com" -ForegroundColor Blue


