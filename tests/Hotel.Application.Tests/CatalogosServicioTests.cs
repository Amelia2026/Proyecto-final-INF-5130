using Hotel.Application.Catalogos;
using Hotel.Application.Excepciones;
using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;
using Xunit;

namespace Hotel.Application.Tests;

public class CatalogosServicioTests
{
    private static readonly DateOnly Hoy = new(2026, 11, 5);

    [Fact]
    public async Task CrearCliente_DocumentoDuplicadoSinImportarMayusculas_LanzaDuplicado()
    {
        var repo = new FakeClienteRepositorio();
        var servicio = new ClienteServicio(repo, new FakeUnidadDeTrabajo());
        await servicio.CrearAsync(new ClienteInput("Ana", "ana@correo.com", "ABC-1"));

        var ex = await Assert.ThrowsAsync<DuplicadoException>(() =>
            servicio.CrearAsync(new ClienteInput("Ana B", "otra@correo.com", "abc-1")));

        Assert.Equal("DUPLICADO", ex.Codigo);
        Assert.Single(repo.Datos);
    }

    [Fact]
    public async Task EliminarTipo_ConHabitacionesAsociadas_LanzaRecursoEnUso()
    {
        var tipos = new FakeTipoHabitacionRepositorio();
        var habitaciones = new FakeHabitacionRepositorio();
        tipos.Habitaciones = habitaciones;
        var servicio = new TipoHabitacionServicio(tipos, new FakeUnidadDeTrabajo());
        var tipo = await servicio.CrearAsync(new TipoInput("Doble", 3800.00m, TipoEstrategia.FinDeSemana));
        habitaciones.Datos.Add(new Habitacion(tipos.Datos[0], "201").ConId(1));

        await Assert.ThrowsAsync<RecursoEnUsoException>(() => servicio.EliminarAsync(tipo.Id));
        Assert.Single(tipos.Datos); // no se borró
    }

    [Fact]
    public async Task ActualizarHabitacion_CambiarTipoConReservaVigente_LanzaRecursoEnUso()
    {
        var sencilla = new TipoHabitacion("Sencilla", 2500.00m, TipoEstrategia.Plana).ConId(1);
        var doble = new TipoHabitacion("Doble", 3800.00m, TipoEstrategia.FinDeSemana).ConId(2);
        var tipos = new FakeTipoHabitacionRepositorio();
        tipos.Datos.Add(sencilla);
        tipos.Datos.Add(doble);

        var reservas = new FakeReservaRepositorio();
        var habitaciones = new FakeHabitacionRepositorio { Reservas = reservas };
        var habitacion = new Habitacion(sencilla, "101").ConId(10);
        habitaciones.Datos.Add(habitacion);

        var cliente = new Cliente("Ana", "ana@correo.com", "001").ConId(1);
        reservas.Agregar(Reserva.Crear(habitacion, cliente, Hoy.AddDays(1), Hoy.AddDays(3), Hoy));

        var servicio = new HabitacionServicio(habitaciones, tipos, new FakeUnidadDeTrabajo());

        await Assert.ThrowsAsync<RecursoEnUsoException>(() =>
            servicio.ActualizarAsync(10, new HabitacionInput(2, "101", true)));
    }
}
