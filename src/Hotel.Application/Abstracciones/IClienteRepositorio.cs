using Hotel.Domain.Entidades;

namespace Hotel.Application.Abstracciones;

public interface IClienteRepositorio
{
    Task<Cliente?> ObtenerAsync(int id, CancellationToken ct = default);
}
