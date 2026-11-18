using Hotel.Domain.Excepciones;

namespace Hotel.Application.Excepciones;

/// <summary>
/// Dos operaciones chocaron al guardar (la base de datos rechazó la segunda). La API la traduce a HTTP 409.
/// </summary>
public class ConflictoDePersistenciaException : ReglaNegocioException
{
    public ConflictoDePersistenciaException(string mensaje)
        : base("CONFLICTO_CONCURRENCIA", mensaje) { }
}
