namespace Hotel.Application.Seguridad;

/// <summary>Los dos roles del sistema. "Ambos" se usa en los [Authorize] de las rutas que ambos pueden usar.</summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Gestor = "Gestor";
    public const string Ambos = Administrador + "," + Gestor;
}
