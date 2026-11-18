using Hotel.Application.Reservas;
using Hotel.Application.Seguridad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

/// <summary>
/// La "ventanilla" de reservas. No tiene lógica: recibe la petición HTTP, llama al caso de uso
/// de Application y devuelve el resultado. Los errores los traduce ManejadorDeExcepciones.
/// </summary>
[ApiController]
[Route("api/reservas")]
[Authorize(Roles = Roles.Ambos)] // Administrador y Gestor; sin token -> 401
public class ReservasController : ControllerBase
{
    private readonly ReservaServicio _servicio;
    public ReservasController(ReservaServicio servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ReservaDto>>> Listar(CancellationToken ct)
        => Ok(await _servicio.ListarAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservaDto>> Obtener(int id, CancellationToken ct)
        => Ok(await _servicio.ObtenerAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ReservaDto>> Reservar(ReservaInput input, CancellationToken ct)
    {
        var reserva = await _servicio.ReservarAsync(input, ct);
        // 201 Created + encabezado Location con la dirección de la reserva nueva (lo pide el contrato)
        return CreatedAtAction(nameof(Obtener), new { id = reserva.Id }, reserva);
    }

    [HttpPost("{id:int}/cancelacion")]
    public async Task<ActionResult<ReservaDto>> Cancelar(int id, CancellationToken ct)
        => Ok(await _servicio.CancelarAsync(id, ct));

    [HttpPost("{id:int}/check-in")]
    public async Task<ActionResult<ReservaDto>> CheckIn(int id, CancellationToken ct)
        => Ok(await _servicio.RegistrarCheckInAsync(id, ct));

    [HttpPost("{id:int}/check-out")]
    public async Task<ActionResult<ReservaDto>> CheckOut(int id, CheckOutInput input, CancellationToken ct)
        => Ok(await _servicio.RegistrarCheckOutAsync(id, input, ct));
}
