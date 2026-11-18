using Hotel.Application.Abstracciones;
using Hotel.Domain.Entidades;

namespace Hotel.Application.Tests;

/// <summary>Asigna el Id a una entidad (en la vida real lo asigna la base de datos).</summary>
internal static class Reflejo
{
    public static T ConId<T>(this T entidad, int id) where T : class
    {
        typeof(T).GetProperty("Id")!.SetValue(entidad, id);
        return entidad;
    }
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
    public Task<Cliente?> ObtenerAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Datos.FirstOrDefault(c => c.Id == id));
}

internal class FakeHabitacionRepositorio : IHabitacionRepositorio
{
    public List<Habitacion> Datos { get; } = new();
    public Task<Habitacion?> ObtenerConBloqueoAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Datos.FirstOrDefault(h => h.Id == id));
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
                          && (r.Estado == Domain.Enums.EstadoReserva.Reservada
                           || r.Estado == Domain.Enums.EstadoReserva.EnEstadia)).ToList());

    public void Agregar(Reserva reserva)
    {
        reserva.ConId(_siguienteId++);
        Datos.Add(reserva);
    }
}
