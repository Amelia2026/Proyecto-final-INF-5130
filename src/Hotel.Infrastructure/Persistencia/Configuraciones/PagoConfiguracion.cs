using Hotel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistencia.Configuraciones;

public class PagoConfiguracion : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> b)
    {
        b.ToTable("Pagos");
        b.HasKey(p => p.Id);
        b.Property(p => p.Monto).HasPrecision(12, 2);
        b.Property(p => p.Metodo).HasConversion<string>().HasMaxLength(15).IsUnicode(false).IsRequired();
        b.Property(p => p.Fecha).HasColumnType("date");
    }
}
