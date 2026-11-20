using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Hotel.Web.Services;

/// <summary>Toma el JWT guardado en la cookie de la sesión y lo envía a la API como "Authorization: Bearer ...".</summary>
public class TokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accesor;

    public TokenHandler(IHttpContextAccessor accesor) => _accesor = accesor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var contexto = _accesor.HttpContext;
        if (contexto is not null)
        {
            var token = await contexto.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, ct);
    }
}
