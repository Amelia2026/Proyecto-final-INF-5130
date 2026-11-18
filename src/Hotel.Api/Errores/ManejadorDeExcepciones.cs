using Hotel.Application.Excepciones;
using Hotel.Domain.Excepciones;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Errores;

/// <summary>
/// Atrapa las excepciones del negocio y las convierte en respuestas HTTP (application/problem+json):
/// Validación -> 400, No encontrado -> 404, cualquier otra regla (ocupada, transición inválida, choque) -> 409.
/// El "codigo" estable (ej. HABITACION_NO_DISPONIBLE) viaja en la respuesta.
/// </summary>
public class ManejadorDeExcepciones : IExceptionHandler
{
    private readonly IProblemDetailsService _problemas;
    public ManejadorDeExcepciones(IProblemDetailsService problemas) => _problemas = problemas;

    public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion, CancellationToken ct)
    {
        if (excepcion is not ReglaNegocioException regla)
            return false; // errores inesperados: que los trate ASP.NET (HTTP 500)

        var (estado, titulo) = regla switch
        {
            ValidacionDominioException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            NoEncontradoException => (StatusCodes.Status404NotFound, "No encontrado"),
            _ => (StatusCodes.Status409Conflict, "Conflicto")
        };

        contexto.Response.StatusCode = estado;
        var detalle = new ProblemDetails { Status = estado, Title = titulo, Detail = regla.Message };
        detalle.Extensions["codigo"] = regla.Codigo;

        return await _problemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto,
            ProblemDetails = detalle,
            Exception = excepcion
        });
    }
}
