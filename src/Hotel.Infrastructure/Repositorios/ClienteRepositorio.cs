using Hotel.Application.Abstracciones;
using Hotel.Domain.Entidades;
using Hotel.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Repositorios;

public class ClienteRepositorio : IClienteRepositorio
{
    private readonly HotelDbContext _db;
    public ClienteRepositorio(HotelDbContext db) => _db = db;

    public Task<Cliente?> ObtenerAsync(int id, CancellationToken ct = default)
        => _db.Clientes.FirstOrDefaultAsync(c => c.Id == id, ct);
}
