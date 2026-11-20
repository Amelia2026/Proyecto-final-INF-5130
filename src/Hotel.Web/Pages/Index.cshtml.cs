using Hotel.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hotel.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApiCliente _api;

    public IndexModel(ApiCliente api) => _api = api;

    public ResumenApi? Resumen { get; private set; }
    public string? Error { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var respuesta = await _api.ObtenerResumenAsync();

        // Si la API ya no acepta el token (vencido), se cierra la sesión y se vuelve al login.
        if (respuesta.Estado == 401)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login");
        }

        Resumen = respuesta.Datos;
        Error = respuesta.Error;
        return Page();
    }
}
