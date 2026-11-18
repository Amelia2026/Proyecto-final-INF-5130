using Hotel.Application.Abstracciones;
using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;

namespace Hotel.Application.Tests;

/// <summary>Asigna el Id a una entidad (en la vida real lo asigna la base de datos).</summary>
internal static class Reflejo
{
    public static T ConId<T>(this T entidad, int id) where T : class
    {
        typeof(T).GetProperty("Id")!.SetValue(entidad, id);
        return entidad;
    }

    public static bool Igual(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
}

internal class FakeReloj : IReloj
{
    public DateOnly Fecha { get; set; }
    public FakeReloj(DateOnly fecha) => Fecha = fecha;
    public DateOnly Hoy() => Fecha;
}

internal class FakeUnidadDeTrabajo : IUnidadDeTrabajo
{
    public async Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion, CancellationToken ct = default)
        => await operacion();
}

internal class FakeClienteRepositorio : IClienteRepositorio
{
    public List<Cliente> Datos { get; } = new();
    public FakeReservaRepositorio? Reservas { get; set; }

    public Task<Cliente?> ObtenerAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Datos.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Cliente>>(Datos.ToList());

    public Task<bool> ExisteEmailAsync(string email, int? excluirId, CancellationToken ct = default)
        => Task.FromResult(Datos.Any(c => c.Id != excluirId && Reflejo.Igual(c.Email, email)));

    public Task<bool> ExisteDocumentoAsync(string documento, int? excluirId, CancellationToken ct = default)
        => Task.FromResult(Datos.Any(c => c.Id != excluirId && Reflejo.Igual(c.Documento, documento)));

    public Task<bool> TieneReservasAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Reservas?.Datos.Any(r => r.ClienteId == id) ?? false);

    public void Agregar(Cliente cliente)
    {
        cliente.ConId(Datos.Count == 0 ? 1 : Datos.Max(c => c.Id) + 1);
        Datos.Add(cliente);
    }

    public void Eliminar(Cliente cliente) => Datos.Remove(cliente);
}

internal class FakeTipoHabitacionRepositorio : ITipoHabitacionRepositorio
{
    public List<TipoHabitacion> Datos { get; } = new();
    public FakeHabitacionRepositorio? Habitaciones { get; set; }

    public Task<TipoHabitacion?> ObtenerAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Datos.FirstOrDefault(t => t.Id == id));

    public Task<IReadOnlyList<TipoHabitacion>> ListarAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<TipoHabitacion>>(Datos.ToList());

    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId, CancellationToken ct = default)
        => Task.FromResult(Datos.Any(t => t.Id != excluirId && Reflejo.Igual(t.Nombre, nombre)));

    public Task<bool> TieneHabitacionesAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Habitaciones?.Datos.Any(h => h.TipoHabitacionId == id) ?? false);

    public void Agregar(TipoHabitacion tipo)
    {
        tipo.ConId(Datos.Count == 0 ? 1 : Datos.Max(t => t.Id) + 1);
        Datos.Add(tipo);
    }

    public void Eliminar(TipoHabitacion tipo) => Datos.Remove(tipo);
}

internal class FakeHabitacionRepositorio : IHabitacionRepositorio
{
    public List<Habitacion> Datos { get; } = new();
    public FakeReservaRepositorio? Reservas { get; set; }

    public Task<Habitacion?> ObtenerConBloqueoAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Datos.FirstOrDefault(h => h.Id == id));

    public Task<Habitacion?> ObtenerAsync(int id, CancellationToken ct = default)
        => ObtenerConBloqueoAsync(id, ct);

    public Task<IReadOnlyList<Habitacion>> ListarAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Habitacion>>(Datos.ToList());

    public Task<bool> ExisteNumeroAsync(string numero, int? excluirId, CancellationToken ct = default)
        => Task.FromResult(Datos.Any(h => h.Id != excluirId && Reflejo.Igual(h.Numero, numero)));

    public Task<bool> TieneReservasAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Reservas?.Datos.Any(r => r.HabitacionId == id) ?? false);

    public Task<bool> TieneReservasActivasAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Reservas?.Datos.Any(r => r.HabitacionId == id
            && (r.Estado == EstadoReserva.Reservada || r.Estado == EstadoReserva.EnEstadia)) ?? false);

    public void Agregar(Habitacion habitacion)
    {
        habitacion.ConId(Datos.Count == 0 ? 1 : Datos.Max(h => h.Id) + 1);
        Datos.Add(habitacion);
    }

    public void Eliminar(Habitacion habitacion) => Datos.Remove(habitacion);
}

internal class FakeReservaRepositorio : IReservaRepositorio
{
    private int _siguienteId = 1;
    public List<Reserva> Datos { get; } = new();

    public Task<Reserva?> ObtenerAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Datos.FirstOrDefault(r => r.Id == id));

    public Task<Reserva?> ObtenerConBloqueoAsync(int id, CancellationToken ct = default)
        => ObtenerAsync(id, ct);

    public Task<IReadOnlyList<Reserva>> ListarAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Reserva>>(Datos.OrderByDescending(r => r.Id).ToList());

    public Task<IReadOnlyList<Reserva>> ListarActivasDeHabitacionAsync(int habitacionId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Reserva>>(
            Datos.Where(r => r.HabitacionId == habitacionId
                          && (r.Estado == EstadoReserva.Reservada || r.Estado == EstadoReserva.EnEstadia)).ToList());

    public void Agregar(Reserva reserva)
    {
        reserva.ConId(_siguienteId++);
        Datos.Add(reserva);
    }
}
