using Hotel.Application.Abstracciones;
using Hotel.Application.Excepciones;
using Hotel.Infrastructure.Persistencia;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Repositorios;

public class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly HotelDbContext _db;
    public UnidadDeTrabajo(HotelDbContext db) => _db = db;

    public async Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion, CancellationToken ct = default)
    {
        await using var transaccion = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var resultado = await operacion();
            await _db.SaveChangesAsync(ct); // todos los cambios juntos (ej. reserva + pago)
            await transaccion.CommitAsync(ct);
            return resultado;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 or 547 })
        {
            // 2601 / 2627 = violación de unicidad (dos estadías activas, dos pagos, nombre repetido).
            // 547 = violación de llave foránea (borrar algo que otra tabla todavía usa).
            throw new ConflictoDePersistenciaException(
                "La operación chocó con otra o con datos existentes. Intente de nuevo.");
        }
        // Cualquier otra excepción: al salir del 'await using' la transacción se deshace sola (rollback).
    }
}
