using Hotel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Persistencia;

/// <summary>La "puerta" de EF Core a la base de datos: una propiedad por tabla.</summary>
public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<TipoHabitacion> TiposHabitacion => Set<TipoHabitacion>();
    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
}
