using Hotel.Application.Catalogos;
using Hotel.Application.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly ClienteServicio _servicio;
    public ClientesController(ClienteServicio servicio) => _servicio = servicio;

    [HttpGet]
    [Authorize(Roles = Roles.Ambos)]
    public async Task<ActionResult<IReadOnlyList<ClienteDto>>> Listar(CancellationToken ct)
        => Ok(await _servicio.ListarAsync(ct));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Ambos)]
    public async Task<ActionResult<ClienteDto>> Obtener(int id, CancellationToken ct)
        => Ok(await _servicio.ObtenerAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<ClienteDto>> Crear(ClienteInput input, CancellationToken ct)
    {
        var creado = await _servicio.CrearAsync(input, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<ClienteDto>> Actualizar(int id, ClienteInput input, CancellationToken ct)
        => Ok(await _servicio.ActualizarAsync(id, input, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _servicio.EliminarAsync(id, ct);
        return NoContent();
    }
}
