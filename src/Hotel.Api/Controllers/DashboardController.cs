using Hotel.Application.Seguridad;
using Hotel.Application.Tablero;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = Roles.Ambos)]
public class DashboardController : ControllerBase
{
    private readonly TableroServicio _servicio;
    public DashboardController(TableroServicio servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<ResumenDto>> Obtener(CancellationToken ct)
        => Ok(await _servicio.ObtenerAsync(ct));
}
