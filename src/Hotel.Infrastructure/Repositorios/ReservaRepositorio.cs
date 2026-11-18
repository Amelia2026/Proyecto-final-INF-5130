using Hotel.Application.Abstracciones;
using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;
using Hotel.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Repositorios;

public class ReservaRepositorio : IReservaRepositorio
{
    private readonly HotelDbContext _db;
    public ReservaRepositorio(HotelDbContext db) => _db = db;

    private IQueryable<Reserva> ConDetalle => _db.Reservas
        .Include(r => r.Habitacion)
        .Include(r => r.Cliente)
        .Include(r => r.Pago);

    public Task<Reserva?> ObtenerAsync(int id, CancellationToken ct = default)
        => ConDetalle.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<Reserva?> ObtenerConBloqueoAsync(int id, CancellationToken ct = default)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT Id FROM dbo.Reservas WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}", ct);

        // Se lee DESPUÉS de bloquear, así el estado es el más reciente.
        return await ConDetalle.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<IReadOnlyList<Reserva>> ListarAsync(CancellationToken ct = default)
        => await ConDetalle.AsNoTracking().OrderByDescending(r => r.Id).ToListAsync(ct);

    public async Task<IReadOnlyList<Reserva>> ListarActivasDeHabitacionAsync(int habitacionId, CancellationToken ct = default)
        => await _db.Reservas
            .Where(r => r.HabitacionId == habitacionId
                     && (r.Estado == EstadoReserva.Reservada || r.Estado == EstadoReserva.EnEstadia))
            .ToListAsync(ct);

    public void Agregar(Reserva reserva) => _db.Reservas.Add(reserva);
}
