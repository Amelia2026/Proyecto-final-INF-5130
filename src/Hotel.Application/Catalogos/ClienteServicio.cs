using Hotel.Application.Abstracciones;
using Hotel.Application.Excepciones;
using Hotel.Domain.Entidades;

namespace Hotel.Application.Catalogos;

public class ClienteServicio
{
    private readonly IClienteRepositorio _clientes;
    private readonly IUnidadDeTrabajo _unidad;

    public ClienteServicio(IClienteRepositorio clientes, IUnidadDeTrabajo unidad)
    {
        _clientes = clientes;
        _unidad = unidad;
    }

    public async Task<IReadOnlyList<ClienteDto>> ListarAsync(CancellationToken ct = default)
        => (await _clientes.ListarAsync(ct)).Select(ClienteDto.Desde).ToList();

    public async Task<ClienteDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var c = await _clientes.ObtenerAsync(id, ct) ?? throw new NoEncontradoException("Cliente", id);
        return ClienteDto.Desde(c);
    }

    public async Task<ClienteDto> CrearAsync(ClienteInput input, CancellationToken ct = default)
    {
        var cliente = await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var nuevo = new Cliente(input.Nombre, input.Email, input.Documento); // el Domain valida
            await VerificarUnicosAsync(nuevo, null, ct);
            _clientes.Agregar(nuevo);
            return nuevo;
        }, ct);
        return ClienteDto.Desde(cliente);
    }

    public async Task<ClienteDto> ActualizarAsync(int id, ClienteInput input, CancellationToken ct = default)
    {
        var cliente = await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var existente = await _clientes.ObtenerAsync(id, ct) ?? throw new NoEncontradoException("Cliente", id);
            existente.Actualizar(input.Nombre, input.Email, input.Documento);
            await VerificarUnicosAsync(existente, id, ct);
            return existente;
        }, ct);
        return ClienteDto.Desde(cliente);
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        await _unidad.EjecutarEnTransaccionAsync(async () =>
        {
            var existente = await _clientes.ObtenerAsync(id, ct) ?? throw new NoEncontradoException("Cliente", id);
            if (await _clientes.TieneReservasAsync(id, ct))
                throw new RecursoEnUsoException("No se puede borrar un cliente que tiene historial de reservas.");
            _clientes.Eliminar(existente);
            return true;
        }, ct);
    }

    private async Task VerificarUnicosAsync(Cliente cliente, int? excluirId, CancellationToken ct)
    {
        if (await _clientes.ExisteEmailAsync(cliente.Email, excluirId, ct))
            throw new DuplicadoException("un cliente", "correo", cliente.Email);
        if (await _clientes.ExisteDocumentoAsync(cliente.Documento, excluirId, ct))
            throw new DuplicadoException("un cliente", "documento", cliente.Documento);
    }
}
