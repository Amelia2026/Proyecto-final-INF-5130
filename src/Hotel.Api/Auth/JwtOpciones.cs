namespace Hotel.Api.Auth;

/// <summary>Se llena con la sección "Jwt" de appsettings.json.</summary>
public class JwtOpciones
{
    public string Emisor { get; set; } = string.Empty;
    public string Audiencia { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public int MinutosExpiracion { get; set; } = 120;
}
