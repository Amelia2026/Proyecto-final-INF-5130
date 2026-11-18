using Hotel.Application.Abstracciones;

namespace Hotel.Application.Tablero;

/// <summary>Lo que muestra la pantalla de Inicio (Resumen del contrato REST).</summary>
public record ResumenDto(
    int HabitacionesActivas,
    int Clientes,
    int ReservasVigentes,
    int EnEstadia,
    int Finalizadas,
    decimal TotalCobrado);

public class TableroServicio
{
    private readonly IResumenConsulta _consulta;
    private readonly IReloj _reloj;

    public TableroServicio(IResumenConsulta consulta, IReloj reloj)
    {
        _consulta = consulta;
        _reloj = reloj;
    }

    public Task<ResumenDto> ObtenerAsync(CancellationToken ct = default)
        => _consulta.ObtenerAsync(_reloj.Hoy(), ct);
}
