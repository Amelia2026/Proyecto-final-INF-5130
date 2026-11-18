using Hotel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistencia.Configuraciones;

public class TipoHabitacionConfiguracion : IEntityTypeConfiguration<TipoHabitacion>
{
    public void Configure(EntityTypeBuilder<TipoHabitacion> b)
    {
        b.ToTable("TiposHabitacion");
        b.HasKey(t => t.Id);
        b.Property(t => t.Nombre).HasMaxLength(100).UseCollation("Latin1_General_100_CI_AS_SC").IsRequired();
        b.Property(t => t.TarifaBase).HasPrecision(10, 2);
        // La base guarda el NOMBRE ('Plana', 'FinDeSemana'), no el número del enum.
        b.Property(t => t.Estrategia).HasConversion<string>().HasMaxLength(12).IsUnicode(false).IsRequired();
        b.HasIndex(t => t.Nombre).IsUnique().HasDatabaseName("UQ_TiposHabitacion_Nombre");
    }
}
