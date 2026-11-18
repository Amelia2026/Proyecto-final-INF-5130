using Hotel.Domain.Entidades;

namespace Hotel.Application.Abstracciones;

public interface IHabitacionRepositorio
{
    /// <summary>
    /// Trae la habitación (con su tipo) y la "bloquea" hasta terminar la transacción:
    /// si otra persona intenta reservar la misma habitación al mismo tiempo, espera su turno.
    /// </summary>
    Task<Habitacion?> ObtenerConBloqueoAsync(int id, CancellationToken ct = default);
}
