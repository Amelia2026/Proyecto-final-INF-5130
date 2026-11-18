using Hotel.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _usuarios;
    private readonly GeneradorDeToken _tokens;

    public AuthController(UserManager<IdentityUser> usuarios, GeneradorDeToken tokens)
    {
        _usuarios = usuarios;
        _tokens = tokens;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<Sesion>> Login(LoginInput input)
    {
        var usuario = await _usuarios.FindByEmailAsync(input.Email ?? string.Empty);
        if (usuario is null || !await _usuarios.CheckPasswordAsync(usuario, input.Contrasena ?? string.Empty))
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Credenciales inválidas",
                detail: "El correo o la contraseña son incorrectos.");

        var roles = await _usuarios.GetRolesAsync(usuario);
        var token = _tokens.Generar(usuario, roles);
        return Ok(new Sesion(token, usuario.UserName ?? usuario.Email ?? string.Empty, roles.ToList()));
    }
}
