using Hotel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistencia.Configuraciones;

public class HabitacionConfiguracion : IEntityTypeConfiguration<Habitacion>
{
    public void Configure(EntityTypeBuilder<Habitacion> b)
    {
        b.ToTable("Habitaciones");
        b.HasKey(h => h.Id);
        b.Property(h => h.Numero).HasMaxLength(12).UseCollation("Latin1_General_100_CI_AS_SC").IsRequired();
        b.Property(h => h.Activa).IsRequired();
        b.HasIndex(h => h.Numero).IsUnique().HasDatabaseName("UQ_Habitaciones_Numero");

        b.HasOne(h => h.TipoHabitacion)
            .WithMany()
            .HasForeignKey(h => h.TipoHabitacionId)
            .OnDelete(DeleteBehavior.Restrict); // no se puede borrar un tipo que tiene habitaciones
    }
}
