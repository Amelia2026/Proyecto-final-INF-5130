using Hotel.Application.Abstracciones;
using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;
using Hotel.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Repositorios;

public class HabitacionRepositorio : IHabitacionRepositorio
{
    private readonly HotelDbContext _db;
    public HabitacionRepositorio(HotelDbContext db) => _db = db;

    public async Task<Habitacion?> ObtenerConBloqueoAsync(int id, CancellationToken ct = default)
    {
        // UPDLOCK + HOLDLOCK: nadie más puede tocar esta fila hasta que termine la transacción.
        // Debe ejecutarse DENTRO de una transacción (IUnidadDeTrabajo se encarga).
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT Id FROM dbo.Habitaciones WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}", ct);

        return await _db.Habitaciones
            .Include(h => h.TipoHabitacion)
            .FirstOrDefaultAsync(h => h.Id == id, ct);
    }

    public Task<Habitacion?> ObtenerAsync(int id, CancellationToken ct = default)
        => _db.Habitaciones.Include(h => h.TipoHabitacion).FirstOrDefaultAsync(h => h.Id == id, ct);

    public async Task<IReadOnlyList<Habitacion>> ListarAsync(CancellationToken ct = default)
        => await _db.Habitaciones.AsNoTracking().OrderBy(h => h.Id).ToListAsync(ct);

    public Task<bool> ExisteNumeroAsync(string numero, int? excluirId, CancellationToken ct = default)
        => _db.Habitaciones.AnyAsync(h => h.Numero == numero && (excluirId == null || h.Id != excluirId.Value), ct);

    public Task<bool> TieneReservasAsync(int id, CancellationToken ct = default)
        => _db.Reservas.AnyAsync(r => r.HabitacionId == id, ct);

    public Task<bool> TieneReservasActivasAsync(int id, CancellationToken ct = default)
        => _db.Reservas.AnyAsync(r => r.HabitacionId == id
            && (r.Estado == EstadoReserva.Reservada || r.Estado == EstadoReserva.EnEstadia), ct);

    public void Agregar(Habitacion habitacion) => _db.Habitaciones.Add(habitacion);
    public void Eliminar(Habitacion habitacion) => _db.Habitaciones.Remove(habitacion);
}
