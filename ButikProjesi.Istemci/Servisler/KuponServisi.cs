using ButikProjesi.Shared.Modeller;
using System.Text.Json;

public class KuponServisi
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public KuponServisi(HttpClient http) 
    { 
        _http = http;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<Kupon>> TumKuponlariGetirAsync()
    {
        var response = await _http.GetAsync("api/kuponlar");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Kupon>>(json, _jsonOptions) ?? new List<Kupon>();
        }
        return new List<Kupon>();
    }

    public async Task<Kupon?> KuponEkleAsync(Kupon yeniKupon)
    {
        var json = JsonSerializer.Serialize(yeniKupon, _jsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await _http.PostAsync("api/kuponlar", content);
        
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Kupon>(responseJson, _jsonOptions);
        }
        return null;
    }

    public async Task<(bool basarili, decimal indirimDegeri, string mesaj, IndirimTipi tipi)> KuponDogrulaAsync(string kuponKodu)
    {
        try
        {
            var response = await _http.GetAsync($"api/kuponlar/dogrula/{kuponKodu}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var kupon = JsonSerializer.Deserialize<Kupon>(json, _jsonOptions);
                
                if (kupon != null)
                {
                    var indirimDegeri = kupon.Deger;
                    var tipi = kupon.Tipi;
                    var mesaj = $"Kupon geçerli! {indirimDegeri} {(tipi == IndirimTipi.Yuzde ? "%" : "₺")} indirim kazandınız";
                    return (true, indirimDegeri, mesaj, tipi);
                }
                else
                {
                    return (false, 0, "Geçersiz kupon", IndirimTipi.Yuzde);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return (false, 0, "Geçersiz kupon", IndirimTipi.Yuzde);
            }
            return (false, 0, "Kupon doğrulanamadı", IndirimTipi.Yuzde);
        }
        catch (Exception ex)
        {
            return (false, 0, $"Kupon doğrulama hatası: {ex.Message}", IndirimTipi.Yuzde);
        }
    }

    public decimal IndirimTutariHesapla(decimal toplamTutar, decimal indirimOrani)
    {
        return toplamTutar * (indirimOrani / 100);
    }

    public decimal IndirimliTutarHesapla(decimal toplamTutar, decimal indirimTutari)
    {
        return Math.Max(0, toplamTutar - indirimTutari);
    }

    public async Task ToggleAktifAsync(int kuponId)
    {
        await _http.PutAsync($"api/kuponlar/{kuponId}/toggle-aktif", null);
    }

    public async Task KuponSilAsync(int kuponId)
    {
        await _http.DeleteAsync($"api/kuponlar/{kuponId}");
    }
}