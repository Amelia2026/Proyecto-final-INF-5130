using Hotel.Domain.Enums;
using Hotel.Domain.Excepciones;
using Hotel.Domain.Tarifas;

namespace Hotel.Domain.Entidades;

public class TipoHabitacion
{
    public const decimal TarifaMinima = 0.01m;
    public const decimal TarifaMaxima = 99_999.99m;

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public decimal TarifaBase { get; private set; }
    public TipoEstrategia Estrategia { get; private set; }

    private TipoHabitacion() { } // EF Core

    public TipoHabitacion(string nombre, decimal tarifaBase, TipoEstrategia estrategia)
        => Actualizar(nombre, tarifaBase, estrategia);

    public void Actualizar(string nombre, decimal tarifaBase, TipoEstrategia estrategia)
    {
        nombre = (nombre ?? string.Empty).Trim();
        if (nombre.Length == 0)
            throw new ValidacionDominioException("El nombre del tipo es obligatorio.");
        if (tarifaBase < TarifaMinima || tarifaBase > TarifaMaxima)
            throw new ValidacionDominioException($"La tarifa base debe estar entre {TarifaMinima} y {TarifaMaxima}.");
        if (decimal.Round(tarifaBase, 2) != tarifaBase)
            throw new ValidacionDominioException("La tarifa base admite como máximo dos decimales.");
        if (!Enum.IsDefined(estrategia))
            throw new ValidacionDominioException("Estrategia de tarifa fuera del catálogo.");

        Nombre = nombre;
        TarifaBase = tarifaBase;
        Estrategia = estrategia;
    }

    /// <summary>Delegamos el cálculo a la estrategia (polimorfismo): este método no tiene if por tipo de tarifa.</summary>
    public decimal CalcularImporte(Periodo periodo)
        => EstrategiaTarifaFactory.Crear(Estrategia).CalcularImporte(TarifaBase, periodo);
}
