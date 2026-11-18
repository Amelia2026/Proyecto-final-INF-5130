using System.Text.RegularExpressions;
using Hotel.Domain.Excepciones;

namespace Hotel.Domain.Entidades;

public class Cliente
{
    private static readonly Regex FormatoEmail =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Documento { get; private set; } = string.Empty;

    private Cliente() { } // requerido por EF Core

    public Cliente(string nombre, string email, string documento)
        => Actualizar(nombre, email, documento);

    public void Actualizar(string nombre, string email, string documento)
    {
        nombre = (nombre ?? string.Empty).Trim();
        email = (email ?? string.Empty).Trim();
        documento = (documento ?? string.Empty).Trim();

        if (nombre.Length == 0) throw new ValidacionDominioException("El nombre es obligatorio.");
        if (documento.Length == 0) throw new ValidacionDominioException("El documento es obligatorio.");
        if (!FormatoEmail.IsMatch(email)) throw new ValidacionDominioException("El correo no tiene un formato válido.");

        Nombre = nombre;
        Email = email;
        Documento = documento;
    }
}
