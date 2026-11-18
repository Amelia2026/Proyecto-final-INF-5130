using Hotel.Application.Catalogos;
using Hotel.Application.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/tipos-habitacion")]
public class TiposHabitacionController : ControllerBase
{
    private readonly TipoHabitacionServicio _servicio;
    public TiposHabitacionController(TipoHabitacionServicio servicio) => _servicio = servicio;

    [HttpGet]
    [Authorize(Roles = Roles.Ambos)]
    public async Task<ActionResult<IReadOnlyList<TipoHabitacionDto>>> Listar(CancellationToken ct)
        => Ok(await _servicio.ListarAsync(ct));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Ambos)]
    public async Task<ActionResult<TipoHabitacionDto>> Obtener(int id, CancellationToken ct)
        => Ok(await _servicio.ObtenerAsync(id, ct));

    // Cambiar tarifas es solo del Administrador: el Gestor recibe 403.
    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TipoHabitacionDto>> Crear(TipoInput input, CancellationToken ct)
    {
        var creado = await _servicio.CrearAsync(input, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<TipoHabitacionDto>> Actualizar(int id, TipoInput input, CancellationToken ct)
        => Ok(await _servicio.ActualizarAsync(id, input, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _servicio.EliminarAsync(id, ct);
        return NoContent();
    }
}
