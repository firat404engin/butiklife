using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using ButikProjesi.Istemci;
using ButikProjesi.Istemci.Servisler;
using MudBlazor.Services;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// CookieHandler'ı DI'ye ekle
builder.Services.AddScoped<CookieHandler>();

// HttpClient'ı API adresine yönlendir (Cookie desteği ile)
builder.Services.AddScoped(sp => 
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiBaseAddress = configuration["ApiBaseAddress"] ?? "http://localhost:5154";
    
    // HttpClient'ı CookieHandler ile sarmalayarak oluştur
    var httpClient = new HttpClient(new CookieHandler())
    { 
        BaseAddress = new Uri(apiBaseAddress)
    };
    
    return httpClient;
});

// Blazored Local Storage servisini ekle
builder.Services.AddBlazoredLocalStorage();

// UrunServisi'ni dependency injection'a ekle
builder.Services.AddScoped<UrunServisi>();

// SepetServisi'ni Scoped olarak ekle (LocalStorage da Scoped olduğu için)
builder.Services.AddScoped<SepetServisi>();

// SiparisServisi'ni dependency injection'a ekle
builder.Services.AddScoped<SiparisServisi>();

// YorumServisi'ni dependency injection'a ekle
builder.Services.AddScoped<YorumServisi>();

// FavoriServisi'ni dependency injection'a ekle
builder.Services.AddScoped<FavoriServisi>();

// KuponServisi'ni dependency injection'a ekle
builder.Services.AddScoped<KuponServisi>();

// AuthServisi'ni dependency injection'a ekle
builder.Services.AddScoped<AuthServisi>();

// MusteriYonetimiServisi'ni dependency injection'a ekle
builder.Services.AddScoped<MusteriYonetimiServisi>();

// CustomAuthenticationStateProvider'ı kaydet
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Authorization servislerini ekle
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// MudBlazor servislerini ekle
builder.Services.AddMudServices();

await builder.Build().RunAsync();
