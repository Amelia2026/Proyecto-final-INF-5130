using Hotel.Domain.Entidades;

namespace Hotel.Application.Abstracciones;

public interface ITipoHabitacionRepositorio
{
    Task<TipoHabitacion?> ObtenerAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<TipoHabitacion>> ListarAsync(CancellationToken ct = default);
    /// <summary>¿Existe otro tipo con ese nombre (sin distinguir mayúsculas)? excluirId = el que se está editando.</summary>
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId, CancellationToken ct = default);
    Task<bool> TieneHabitacionesAsync(int id, CancellationToken ct = default);
    void Agregar(TipoHabitacion tipo);
    void Eliminar(TipoHabitacion tipo);
}
