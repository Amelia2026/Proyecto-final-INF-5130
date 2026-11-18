using Hotel.Application.Abstracciones;
using Hotel.Application.Seguridad;
using Hotel.Domain.Entidades;
using Hotel.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Infrastructure.Persistencia;

/// <summary>
/// Datos de desarrollo: crea la base desde las migraciones, los 2 roles, los usuarios de prueba
/// (los mismos del frontend del profe) y datos de ejemplo. Solo agrega lo que falta,
/// así que se puede ejecutar muchas veces.
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
        const string clave = "TF02-Demo";
        await CrearUsuarioAsync(usuarios, "Administrador", "admin@tf02.example.invalid", clave, Roles.Administrador);
        await CrearUsuarioAsync(usuarios, "Gestor", "gestor@tf02.example.invalid", clave, Roles.Gestor);

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

        if (!await db.Reservas.AnyAsync(ct))
            await SembrarReservasAsync(db, sp.GetRequiredService<IReloj>(), ct);
    }

    /// <summary>
    /// Historial de ejemplo creado con las MISMAS reglas del Domain: el parámetro "hoy" permite
    /// simular que la reserva se hizo en el pasado sin saltarse ninguna validación.
    /// </summary>
    private static async Task SembrarReservasAsync(HotelDbContext db, IReloj reloj, CancellationToken ct)
    {
        var hoy = reloj.Hoy();
        var habs = await db.Habitaciones.Include(h => h.TipoHabitacion).ToDictionaryAsync(h => h.Numero, ct);
        var clientes = await db.Clientes.ToDictionaryAsync(c => c.Documento, ct);

        var completo = habs.ContainsKey("101") && habs.ContainsKey("102") && habs.ContainsKey("201")
            && clientes.ContainsKey("DOC-DEMO-01") && clientes.ContainsKey("DOC-DEMO-02") && clientes.ContainsKey("DOC-DEMO-03");
        if (!completo) return;

        // 1) Finalizada con pago: hace 4 días -> hace 2 días, hab. 102 (2 noches x 2,500 = 5,000)
        var finalizada = Reserva.Crear(habs["102"], clientes["DOC-DEMO-01"], hoy.AddDays(-4), hoy.AddDays(-2), hoy.AddDays(-4));
        finalizada.RegistrarCheckIn(hoy.AddDays(-4), otraEstadiaActiva: false);
        finalizada.RegistrarCheckOut(hoy.AddDays(-2), MetodoPago.Efectivo);

        // 2) Reservada: mañana -> dentro de 3 días, hab. 101
        var reservada = Reserva.Crear(habs["101"], clientes["DOC-DEMO-02"], hoy.AddDays(1), hoy.AddDays(3), hoy);

        // 3) En estadía: ayer -> mañana, hab. 201
        var enEstadia = Reserva.Crear(habs["201"], clientes["DOC-DEMO-03"], hoy.AddDays(-1), hoy.AddDays(1), hoy.AddDays(-1));
        enEstadia.RegistrarCheckIn(hoy.AddDays(-1), otraEstadiaActiva: false);

        db.Reservas.AddRange(finalizada, reservada, enEstadia);
        await db.SaveChangesAsync(ct);
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
