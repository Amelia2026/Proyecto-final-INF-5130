using Hotel.Domain.Entidades;

namespace Hotel.Domain.Tarifas;

/// <summary>Precio base cada noche.</summary>
public class EstrategiaPlana : IEstrategiaTarifa
{
    public decimal CalcularImporte(decimal tarifaBase, Periodo periodo)
        => tarifaBase * periodo.NumeroNoches;
}
