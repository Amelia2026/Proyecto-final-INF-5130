using Hotel.Application.Abstracciones;
using Hotel.Infrastructure.Persistencia;
using Hotel.Infrastructure.Repositorios;
using Hotel.Infrastructure.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Infrastructure;

public static class DependencyInjection
{
    /// <summary>La Api llamará a esto una vez al arrancar para "enchufar" todo lo de Infrastructure.</summary>
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection servicios, string cadenaConexion)
    {
        servicios.AddDbContext<HotelDbContext>(o => o.UseSqlServer(cadenaConexion));
        servicios.AddScoped<IReservaRepositorio, ReservaRepositorio>();
        servicios.AddScoped<IHabitacionRepositorio, HabitacionRepositorio>();
        servicios.AddScoped<IClienteRepositorio, ClienteRepositorio>();
        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
        servicios.AddSingleton<IReloj, RelojSantoDomingo>();
        return servicios;
    }
}
