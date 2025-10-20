using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace ButikProjesi.Istemci.Servisler
{
    /// <summary>
    /// HttpClient isteklerine cookie desteği ekleyen handler
    /// Blazor WASM'da cross-origin cookie'lerin gönderilmesini sağlar
    /// </summary>
    public class CookieHandler : DelegatingHandler
    {
        public CookieHandler() : base(new HttpClientHandler())
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            // Blazor WASM için cookie'lerin gönderilmesini etkinleştir
            // Bu, tarayıcının fetch API'sine credentials: 'include' eklenmesini sağlar
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            
            Console.WriteLine($"CookieHandler: {request.Method} {request.RequestUri} - Credentials: Include");
            
            return base.SendAsync(request, cancellationToken);
        }
    }
}

