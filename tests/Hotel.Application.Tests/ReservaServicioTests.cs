using Hotel.Application.Excepciones;
using Hotel.Application.Reservas;
using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;
using Hotel.Domain.Excepciones;
using Xunit;

namespace Hotel.Application.Tests;

public class ReservaServicioTests
{
    private static readonly DateOnly Hoy = new(2026, 11, 5);       // jueves
    private static readonly DateOnly Viernes = new(2026, 11, 6);
    private static readonly DateOnly Lunes = new(2026, 11, 9);

    /// <summary>Arma un "mini hotel" en memoria: una habitación 201 (Doble, FinDeSemana) y dos clientes.</summary>
    private class Escenario
    {
        public FakeReloj Reloj { get; } = new(Hoy);
        public FakeReservaRepositorio Reservas { get; } = new();
        public ReservaServicio Servicio { get; }
        public const int HabitacionId = 3, ClienteA = 1, ClienteB = 2;

        public Escenario()
        {
            var tipo = new TipoHabitacion("Doble", 3800.00m, TipoEstrategia.FinDeSemana).ConId(2);
            var habitaciones = new FakeHabitacionRepositorio();
            habitaciones.Datos.Add(new Habitacion(tipo, "201").ConId(HabitacionId));

            var clientes = new FakeClienteRepositorio();
            clientes.Datos.Add(new Cliente("Sofía Díaz", "sofia@correo.com", "001").ConId(ClienteA));
            clientes.Datos.Add(new Cliente("Luis Peña", "luis@correo.com", "002").ConId(ClienteB));

            Servicio = new ReservaServicio(Reservas, habitaciones, clientes, new FakeUnidadDeTrabajo(), Reloj);
        }

        public Task<ReservaDto> Reservar(int cliente, DateOnly entrada, DateOnly salida)
            => Servicio.ReservarAsync(new ReservaInput(HabitacionId, cliente, entrada, salida));
    }

    [Fact]
    public async Task Reservar_Valida_GuardaConImporteCalculado()
    {
        var e = new Escenario();

        var dto = await e.Reservar(Escenario.ClienteA, Viernes, Lunes);

        Assert.Equal(12920.00m, dto.Importe);
        Assert.Equal(EstadoReserva.Reservada, dto.Estado);
        Assert.Equal("201", dto.NumeroHabitacion);
        Assert.Single(e.Reservas.Datos);
    }

    [Fact]
    public async Task Reservar_PeriodoSolapado_LanzaHabitacionNoDisponible()
    {
        var e = new Escenario();
        await e.Reservar(Escenario.ClienteA, Viernes, Lunes);

        var ex = await Assert.ThrowsAsync<HabitacionNoDisponibleException>(() =>
            e.Reservar(Escenario.ClienteB, Viernes.AddDays(2), Lunes.AddDays(1)));

        Assert.Equal("HABITACION_NO_DISPONIBLE", ex.Codigo);
        Assert.Single(e.Reservas.Datos); // la segunda no se guardó
    }

    [Fact]
    public async Task Reservar_FechasContiguas_SePermite()
    {
        var e = new Escenario();
        await e.Reservar(Escenario.ClienteA, Viernes, Lunes);

        var dto = await e.Reservar(Escenario.ClienteB, Lunes, Lunes.AddDays(2)); // entra el día que el otro sale

        Assert.Equal(EstadoReserva.Reservada, dto.Estado);
        Assert.Equal(2, e.Reservas.Datos.Count);
    }

    [Fact]
    public async Task Reservar_ClienteInexistente_LanzaNoEncontrado()
    {
        var e = new Escenario();

        await Assert.ThrowsAsync<NoEncontradoException>(() => e.Reservar(cliente: 99, Viernes, Lunes));
    }

    [Fact]
    public async Task Cancelar_LiberaElPeriodoParaOtraReserva()
    {
        var e = new Escenario();
        var primera = await e.Reservar(Escenario.ClienteA, Viernes, Lunes);

        var cancelada = await e.Servicio.CancelarAsync(primera.Id);
        var nueva = await e.Reservar(Escenario.ClienteB, Viernes, Lunes); // mismo período, ahora libre

        Assert.Equal(EstadoReserva.Cancelada, cancelada.Estado);
        Assert.Equal(Hoy, cancelada.FechaCancelacion);
        Assert.Null(cancelada.Pago);
        Assert.Equal(EstadoReserva.Reservada, nueva.Estado);
    }

    [Fact]
    public async Task CheckIn_ConOtraEstadiaActiva_LanzaHabitacionNoDisponible()
    {
        var e = new Escenario();
        var a = await e.Reservar(Escenario.ClienteA, Viernes, Lunes);
        var b = await e.Reservar(Escenario.ClienteB, Lunes, Lunes.AddDays(2));
        e.Reloj.Fecha = Viernes;
        await e.Servicio.RegistrarCheckInAsync(a.Id); // A ya está hospedada

        e.Reloj.Fecha = Lunes;
        await Assert.ThrowsAsync<HabitacionNoDisponibleException>(() =>
            e.Servicio.RegistrarCheckInAsync(b.Id)); // A todavía no hizo check-out
    }

    [Fact]
    public async Task CheckOut_CreaUnSoloPagoPorElImportePactado()
    {
        var e = new Escenario();
        var r = await e.Reservar(Escenario.ClienteA, Viernes, Lunes);
        e.Reloj.Fecha = Viernes;
        await e.Servicio.RegistrarCheckInAsync(r.Id);

        e.Reloj.Fecha = Lunes;
        var final = await e.Servicio.RegistrarCheckOutAsync(r.Id, new CheckOutInput(MetodoPago.Tarjeta));

        Assert.Equal(EstadoReserva.Finalizada, final.Estado);
        Assert.NotNull(final.Pago);
        Assert.Equal(12920.00m, final.Pago!.Monto);
        Assert.Equal(MetodoPago.Tarjeta, final.Pago.Metodo);

        // Repetir el check-out se rechaza y no crea otro pago.
        await Assert.ThrowsAsync<TransicionInvalidaException>(() =>
            e.Servicio.RegistrarCheckOutAsync(r.Id, new CheckOutInput(MetodoPago.Efectivo)));
    }
}
