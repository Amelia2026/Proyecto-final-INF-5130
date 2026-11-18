using Hotel.Application.Catalogos;
using Hotel.Application.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/habitaciones")]
public class HabitacionesController : ControllerBase
{
    private readonly HabitacionServicio _servicio;
    public HabitacionesController(HabitacionServicio servicio) => _servicio = servicio;

    [HttpGet]
    [Authorize(Roles = Roles.Ambos)]
    public async Task<ActionResult<IReadOnlyList<HabitacionDto>>> Listar(CancellationToken ct)
        => Ok(await _servicio.ListarAsync(ct));

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.Ambos)]
    public async Task<ActionResult<HabitacionDto>> Obtener(int id, CancellationToken ct)
        => Ok(await _servicio.ObtenerAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<HabitacionDto>> Crear(HabitacionInput input, CancellationToken ct)
    {
        var creada = await _servicio.CrearAsync(input, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<HabitacionDto>> Actualizar(int id, HabitacionInput input, CancellationToken ct)
        => Ok(await _servicio.ActualizarAsync(id, input, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _servicio.EliminarAsync(id, ct);
        return NoContent();
    }
}
