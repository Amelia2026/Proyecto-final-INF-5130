using Hotel.Domain.Entidades;

namespace Hotel.Application.Abstracciones;

public interface IReservaRepositorio
{
    Task<Reserva?> ObtenerAsync(int id, CancellationToken ct = default);

    /// <summary>Trae la reserva bloqueándola hasta terminar la transacción (evita dos check-out a la vez).</summary>
    Task<Reserva?> ObtenerConBloqueoAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<Reserva>> ListarAsync(CancellationToken ct = default);

    /// <summary>Reservas Reservada o EnEstadia de una habitación: las únicas que pueden estorbar a otra.</summary>
    Task<IReadOnlyList<Reserva>> ListarActivasDeHabitacionAsync(int habitacionId, CancellationToken ct = default);

    void Agregar(Reserva reserva);
}
