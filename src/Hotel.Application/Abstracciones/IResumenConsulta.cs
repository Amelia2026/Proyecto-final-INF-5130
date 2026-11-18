using Hotel.Application.Tablero;

namespace Hotel.Application.Abstracciones;

/// <summary>Consulta de solo lectura para el tablero de Inicio (conteos y total cobrado).</summary>
public interface IResumenConsulta
{
    Task<ResumenDto> ObtenerAsync(DateOnly hoy, CancellationToken ct = default);
}
