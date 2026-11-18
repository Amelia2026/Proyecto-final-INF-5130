using Hotel.Application.Abstracciones;
using Hotel.Domain.Entidades;
using Hotel.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Repositorios;

public class TipoHabitacionRepositorio : ITipoHabitacionRepositorio
{
    private readonly HotelDbContext _db;
    public TipoHabitacionRepositorio(HotelDbContext db) => _db = db;

    public Task<TipoHabitacion?> ObtenerAsync(int id, CancellationToken ct = default)
        => _db.TiposHabitacion.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<TipoHabitacion>> ListarAsync(CancellationToken ct = default)
        => await _db.TiposHabitacion.AsNoTracking().OrderBy(t => t.Id).ToListAsync(ct);

    // La columna Nombre es "CI" (no distingue mayúsculas), así que la comparación lo respeta.
    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId, CancellationToken ct = default)
        => _db.TiposHabitacion.AnyAsync(t => t.Nombre == nombre && (excluirId == null || t.Id != excluirId.Value), ct);

    public Task<bool> TieneHabitacionesAsync(int id, CancellationToken ct = default)
        => _db.Habitaciones.AnyAsync(h => h.TipoHabitacionId == id, ct);

    public void Agregar(TipoHabitacion tipo) => _db.TiposHabitacion.Add(tipo);
    public void Eliminar(TipoHabitacion tipo) => _db.TiposHabitacion.Remove(tipo);
}
