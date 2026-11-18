using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;
using Hotel.Domain.Excepciones;

namespace Hotel.Domain.Tests;

public class CatalogosTests
{
    [Theory]
    [InlineData(0.00)]
    [InlineData(100000.00)]
    [InlineData(-5)]
    public void TipoHabitacion_TarifaFueraDeRango_LanzaValidacion(double tarifa)
    {
        Assert.Throws<ValidacionDominioException>(() =>
            new TipoHabitacion("Estándar", (decimal)tarifa, TipoEstrategia.Plana));
    }

    [Fact]
    public void TipoHabitacion_MasDeDosDecimales_LanzaValidacion()
    {
        Assert.Throws<ValidacionDominioException>(() =>
            new TipoHabitacion("Estándar", 100.123m, TipoEstrategia.Plana));
    }

    [Fact]
    public void Cliente_EmailInvalido_LanzaValidacion()
    {
        Assert.Throws<ValidacionDominioException>(() =>
            new Cliente("Ana", "esto-no-es-un-correo", "001"));
    }

    [Fact]
    public void Cliente_RecortaEspaciosEnLosCampos()
    {
        var cliente = new Cliente("  Ana  ", "  ana@correo.com ", " 001 ");

        Assert.Equal("Ana", cliente.Nombre);
        Assert.Equal("ana@correo.com", cliente.Email);
        Assert.Equal("001", cliente.Documento);
    }
}
