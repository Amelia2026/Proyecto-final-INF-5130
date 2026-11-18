using Hotel.Domain.Excepciones;

namespace Hotel.Domain.Entidades;

public class Habitacion
{
    public int Id { get; private set; }
    public int TipoHabitacionId { get; private set; }
    public TipoHabitacion? TipoHabitacion { get; private set; }
    public string Numero { get; private set; } = string.Empty;
    public bool Activa { get; private set; }

    private Habitacion() { } // EF Core

    public Habitacion(TipoHabitacion tipo, string numero, bool activa = true)
        => Actualizar(tipo, numero, activa);

    public void Actualizar(TipoHabitacion tipo, string numero, bool activa)
    {
        ArgumentNullException.ThrowIfNull(tipo);
        numero = (numero ?? string.Empty).Trim();
        if (numero.Length == 0)
            throw new ValidacionDominioException("El número de habitación es obligatorio.");

        TipoHabitacion = tipo;
        TipoHabitacionId = tipo.Id;
        Numero = numero;
        Activa = activa;
    }
}
