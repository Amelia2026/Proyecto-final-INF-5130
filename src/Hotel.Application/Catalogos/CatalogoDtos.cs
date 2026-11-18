using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;

namespace Hotel.Application.Catalogos;

// ----- Tipos de habitación -----
public record TipoInput(string Nombre, decimal TarifaBase, TipoEstrategia Estrategia);

public record TipoHabitacionDto(int Id, string Nombre, decimal TarifaBase, TipoEstrategia Estrategia)
{
    public static TipoHabitacionDto Desde(TipoHabitacion t) => new(t.Id, t.Nombre, t.TarifaBase, t.Estrategia);
}

// ----- Habitaciones -----
public record HabitacionInput(int TipoHabitacionId, string Numero, bool Activa);

public record HabitacionDto(int Id, int TipoHabitacionId, string Numero, bool Activa)
{
    public static HabitacionDto Desde(Habitacion h) => new(h.Id, h.TipoHabitacionId, h.Numero, h.Activa);
}

// ----- Clientes -----
public record ClienteInput(string Nombre, string Email, string Documento);

public record ClienteDto(int Id, string Nombre, string Email, string Documento)
{
    public static ClienteDto Desde(Cliente c) => new(c.Id, c.Nombre, c.Email, c.Documento);
}
