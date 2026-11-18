using Hotel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistencia.Configuraciones;

public class ReservaConfiguracion : IEntityTypeConfiguration<Reserva>
{
    public void Configure(EntityTypeBuilder<Reserva> b)
    {
        b.ToTable("Reservas");
        b.HasKey(r => r.Id);
        b.Property(r => r.Entrada).HasColumnType("date");
        b.Property(r => r.Salida).HasColumnType("date");
        b.Property(r => r.FechaCancelacion).HasColumnType("date");
        b.Property(r => r.FechaCheckIn).HasColumnType("date");
        b.Property(r => r.FechaCheckOut).HasColumnType("date");
        b.Property(r => r.Importe).HasPrecision(12, 2);
        // La base guarda 'Reservada', 'EnEstadia', etc. como texto.
        b.Property(r => r.Estado).HasConversion<string>().HasMaxLength(12).IsUnicode(false).IsRequired();

        b.HasOne(r => r.Habitacion).WithMany().HasForeignKey(r => r.HabitacionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(r => r.Cliente).WithMany().HasForeignKey(r => r.ClienteId).OnDelete(DeleteBehavior.Restrict);

        // Una reserva tiene como máximo un pago.
        b.HasOne(r => r.Pago).WithOne().HasForeignKey<Pago>(p => p.ReservaId);

        // Red de seguridad de la base de datos: una sola estadía activa por habitación.
        b.HasIndex(r => r.HabitacionId)
            .IsUnique()
            .HasFilter("[Estado] = 'EnEstadia'")
            .HasDatabaseName("UX_Reservas_EstadiaActiva");
        b.HasIndex(r => r.ClienteId).HasDatabaseName("IX_Reservas_ClienteId");
    }
}
