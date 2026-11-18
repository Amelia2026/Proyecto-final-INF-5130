using Hotel.Domain.Entidades;

namespace Hotel.Application.Abstracciones;

public interface IClienteRepositorio
{
    Task<Cliente?> ObtenerAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken ct = default);
    Task<bool> ExisteEmailAsync(string email, int? excluirId, CancellationToken ct = default);
    Task<bool> ExisteDocumentoAsync(string documento, int? excluirId, CancellationToken ct = default);
    Task<bool> TieneReservasAsync(int id, CancellationToken ct = default);
    void Agregar(Cliente cliente);
    void Eliminar(Cliente cliente);
}
