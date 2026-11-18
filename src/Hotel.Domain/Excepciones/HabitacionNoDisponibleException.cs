namespace Hotel.Domain.Excepciones;

/// <summary>Habitación fuera de servicio, ocupada o con período solapado. La API la traduce a HTTP 409.</summary>
public class HabitacionNoDisponibleException : ReglaNegocioException
{
    public HabitacionNoDisponibleException(string mensaje) : base("HABITACION_NO_DISPONIBLE", mensaje) { }
}
