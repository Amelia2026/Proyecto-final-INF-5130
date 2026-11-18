namespace Hotel.Domain.Enums;

/// <summary>Ciclo de vida de una reserva. Reservada -> EnEstadia -> Finalizada, o Reservada -> Cancelada.</summary>
public enum EstadoReserva { Reservada, Cancelada, EnEstadia, Finalizada }
