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

    public async Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken ct = default)
        => await _db.Clientes.AsNoTracking().OrderBy(c => c.Id).ToListAsync(ct);

    public Task<bool> ExisteEmailAsync(string email, int? excluirId, CancellationToken ct = default)
        => _db.Clientes.AnyAsync(c => c.Email == email && (excluirId == null || c.Id != excluirId.Value), ct);

    public Task<bool> ExisteDocumentoAsync(string documento, int? excluirId, CancellationToken ct = default)
        => _db.Clientes.AnyAsync(c => c.Documento == documento && (excluirId == null || c.Id != excluirId.Value), ct);

    public Task<bool> TieneReservasAsync(int id, CancellationToken ct = default)
        => _db.Reservas.AnyAsync(r => r.ClienteId == id, ct);

    public void Agregar(Cliente cliente) => _db.Clientes.Add(cliente);
    public void Eliminar(Cliente cliente) => _db.Clientes.Remove(cliente);
}
