using Hotel.Domain.Excepciones;

namespace Hotel.Domain.Entidades;

/// <summary>
/// Objeto de valor: rango de noches [Entrada, Salida). La entrada se incluye y la salida se excluye,
/// por eso una reserva que termina el día en que otra empieza NO se solapa (son contiguas).
/// </summary>
public readonly record struct Periodo
{
    public const int MaximoNoches = 30;

    public DateOnly Entrada { get; }
    public DateOnly Salida { get; }

    public Periodo(DateOnly entrada, DateOnly salida)
    {
        if (salida <= entrada)
            throw new ValidacionDominioException("La fecha de salida debe ser posterior a la de entrada.");

        var noches = salida.DayNumber - entrada.DayNumber;
        if (noches > MaximoNoches)
            throw new ValidacionDominioException($"La estadía no puede exceder {MaximoNoches} noches.");

        Entrada = entrada;
        Salida = salida;
    }

    public int NumeroNoches => Salida.DayNumber - Entrada.DayNumber;

    /// <summary>Dos rangos [a,b) y [c,d) se solapan si a &lt; d y b &gt; c.</summary>
    public bool SolapaCon(Periodo otro) => Entrada < otro.Salida && Salida > otro.Entrada;

    /// <summary>Cada noche del período: la fecha en que el huésped duerme en la habitación.</summary>
    public IEnumerable<DateOnly> Noches()
    {
        for (var dia = Entrada; dia < Salida; dia = dia.AddDays(1))
            yield return dia;
    }
}
