using Hotel.Application.Abstracciones;
using Hotel.Application.Excepciones;
using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;
using Hotel.Domain.Excepciones;

namespace Hotel.Application.Reservas;

/// <summary>
/// Los casos de uso: reservar, cancelar, check-in y check-out.
/// Esta clase NO tiene reglas de negocio propias (esas viven en Domain): solo coordina
/// "buscar datos -> pedirle al Domain que decida -> guardar todo junto".
/// </summary>
public class ReservaServicio
{
    private readonly IReservaRepositorio _reservas;
    private readonly IHabitacionRepositorio _habitaciones;
    private readonly IClienteRepositorio _clientes;
    private readonly IUnidadDeTrabajo _unidad;
    private readonly IReloj _reloj;

    public ReservaServicio(
        IReservaRepositorio reservas,
        IHabitacionRepositorio habitaciones,
        IClienteRepositorio clientes,
        IUnidadDeTrabajo unidad,
        IReloj reloj)
    {
        _reservas = reservas;
        _habitaciones = habitaciones;
        _clientes = clientes;
        _unidad = unidad;
        _reloj = reloj;
    }

    // ---------- Consultas ----------

    public async Task<ReservaDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var reserva = await _reservas.ObtenerAsync(id, ct)
            ?? throw new NoEncontradoException("Reserva", id);
        return ReservaDto.Desde(reserva);
    }

    public async Task<IReadOnlyList<ReservaDto>> ListarAsync(CancellationToken ct = default)
    {
        var reservas = await _reservas.ListarAsync(ct);
        return reservas.Select(ReservaDto.Desde).ToList();
    }

    // ---------- Caso de uso 1: reservar ----------

    public async Task<ReservaDto> ReservarAsync(ReservaInput input, CancellationToken ct = default)
    {
        var reserva = await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var hoy = _reloj.Hoy();

            // Bloqueamos la habitación: si dos personas reservan a la vez, la segunda espera a que termine la primera.
            var habitacion = await _habitaciones.ObtenerConBloqueoAsync(input.HabitacionId, ct)
                ?? throw new NoEncontradoException("Habitación", input.HabitacionId);
            var cliente = await _clientes.ObtenerAsync(input.ClienteId, ct)
                ?? throw new NoEncontradoException("Cliente", input.ClienteId);

            // El Domain valida fechas, 1..30 noches, habitación en servicio y calcula el importe.
            var nueva = Reserva.Crear(habitacion, cliente, input.Entrada, input.Salida, hoy);

            // Disponibilidad: ¿alguna reserva activa de esta habitación estorba en ese período?
            var activas = await _reservas.ListarActivasDeHabitacionAsync(habitacion.Id, ct);
            var periodo = nueva.ObtenerPeriodo();
            if (activas.Any(r => r.BloqueaDisponibilidad(periodo, hoy)))
                throw new HabitacionNoDisponibleException(
                    $"La habitación {habitacion.Numero} ya está reservada u ocupada en ese período.");

            _reservas.Agregar(nueva);
            return nueva;
        }, ct);

        // El Id solo existe después de guardar, por eso se arma el DTO aquí y no adentro.
        return ReservaDto.Desde(reserva);
    }

    // ---------- Caso de uso 2: cancelar ----------

    public async Task<ReservaDto> CancelarAsync(int reservaId, CancellationToken ct = default)
    {
        var reserva = await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var r = await _reservas.ObtenerConBloqueoAsync(reservaId, ct)
                ?? throw new NoEncontradoException("Reserva", reservaId);
            r.Cancelar(_reloj.Hoy()); // al quedar Cancelada, deja de bloquear el período
            return r;
        }, ct);

        return ReservaDto.Desde(reserva);
    }

    // ---------- Caso de uso 3a: check-in ----------

    public async Task<ReservaDto> RegistrarCheckInAsync(int reservaId, CancellationToken ct = default)
    {
        var reserva = await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var r = await _reservas.ObtenerConBloqueoAsync(reservaId, ct)
                ?? throw new NoEncontradoException("Reserva", reservaId);

            var activas = await _reservas.ListarActivasDeHabitacionAsync(r.HabitacionId, ct);
            var otraEstadiaActiva = activas.Any(a => a.Id != r.Id && a.Estado == EstadoReserva.EnEstadia);

            r.RegistrarCheckIn(_reloj.Hoy(), otraEstadiaActiva);
            return r;
        }, ct);

        return ReservaDto.Desde(reserva);
    }

    // ---------- Caso de uso 3b: check-out (cierra la estadía y crea el pago) ----------

    public async Task<ReservaDto> RegistrarCheckOutAsync(int reservaId, CheckOutInput input, CancellationToken ct = default)
    {
        var reserva = await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var r = await _reservas.ObtenerConBloqueoAsync(reservaId, ct)
                ?? throw new NoEncontradoException("Reserva", reservaId);

            // Cambia el estado Y crea el pago; ambos se guardan en la misma transacción.
            r.RegistrarCheckOut(_reloj.Hoy(), input.MetodoPago);
            return r;
        }, ct);

        return ReservaDto.Desde(reserva);
    }
}
