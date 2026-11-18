using Hotel.Application.Abstracciones;
using Hotel.Application.Excepciones;
using Hotel.Domain.Entidades;

namespace Hotel.Application.Catalogos;

/// <summary>Mantenimiento de tipos de habitación (tarifa base y estrategia).</summary>
public class TipoHabitacionServicio
{
    private readonly ITipoHabitacionRepositorio _tipos;
    private readonly IUnidadDeTrabajo _unidad;

    public TipoHabitacionServicio(ITipoHabitacionRepositorio tipos, IUnidadDeTrabajo unidad)
    {
        _tipos = tipos;
        _unidad = unidad;
    }

    public async Task<IReadOnlyList<TipoHabitacionDto>> ListarAsync(CancellationToken ct = default)
        => (await _tipos.ListarAsync(ct)).Select(TipoHabitacionDto.Desde).ToList();

    public async Task<TipoHabitacionDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var tipo = await _tipos.ObtenerAsync(id, ct) ?? throw new NoEncontradoException("Tipo de habitación", id);
        return TipoHabitacionDto.Desde(tipo);
    }

    public async Task<TipoHabitacionDto> CrearAsync(TipoInput input, CancellationToken ct = default)
    {
        var tipo = await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var nuevo = new TipoHabitacion(input.Nombre, input.TarifaBase, input.Estrategia); // el Domain valida
            if (await _tipos.ExisteNombreAsync(nuevo.Nombre, null, ct))
                throw new DuplicadoException("un tipo de habitación", "nombre", nuevo.Nombre);
            _tipos.Agregar(nuevo);
            return nuevo;
        }, ct);
        return TipoHabitacionDto.Desde(tipo);
    }

    public async Task<TipoHabitacionDto> ActualizarAsync(int id, TipoInput input, CancellationToken ct = default)
    {
        var tipo = await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var existente = await _tipos.ObtenerAsync(id, ct) ?? throw new NoEncontradoException("Tipo de habitación", id);
            // Cambiar la tarifa NO altera las reservas ya creadas: guardan su importe pactado.
            existente.Actualizar(input.Nombre, input.TarifaBase, input.Estrategia);
            if (await _tipos.ExisteNombreAsync(existente.Nombre, id, ct))
                throw new DuplicadoException("un tipo de habitación", "nombre", existente.Nombre);
            return existente;
        }, ct);
        return TipoHabitacionDto.Desde(tipo);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var tipo = await _tipos.ObtenerAsync(id, ct) ?? throw new NoEncontradoException("Tipo de habitación", id);
            if (await _tipos.TieneHabitacionesAsync(id, ct))
                throw new RecursoEnUsoException("No se puede borrar un tipo que tiene habitaciones asociadas.");
            _tipos.Eliminar(tipo);
            return true;
        }, ct);
    }
}
