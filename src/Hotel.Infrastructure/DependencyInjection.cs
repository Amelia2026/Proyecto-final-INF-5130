using Hotel.Application.Abstracciones;
using Hotel.Infrastructure.Persistencia;
using Hotel.Infrastructure.Repositorios;
using Hotel.Infrastructure.Servicios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Infrastructure;

public static class DependencyInjection
{
    /// <summary>La Api llama a esto una vez al arrancar para "enchufar" todo lo de Infrastructure.</summary>
    public static IServiceCollection AgregarInfraestructura(this IServiceCollection servicios, string cadenaConexion)
    {
        servicios.AddDbContext<HotelDbContext>(o => o.UseSqlServer(cadenaConexion));

        // Identity: usuarios y roles guardados en la misma base. (Core = sin cookies ni pantallas; usamos JWT.)
        servicios.AddIdentityCore<IdentityUser>(o => o.User.RequireUniqueEmail = true)
                 .AddRoles<IdentityRole>()
                 .AddEntityFrameworkStores<HotelDbContext>();

        servicios.AddScoped<IReservaRepositorio, ReservaRepositorio>();
        servicios.AddScoped<IHabitacionRepositorio, HabitacionRepositorio>();
        servicios.AddScoped<IClienteRepositorio, ClienteRepositorio>();
        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
        servicios.AddSingleton<IReloj, RelojSantoDomingo>();
        return servicios;
    }
}
