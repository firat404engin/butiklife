using ButikProjesi.API.Modeller;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Controller'ları ekle
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Döngüsel referans sorununu çöz
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Swagger/OpenAPI servislerini ekle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Veritabanı bağlantısını servislere ekle (SQLite)
var sqliteConn = Environment.GetEnvironmentVariable("CONN")
                 ?? builder.Configuration.GetConnectionString("ButikDB")
                 ?? "Data Source=butik.db";

// Railway/Cloud ortamında memory database kullan
if (Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT") != null || 
    Environment.GetEnvironmentVariable("RENDER") != null)
{
    sqliteConn = "Data Source=:memory:";
}

builder.Services.AddDbContext<VeriTabaniContext>(options =>
    options.UseSqlite(sqliteConn));

// Identity servislerini ekle (ApplicationUser kullanarak)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Şifre gereksinimleri
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // Kullanıcı gereksinimleri
    options.User.RequireUniqueEmail = true;

    // Sign-in gereksinimleri
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<VeriTabaniContext>()
.AddDefaultTokenProviders();

// CORS ayarları (geçici olarak serbest, prod URL'leri geldikçe daraltılacak)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// CORS'u kullan
app.UseCors("AllowBlazorClient");

// Authentication ve Authorization middleware'lerini ekle
app.UseAuthentication();
app.UseAuthorization();

// Veritabanını oluştur ve migration'ları uygula
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<VeriTabaniContext>();
    try
    {
        // Önce veritabanının var olup olmadığını kontrol et
        var canConnect = await context.Database.CanConnectAsync();
        Console.WriteLine($"Veritabanı bağlantısı: {canConnect}");
        
        if (!canConnect)
        {
            // Veritabanı yoksa oluştur
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("Veritabanı oluşturuldu");
        }
        
        // Migration'ları uygula
        await context.Database.MigrateAsync();
        Console.WriteLine("Migration'lar uygulandı");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Veritabanı kurulum hatası: {ex.Message}");
        Console.WriteLine($"Hata detayı: {ex.InnerException?.Message}");
    }
}

// Admin rolü ve kullanıcısını oluştur
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    
    // Admin rolünü oluştur
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
        Console.WriteLine("Admin rolü oluşturuldu");
    }
    
    // Admin kullanıcısını oluştur veya şifresini sıfırla
    var adminEmail = "admin@butik.com";
    var adminPassword = "Admin123!";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    
    if (adminUser != null)
    {
        // Admin kullanıcısı mevcut - Şifresini sıfırla
        Console.WriteLine($"Admin kullanıcısı bulundu. Şifre sıfırlanıyor...");
        
        // Şifre sıfırlama token'ı oluştur
        var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
        
        // Token ile şifreyi sıfırla
        var resetResult = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
        
        if (resetResult.Succeeded)
        {
            Console.WriteLine($"Admin kullanıcısı şifresi başarıyla sıfırlandı: {adminEmail}");
        }
        else
        {
            Console.WriteLine($"Admin şifresi sıfırlanamadı: {string.Join(", ", resetResult.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        // Admin kullanıcısı yok - Yeni oluştur
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            AdSoyad = "Admin Kullanıcı"
        };
        
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"Admin kullanıcısı oluşturuldu: {adminEmail}");
        }
        else
        {
            Console.WriteLine($"Admin kullanıcısı oluşturulamadı: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}

// Controller'ları map et
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
