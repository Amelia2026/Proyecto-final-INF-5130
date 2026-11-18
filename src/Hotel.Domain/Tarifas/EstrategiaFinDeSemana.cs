using Hotel.Domain.Entidades;

namespace Hotel.Domain.Tarifas;

/// <summary>+20 % las noches de viernes y sábado. Se redondea por noche a 2 decimales, alejando el punto medio de cero.</summary>
public class EstrategiaFinDeSemana : IEstrategiaTarifa
{
    private const decimal Recargo = 1.20m;

    public decimal CalcularImporte(decimal tarifaBase, Periodo periodo)
    {
        decimal total = 0m;
        foreach (var noche in periodo.Noches())
        {
            var esFinDeSemana = noche.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;
            total += esFinDeSemana
                ? Math.Round(tarifaBase * Recargo, 2, MidpointRounding.AwayFromZero)
                : tarifaBase;
        }
        return total;
    }
}
