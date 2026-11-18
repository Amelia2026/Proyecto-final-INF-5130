using Hotel.Domain.Enums;

namespace Hotel.Domain.Entidades;

/// <summary>Solo lo crea Reserva.RegistrarCheckOut: no existe un pago sin una estadía cerrada.</summary>
public class Pago
{
    public int Id { get; private set; }
    public int ReservaId { get; private set; }
    public decimal Monto { get; private set; }
    public MetodoPago Metodo { get; private set; }
    public DateOnly Fecha { get; private set; }

    private Pago() { } // EF Core

    internal Pago(decimal monto, MetodoPago metodo, DateOnly fecha)
    {
        Monto = monto;
        Metodo = metodo;
        Fecha = fecha;
    }
}
