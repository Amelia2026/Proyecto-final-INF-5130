using Hotel.Domain.Enums;
using Hotel.Domain.Excepciones;

namespace Hotel.Domain.Tarifas;

/// <summary>Convierte el valor guardado en BD (TipoEstrategia) en el objeto que sabe calcular.</summary>
public static class EstrategiaTarifaFactory
{
    public static IEstrategiaTarifa Crear(TipoEstrategia tipo) => tipo switch
    {
        TipoEstrategia.Plana => new EstrategiaPlana(),
        TipoEstrategia.FinDeSemana => new EstrategiaFinDeSemana(),
        _ => throw new ValidacionDominioException("Estrategia de tarifa fuera del catálogo.")
    };
}
