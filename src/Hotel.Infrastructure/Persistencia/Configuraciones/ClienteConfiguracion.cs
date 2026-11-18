using Hotel.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hotel.Infrastructure.Persistencia.Configuraciones;

public class ClienteConfiguracion : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("Clientes");
        b.HasKey(c => c.Id);
        b.Property(c => c.Nombre).HasMaxLength(120).IsRequired();
        b.Property(c => c.Email).HasMaxLength(254).UseCollation("Latin1_General_100_CI_AS_SC").IsRequired();
        b.Property(c => c.Documento).HasMaxLength(32).UseCollation("Latin1_General_100_CI_AS_SC").IsRequired();
        b.HasIndex(c => c.Email).IsUnique().HasDatabaseName("UQ_Clientes_Email");
        b.HasIndex(c => c.Documento).IsUnique().HasDatabaseName("UQ_Clientes_Documento");
    }
}
