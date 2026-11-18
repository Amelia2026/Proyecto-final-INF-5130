using Hotel.Application.Abstracciones;
using Hotel.Application.Excepciones;
using Hotel.Domain.Entidades;

namespace Hotel.Application.Catalogos;

public class HabitacionServicio
{
    private readonly IHabitacionRepositorio _habitaciones;
    private readonly ITipoHabitacionRepositorio _tipos;
    private readonly IUnidadDeTrabajo _unidad;

    public HabitacionServicio(IHabitacionRepositorio habitaciones, ITipoHabitacionRepositorio tipos, IUnidadDeTrabajo unidad)
    {
        _habitaciones = habitaciones;
        _tipos = tipos;
        _unidad = unidad;
    }

    public async Task<IReadOnlyList<HabitacionDto>> ListarAsync(CancellationToken ct = default)
        => (await _habitaciones.ListarAsync(ct)).Select(HabitacionDto.Desde).ToList();

    public async Task<HabitacionDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var h = await _habitaciones.ObtenerAsync(id, ct) ?? throw new NoEncontradoException("Habitación", id);
        return HabitacionDto.Desde(h);
    }

    public async Task<HabitacionDto> CrearAsync(HabitacionInput input, CancellationToken ct = default)
    {
        var habitacion = await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var tipo = await _tipos.ObtenerAsync(input.TipoHabitacionId, ct)
                ?? throw new NoEncontradoException("Tipo de habitación", input.TipoHabitacionId);
            var nueva = new Habitacion(tipo, input.Numero, input.Activa); // el Domain valida
            if (await _habitaciones.ExisteNumeroAsync(nueva.Numero, null, ct))
                throw new DuplicadoException("una habitación", "número", nueva.Numero);
            _habitaciones.Agregar(nueva);
            return nueva;
        }, ct);
        return HabitacionDto.Desde(habitacion);
    }

    public async Task<HabitacionDto> ActualizarAsync(int id, HabitacionInput input, CancellationToken ct = default)
    {
        var habitacion = await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var existente = await _habitaciones.ObtenerAsync(id, ct) ?? throw new NoEncontradoException("Habitación", id);
            var tipo = await _tipos.ObtenerAsync(input.TipoHabitacionId, ct)
                ?? throw new NoEncontradoException("Tipo de habitación", input.TipoHabitacionId);

            // Regla: con reserva vigente o estadía activa, la habitación conserva su tipo y su condición de servicio.
            var cambiaTipoOServicio = existente.TipoHabitacionId != tipo.Id || existente.Activa != input.Activa;
            if (cambiaTipoOServicio && await _habitaciones.TieneReservasActivasAsync(id, ct))
                throw new RecursoEnUsoException(
                    "La habitación tiene una reserva vigente o una estadía activa: no se puede cambiar su tipo ni su condición de servicio.");

            existente.Actualizar(tipo, input.Numero, input.Activa);
            if (await _habitaciones.ExisteNumeroAsync(existente.Numero, id, ct))
                throw new DuplicadoException("una habitación", "número", existente.Numero);
            return existente;
        }, ct);
        return HabitacionDto.Desde(habitacion);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var existente = await _habitaciones.ObtenerAsync(id, ct) ?? throw new NoEncontradoException("Habitación", id);
            if (await _habitaciones.TieneReservasAsync(id, ct))
                throw new RecursoEnUsoException("No se puede borrar una habitación que tiene historial de reservas.");
            _habitaciones.Eliminar(existente);
            return true;
        }, ct);
    }
}
