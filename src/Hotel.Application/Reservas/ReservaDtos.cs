using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;

namespace Hotel.Application.Reservas;

/// <summary>Lo que el cliente envía para reservar (ReservaInput del contrato REST).</summary>
public record ReservaInput(int HabitacionId, int ClienteId, DateOnly Entrada, DateOnly Salida);

/// <summary>Lo que el cliente envía al hacer check-out (CheckOutInput del contrato REST).</summary>
public record CheckOutInput(MetodoPago MetodoPago);

public record PagoDto(int Id, decimal Monto, MetodoPago Metodo, DateOnly Fecha);

/// <summary>Lo que se devuelve al cliente. Se arma a partir de la entidad para no exponer el Domain tal cual.</summary>
public record ReservaDto(
    int Id,
    int HabitacionId,
    string NumeroHabitacion,
    int ClienteId,
    string ClienteNombre,
    DateOnly Entrada,
    DateOnly Salida,
    EstadoReserva Estado,
    DateOnly? FechaCancelacion,
    DateOnly? FechaCheckIn,
    DateOnly? FechaCheckOut,
    decimal Importe,
    PagoDto? Pago)
{
    public static ReservaDto Desde(Reserva r) => new(
        r.Id,
        r.HabitacionId,
        r.Habitacion?.Numero ?? string.Empty,
        r.ClienteId,
        r.Cliente?.Nombre ?? string.Empty,
        r.Entrada,
        r.Salida,
        r.Estado,
        r.FechaCancelacion,
        r.FechaCheckIn,
        r.FechaCheckOut,
        r.Importe,
        r.Pago is null ? null : new PagoDto(r.Pago.Id, r.Pago.Monto, r.Pago.Metodo, r.Pago.Fecha));
}
