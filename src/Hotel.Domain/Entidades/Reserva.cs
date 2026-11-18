using Hotel.Domain.Enums;
using Hotel.Domain.Excepciones;

namespace Hotel.Domain.Entidades;

/// <summary>
/// Raíz del agregado. Nadie cambia su estado desde afuera: solo a través de
/// Crear, Cancelar, RegistrarCheckIn y RegistrarCheckOut (encapsulamiento).
/// "hoy" llega por parámetro: el dominio no conoce la zona horaria ni el reloj (eso es de Application).
/// </summary>
public class Reserva
{
    public int Id { get; private set; }
    public int HabitacionId { get; private set; }
    public Habitacion? Habitacion { get; private set; }
    public int ClienteId { get; private set; }
    public Cliente? Cliente { get; private set; }
    public DateOnly Entrada { get; private set; }
    public DateOnly Salida { get; private set; }
    public EstadoReserva Estado { get; private set; }
    public DateOnly? FechaCancelacion { get; private set; }
    public DateOnly? FechaCheckIn { get; private set; }
    public DateOnly? FechaCheckOut { get; private set; }
    public decimal Importe { get; private set; }
    public Pago? Pago { get; private set; }

    private Reserva() { } // EF Core

    /// <summary>Caso de uso "Reservar": valida, calcula el importe y lo congela en la reserva.</summary>
    public static Reserva Crear(Habitacion habitacion, Cliente cliente, DateOnly entrada, DateOnly salida, DateOnly hoy)
    {
        ArgumentNullException.ThrowIfNull(habitacion);
        ArgumentNullException.ThrowIfNull(cliente);

        if (entrada < hoy)
            throw new ValidacionDominioException("La entrada no puede ser anterior a hoy.");

        var periodo = new Periodo(entrada, salida); // valida salida > entrada y 1..30 noches

        if (!habitacion.Activa)
            throw new HabitacionNoDisponibleException($"La habitación {habitacion.Numero} está fuera de servicio.");
        if (habitacion.TipoHabitacion is null)
            throw new InvalidOperationException("La habitación debe cargarse con su tipo para calcular la tarifa.");

        return new Reserva
        {
            Habitacion = habitacion,
            HabitacionId = habitacion.Id,
            Cliente = cliente,
            ClienteId = cliente.Id,
            Entrada = entrada,
            Salida = salida,
            Estado = EstadoReserva.Reservada,
            Importe = habitacion.TipoHabitacion.CalcularImporte(periodo)
        };
    }

    public Periodo ObtenerPeriodo() => new(Entrada, Salida);

    /// <summary>
    /// ¿Esta reserva impide reservar la habitación en el período dado?
    /// Reservada: solo si se solapa. EnEstadia: bloquea hasta que se registre el check-out.
    /// Cancelada y Finalizada no bloquean.
    /// </summary>
    public bool BloqueaDisponibilidad(Periodo solicitado) => Estado switch
    {
        EstadoReserva.Reservada => ObtenerPeriodo().SolapaCon(solicitado),
        EstadoReserva.EnEstadia => solicitado.Salida > Entrada,
        _ => false
    };

    public void Cancelar(DateOnly hoy)
    {
        if (Estado != EstadoReserva.Reservada)
            throw new TransicionInvalidaException($"Solo se puede cancelar una reserva en estado Reservada (actual: {Estado}).");

        Estado = EstadoReserva.Cancelada;
        FechaCancelacion = hoy;
    }

    /// <param name="otraEstadiaActiva">Application consulta la BD y nos dice si otra estadía sigue abierta en la habitación.</param>
    public void RegistrarCheckIn(DateOnly hoy, bool otraEstadiaActiva)
    {
        if (Estado != EstadoReserva.Reservada)
            throw new TransicionInvalidaException($"Solo una reserva Reservada admite check-in (actual: {Estado}).");
        if (hoy < Entrada || hoy >= Salida)
            throw new ValidacionDominioException("El check-in debe hacerse dentro de las fechas reservadas.");
        if (Habitacion is not null && !Habitacion.Activa)
            throw new HabitacionNoDisponibleException("La habitación está fuera de servicio.");
        if (otraEstadiaActiva)
            throw new HabitacionNoDisponibleException("Otra estadía sigue activa en esta habitación.");

        Estado = EstadoReserva.EnEstadia;
        FechaCheckIn = hoy;
    }

    /// <summary>Cierra la estadía y crea el único pago por el importe pactado (no se recalcula).</summary>
    public Pago RegistrarCheckOut(DateOnly hoy, MetodoPago metodo)
    {
        if (Estado != EstadoReserva.EnEstadia)
            throw new TransicionInvalidaException($"Solo una estadía activa admite check-out (actual: {Estado}).");
        if (!Enum.IsDefined(metodo))
            throw new ValidacionDominioException("Método de pago fuera del catálogo.");
        if (hoy < FechaCheckIn)
            throw new ValidacionDominioException("La salida no puede ser anterior al check-in.");

        Estado = EstadoReserva.Finalizada;
        FechaCheckOut = hoy;
        Pago = new Pago(Importe, metodo, hoy);
        return Pago;
    }
}
