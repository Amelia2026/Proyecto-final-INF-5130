namespace Hotel.Domain.Excepciones;

/// <summary>
/// Base de las excepciones del dominio. El Codigo es estable (ej. HABITACION_NO_DISPONIBLE)
/// y la API lo devuelve en application/problem+json.
/// </summary>
public abstract class ReglaNegocioException : Exception
{
    public string Codigo { get; }

    protected ReglaNegocioException(string codigo, string mensaje) : base(mensaje)
        => Codigo = codigo;
}
