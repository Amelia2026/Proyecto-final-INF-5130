using Hotel.Domain.Entidades;

namespace Hotel.Application.Abstracciones;

public interface IHabitacionRepositorio
{
    /// <summary>
    /// Trae la habitación (con su tipo) y la "bloquea" hasta terminar la transacción:
    /// si otra persona intenta reservar la misma habitación al mismo tiempo, espera su turno.
    /// </summary>
    Task<Habitacion?> ObtenerConBloqueoAsync(int id, CancellationToken ct = default);

    Task<Habitacion?> ObtenerAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Habitacion>> ListarAsync(CancellationToken ct = default);
    Task<bool> ExisteNumeroAsync(string numero, int? excluirId, CancellationToken ct = default);
    /// <summary>¿Tiene alguna reserva, de cualquier estado? (historial)</summary>
    Task<bool> TieneReservasAsync(int id, CancellationToken ct = default);
    /// <summary>¿Tiene una reserva vigente (Reservada) o una estadía activa (EnEstadia)?</summary>
    Task<bool> TieneReservasActivasAsync(int id, CancellationToken ct = default);
    void Agregar(Habitacion habitacion);
    void Eliminar(Habitacion habitacion);
}
