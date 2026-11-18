using Hotel.Application.Seguridad;
using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Infrastructure.Persistencia;

/// <summary>
/// Datos de desarrollo: crea la base desde las migraciones, los 2 roles, 2 usuarios de prueba
/// y datos de ejemplo. Solo agrega lo que falta, así que se puede ejecutar muchas veces.
/// </summary>
public static class DatosIniciales
{
    public static async Task SembrarAsync(IServiceProvider servicios, CancellationToken ct = default)
    {
        using var alcance = servicios.CreateScope();
        var sp = alcance.ServiceProvider;

        var db = sp.GetRequiredService<HotelDbContext>();
        await db.Database.MigrateAsync(ct); // crea/actualiza todas las tablas según las migraciones

        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var rol in new[] { Roles.Administrador, Roles.Gestor })
            if (!await roles.RoleExistsAsync(rol))
                await roles.CreateAsync(new IdentityRole(rol));

        var usuarios = sp.GetRequiredService<UserManager<IdentityUser>>();
        await CrearUsuarioAsync(usuarios, "Administrador", "admin@hotel.test", "Admin#2026", Roles.Administrador);
        await CrearUsuarioAsync(usuarios, "Gestor", "gestor@hotel.test", "Gestor#2026", Roles.Gestor);

        if (!await db.TiposHabitacion.AnyAsync(ct))
        {
            var sencilla = new TipoHabitacion("Sencilla", 2500.00m, TipoEstrategia.Plana);
            var doble = new TipoHabitacion("Doble", 3800.00m, TipoEstrategia.FinDeSemana);
            db.TiposHabitacion.AddRange(sencilla, doble);
            db.Habitaciones.AddRange(
                new Habitacion(sencilla, "101"),
                new Habitacion(sencilla, "102"),
                new Habitacion(doble, "201"),
                new Habitacion(doble, "202", activa: false));
            db.Clientes.AddRange(
                new Cliente("María Reyes", "maria@example.invalid", "DOC-DEMO-01"),
                new Cliente("Luis Peña", "luis@example.invalid", "DOC-DEMO-02"),
                new Cliente("Sofía Díaz", "sofia@example.invalid", "DOC-DEMO-03"));
            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task CrearUsuarioAsync(UserManager<IdentityUser> usuarios, string nombre, string email, string clave, string rol)
    {
        if (await usuarios.FindByEmailAsync(email) is not null) return;

        var usuario = new IdentityUser { UserName = nombre, Email = email, EmailConfirmed = true };
        var resultado = await usuarios.CreateAsync(usuario, clave);
        if (!resultado.Succeeded)
            throw new InvalidOperationException(
                $"No se pudo crear el usuario {email}: {string.Join("; ", resultado.Errors.Select(e => e.Description))}");

        await usuarios.AddToRoleAsync(usuario, rol);
    }
}
