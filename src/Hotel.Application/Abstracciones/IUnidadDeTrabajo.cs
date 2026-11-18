namespace Hotel.Application.Abstracciones;

/// <summary>
/// Agrupa varios cambios en una sola transacción: o se guardan todos o no se guarda ninguno.
/// Así el check-out y su pago nunca quedan a medias.
/// </summary>
public interface IUnidadDeTrabajo
{
    Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion, CancellationToken ct = default);
}
