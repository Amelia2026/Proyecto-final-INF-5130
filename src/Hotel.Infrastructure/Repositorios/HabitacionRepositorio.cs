using Hotel.Application.Abstracciones;
using Hotel.Domain.Entidades;
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
}
