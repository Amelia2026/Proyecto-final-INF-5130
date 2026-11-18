using Hotel.Application.Abstracciones;
using Hotel.Application.Tablero;
using Hotel.Domain.Enums;
using Hotel.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Repositorios;

public class ResumenConsulta : IResumenConsulta
{
    private readonly HotelDbContext _db;
    public ResumenConsulta(HotelDbContext db) => _db = db;

    public async Task<ResumenDto> ObtenerAsync(DateOnly hoy, CancellationToken ct = default)
    {
        var habitacionesActivas = await _db.Habitaciones.CountAsync(h => h.Activa, ct);
        var clientes = await _db.Clientes.CountAsync(ct);
        // Vigente = sigue Reservada y su salida es posterior a hoy.
        var vigentes = await _db.Reservas.CountAsync(r => r.Estado == EstadoReserva.Reservada && r.Salida > hoy, ct);
        var enEstadia = await _db.Reservas.CountAsync(r => r.Estado == EstadoReserva.EnEstadia, ct);
        var finalizadas = await _db.Reservas.CountAsync(r => r.Estado == EstadoReserva.Finalizada, ct);
        var total = await _db.Pagos.SumAsync(p => (decimal?)p.Monto, ct) ?? 0m;

        return new ResumenDto(habitacionesActivas, clientes, vigentes, enEstadia, finalizadas, total);
    }
}
