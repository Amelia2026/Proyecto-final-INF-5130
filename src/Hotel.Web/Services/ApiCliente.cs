using System.Net.Http.Json;

namespace Hotel.Web.Services;

/// <summary>Respuesta de POST /api/auth/login (Sesion del contrato).</summary>
public record SesionApi(string AccessToken, string Nombre, List<string> Roles);

/// <summary>Respuesta de GET /api/dashboard (Resumen del contrato).</summary>
public record ResumenApi(
    int HabitacionesActivas,
    int Clientes,
    int ReservasVigentes,
    int EnEstadia,
    int Finalizadas,
    decimal TotalCobrado);

/// <summary>Cuerpo de error de la API (application/problem+json).</summary>
public record ProblemaApi(string? Title, string? Detail, string? Codigo);

/// <summary>Resultado de una llamada a la API: los datos, o el mensaje de error y el código HTTP (0 = sin conexión).</summary>
public record RespuestaApi<T>(T? Datos, string? Error, int Estado)
{
    public bool Ok => Error is null;
}

/// <summary>Único punto por donde la Web habla con la API REST.</summary>
public class ApiCliente
{
    private const string MensajeSinConexion =
        "No se pudo conectar con la API. ¿Está iniciada en https://localhost:7001?";

    private readonly HttpClient _http;

    public ApiCliente(HttpClient http) => _http = http;

    public async Task<(SesionApi? Sesion, string? Error)> IniciarSesionAsync(
        string email, string contrasena, CancellationToken ct = default)
    {
        try
        {
            var respuesta = await _http.PostAsJsonAsync("api/auth/login", new { email, contrasena }, ct);
            if (respuesta.IsSuccessStatusCode)
            {
                var sesion = await respuesta.Content.ReadFromJsonAsync<SesionApi>(ct);
                return (sesion, sesion is null ? "La API devolvió una respuesta vacía." : null);
            }
            return (null, await LeerMensajeDeErrorAsync(respuesta, ct));
        }
        catch (HttpRequestException)
        {
            return (null, MensajeSinConexion);
        }
    }

    public Task<RespuestaApi<ResumenApi>> ObtenerResumenAsync(CancellationToken ct = default)
        => ObtenerAsync<ResumenApi>("api/dashboard", ct);

    /// <summary>GET genérico: devuelve los datos deserializados o el mensaje de error de la API.</summary>
    private async Task<RespuestaApi<T>> ObtenerAsync<T>(string ruta, CancellationToken ct)
    {
        try
        {
            var respuesta = await _http.GetAsync(ruta, ct);
            if (respuesta.IsSuccessStatusCode)
            {
                var datos = await respuesta.Content.ReadFromJsonAsync<T>(ct);
                return new RespuestaApi<T>(datos, null, (int)respuesta.StatusCode);
            }
            return new RespuestaApi<T>(default, await LeerMensajeDeErrorAsync(respuesta, ct), (int)respuesta.StatusCode);
        }
        catch (HttpRequestException)
        {
            return new RespuestaApi<T>(default, MensajeSinConexion, 0);
        }
    }

    /// <summary>Devuelve el "detail" que envía la API; si no hay, un mensaje genérico con el código HTTP.</summary>
    public static async Task<string> LeerMensajeDeErrorAsync(HttpResponseMessage respuesta, CancellationToken ct = default)
    {
        try
        {
            var problema = await respuesta.Content.ReadFromJsonAsync<ProblemaApi>(ct);
            if (!string.IsNullOrWhiteSpace(problema?.Detail)) return problema.Detail!;
        }
        catch (Exception)
        {
            // el cuerpo no era JSON: se usa el mensaje genérico
        }
        return $"La API respondió con el código {(int)respuesta.StatusCode}.";
    }
}
