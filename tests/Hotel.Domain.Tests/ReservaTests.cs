using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;
using Hotel.Domain.Excepciones;

namespace Hotel.Domain.Tests;

public class ReservaTests
{
    // 2026-11-06 es viernes; 2026-11-09 es lunes (ejemplo del contrato REST).
    private static readonly DateOnly Hoy = new(2026, 11, 5);
    private static readonly DateOnly Viernes = new(2026, 11, 6);
    private static readonly DateOnly Lunes = new(2026, 11, 9);

    private static Habitacion CrearHabitacion(TipoEstrategia estrategia, bool activa = true)
    {
        var tipo = new TipoHabitacion("Suite", 3800.00m, estrategia);
        return new Habitacion(tipo, "201", activa);
    }

    private static Cliente CrearCliente() => new("Sofía Díaz", "sofia@correo.com", "001-0000000-1");

    private static Reserva CrearReserva(TipoEstrategia estrategia = TipoEstrategia.FinDeSemana)
        => Reserva.Crear(CrearHabitacion(estrategia), CrearCliente(), Viernes, Lunes, Hoy);

    // ---- Tarifas ----

    [Fact]
    public void Crear_FinDeSemana_ViernesALunes_CalculaRD12920()
    {
        var reserva = CrearReserva(TipoEstrategia.FinDeSemana);

        Assert.Equal(12920.00m, reserva.Importe);
        Assert.Equal(EstadoReserva.Reservada, reserva.Estado);
    }

    [Fact]
    public void Crear_Plana_ViernesALunes_CalculaRD11400()
    {
        var reserva = CrearReserva(TipoEstrategia.Plana);

        Assert.Equal(11400.00m, reserva.Importe);
    }

    [Fact]
    public void CambiarTarifaDelTipo_NoAlteraElImporteDeUnaReservaYaCreada()
    {
        var habitacion = CrearHabitacion(TipoEstrategia.FinDeSemana);
        var reserva = Reserva.Crear(habitacion, CrearCliente(), Viernes, Lunes, Hoy);

        habitacion.TipoHabitacion!.Actualizar("Suite", 9000.00m, TipoEstrategia.Plana);

        Assert.Equal(12920.00m, reserva.Importe);
    }

    // ---- Validaciones al reservar ----

    [Fact]
    public void Crear_SalidaNoPosteriorAEntrada_LanzaValidacion()
    {
        var ex = Assert.Throws<ValidacionDominioException>(() =>
            Reserva.Crear(CrearHabitacion(TipoEstrategia.Plana), CrearCliente(), Viernes, Viernes, Hoy));

        Assert.Equal("VALIDACION", ex.Codigo);
    }

    [Fact]
    public void Crear_MasDe30Noches_LanzaValidacion()
    {
        Assert.Throws<ValidacionDominioException>(() =>
            Reserva.Crear(CrearHabitacion(TipoEstrategia.Plana), CrearCliente(), Viernes, Viernes.AddDays(31), Hoy));
    }

    [Fact]
    public void Crear_EntradaEnElPasado_LanzaValidacion()
    {
        Assert.Throws<ValidacionDominioException>(() =>
            Reserva.Crear(CrearHabitacion(TipoEstrategia.Plana), CrearCliente(), Hoy.AddDays(-1), Hoy.AddDays(1), Hoy));
    }

    [Fact]
    public void Crear_HabitacionFueraDeServicio_LanzaHabitacionNoDisponible()
    {
        var ex = Assert.Throws<HabitacionNoDisponibleException>(() =>
            Reserva.Crear(CrearHabitacion(TipoEstrategia.Plana, activa: false), CrearCliente(), Viernes, Lunes, Hoy));

        Assert.Equal("HABITACION_NO_DISPONIBLE", ex.Codigo);
    }

    // ---- Período y solapamiento ----

    [Fact]
    public void Periodo_FechasContiguas_NoSeSolapan()
    {
        var a = new Periodo(Viernes, Lunes);
        var b = new Periodo(Lunes, Lunes.AddDays(2)); // empieza el día que termina la otra

        Assert.False(a.SolapaCon(b));
        Assert.False(b.SolapaCon(a));
    }

    [Fact]
    public void Periodo_RangosQueSeCruzan_SeSolapan()
    {
        var a = new Periodo(Viernes, Lunes);
        var b = new Periodo(Viernes.AddDays(2), Lunes.AddDays(2));

        Assert.True(a.SolapaCon(b));
    }

    [Fact]
    public void BloqueaDisponibilidad_ReservaCancelada_NoBloquea()
    {
        var reserva = CrearReserva();
        reserva.Cancelar(Hoy);

        Assert.False(reserva.BloqueaDisponibilidad(new Periodo(Viernes, Lunes), Hoy));
    }

    [Fact]
    public void BloqueaDisponibilidad_EstadiaVigente_NoBloqueaUnPeriodoFuturoQueNoSeSolapa()
    {
        var reserva = CrearReserva(); // viernes -> lunes
        reserva.RegistrarCheckIn(Viernes, otraEstadiaActiva: false);
        var futuro = new Periodo(Lunes.AddDays(5), Lunes.AddDays(7));

        // hoy = viernes: la salida prevista (lunes) todavía no llega
        Assert.False(reserva.BloqueaDisponibilidad(futuro, Viernes));
    }

    [Fact]
    public void BloqueaDisponibilidad_EstadiaConSalidaVencidaSinCheckOut_BloqueaHastaRegistrarla()
    {
        var reserva = CrearReserva();
        reserva.RegistrarCheckIn(Viernes, otraEstadiaActiva: false);
        var futuro = new Periodo(Lunes.AddDays(5), Lunes.AddDays(7));

        // hoy = lunes: ya debía salir y no se registró el check-out
        Assert.True(reserva.BloqueaDisponibilidad(futuro, Lunes));
    }

    // ---- Cancelación ----

    [Fact]
    public void Cancelar_Reservada_RegistraFechaYConservaImporteSinPago()
    {
        var reserva = CrearReserva();

        reserva.Cancelar(Hoy);

        Assert.Equal(EstadoReserva.Cancelada, reserva.Estado);
        Assert.Equal(Hoy, reserva.FechaCancelacion);
        Assert.Equal(12920.00m, reserva.Importe);
        Assert.Null(reserva.Pago);
    }

    [Fact]
    public void Cancelar_DespuesDelCheckIn_LanzaTransicionInvalida()
    {
        var reserva = CrearReserva();
        reserva.RegistrarCheckIn(Viernes, otraEstadiaActiva: false);

        var ex = Assert.Throws<TransicionInvalidaException>(() => reserva.Cancelar(Viernes));

        Assert.Equal("TRANSICION_INVALIDA", ex.Codigo);
    }

    // ---- Check-in ----

    [Fact]
    public void CheckIn_FueraDeLasFechasReservadas_LanzaValidacion()
    {
        var reserva = CrearReserva();

        Assert.Throws<ValidacionDominioException>(() =>
            reserva.RegistrarCheckIn(Hoy, otraEstadiaActiva: false)); // un día antes de la entrada
    }

    [Fact]
    public void CheckIn_ConOtraEstadiaActiva_LanzaHabitacionNoDisponible()
    {
        var reserva = CrearReserva();

        Assert.Throws<HabitacionNoDisponibleException>(() =>
            reserva.RegistrarCheckIn(Viernes, otraEstadiaActiva: true));
    }

    // ---- Check-out y pago ----

    [Fact]
    public void CheckOut_EstadiaActiva_CreaUnPagoPorElImportePactado()
    {
        var reserva = CrearReserva();
        reserva.RegistrarCheckIn(Viernes, otraEstadiaActiva: false);

        var pago = reserva.RegistrarCheckOut(Lunes, MetodoPago.Tarjeta);

        Assert.Equal(EstadoReserva.Finalizada, reserva.Estado);
        Assert.Equal(Lunes, reserva.FechaCheckOut);
        Assert.Equal(12920.00m, pago.Monto);
        Assert.Equal(MetodoPago.Tarjeta, pago.Metodo);
        Assert.Same(pago, reserva.Pago);
    }

    [Fact]
    public void CheckOut_AntesDelCheckIn_LanzaTransicionInvalida()
    {
        var reserva = CrearReserva(); // sigue en estado Reservada

        Assert.Throws<TransicionInvalidaException>(() =>
            reserva.RegistrarCheckOut(Lunes, MetodoPago.Efectivo));
    }

    [Fact]
    public void CheckOut_Repetido_LanzaTransicionInvalidaYConservaUnSoloPago()
    {
        var reserva = CrearReserva();
        reserva.RegistrarCheckIn(Viernes, otraEstadiaActiva: false);
        var primerPago = reserva.RegistrarCheckOut(Lunes, MetodoPago.Efectivo);

        Assert.Throws<TransicionInvalidaException>(() =>
            reserva.RegistrarCheckOut(Lunes, MetodoPago.Tarjeta));

        Assert.Same(primerPago, reserva.Pago);
    }
}
