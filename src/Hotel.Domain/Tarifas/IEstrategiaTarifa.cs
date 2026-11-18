using Hotel.Domain.Entidades;

namespace Hotel.Domain.Tarifas;

/// <summary>Patrón Strategy: cada tipo de habitación calcula su importe con una estrategia intercambiable.</summary>
public interface IEstrategiaTarifa
{
    decimal CalcularImporte(decimal tarifaBase, Periodo periodo);
}
