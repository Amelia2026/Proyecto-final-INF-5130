using System.Security.Claims;
using Hotel.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Hotel.Web.Pages;

public class LoginModel : PageModel
{
    private readonly ApiCliente _api;

    public LoginModel(ApiCliente api) => _api = api;

    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Contrasena { get; set; } = "";
    public string? Error { get; set; }

    public IActionResult OnGet()
        => User.Identity?.IsAuthenticated == true ? RedirectToPage("/Index") : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Contrasena))
        {
            Error = "Escriba el correo y la contraseña.";
            return Page();
        }

        var (sesion, error) = await _api.IniciarSesionAsync(Email.Trim(), Contrasena);
        if (sesion is null)
        {
            Error = error;
            return Page();
        }

        // Cookie de sesión: nombre y roles como claims, y el JWT guardado como token.
        var claims = new List<Claim> { new(ClaimTypes.Name, sesion.Nombre) };
        claims.AddRange(sesion.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var propiedades = new AuthenticationProperties { IsPersistent = false };
        propiedades.StoreTokens(new[]
        {
            new AuthenticationToken { Name = "access_token", Value = sesion.AccessToken }
        });

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad),
            propiedades);

        return RedirectToPage("/Index");
    }
}
