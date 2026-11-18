using Hotel.Domain.Enums;
using Hotel.Infrastructure.Persistencia;
using Hotel.Infrastructure.Repositorios;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hotel.Infrastructure.Tests;

/// <summary>
/// Pruebas de humo (SOLO LECTURA) contra la base INF5130_TF02 local con los datos de ejemplo del profe.
/// Comprueban que EF Core entiende las tablas: textos -> enums, fechas, relaciones.
/// </summary>
public class MapeoTests
{
    private const string Conexion =
        "Server=localhost;Database=INF5130_TF02;Trusted_Connection=True;TrustServerCertificate=True";

    private static HotelDbContext CrearContexto()
        => new(new DbContextOptionsBuilder<HotelDbContext>().UseSqlServer(Conexion).Options);

    [Fact]
    public async Task ListarReservas_LeeEstadosComoEnumYElPagoDeLaFinalizada()
    {
        await using var db = CrearContexto();
        var repo = new ReservaRepositorio(db);

        var reservas = await repo.ListarAsync();

        Assert.True(reservas.Count >= 3);
        var finalizada = reservas.First(r => r.Estado == EstadoReserva.Finalizada);
        Assert.NotNull(finalizada.Pago);
        Assert.Equal(finalizada.Importe, finalizada.Pago!.Monto);
        Assert.False(string.IsNullOrEmpty(finalizada.Habitacion?.Numero));
    }

    [Fact]
    public async Task TiposHabitacion_LeeLaEstrategiaDesdeTexto()
    {
        await using var db = CrearContexto();

        var tipos = await db.TiposHabitacion.AsNoTracking().ToListAsync();

        var doble = tipos.Single(t => t.Nombre == "Doble");
        Assert.Equal(TipoEstrategia.FinDeSemana, doble.Estrategia);
        Assert.Equal(3800.00m, doble.TarifaBase);
    }
}
