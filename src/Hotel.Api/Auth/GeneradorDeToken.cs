using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Hotel.Api.Auth;

/// <summary>Fabrica el JWT: un texto firmado que dice quién eres y qué roles tienes.</summary>
public class GeneradorDeToken
{
    private readonly JwtOpciones _opciones;
    public GeneradorDeToken(JwtOpciones opciones) => _opciones = opciones;

    public string Generar(IdentityUser usuario, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new("sub", usuario.Id),
            new("name", usuario.UserName ?? usuario.Email ?? string.Empty),
            new("email", usuario.Email ?? string.Empty)
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Clave));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _opciones.Emisor,
            Audience = _opciones.Audiencia,
            Expires = DateTime.UtcNow.AddMinutes(_opciones.MinutosExpiracion),
            SigningCredentials = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
