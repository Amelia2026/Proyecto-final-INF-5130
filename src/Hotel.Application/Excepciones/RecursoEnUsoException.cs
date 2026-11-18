using Hotel.Domain.Excepciones;

namespace Hotel.Application.Excepciones;

/// <summary>El recurso tiene historial o está en uso y no se puede borrar ni modificar así. HTTP 409.</summary>
public class RecursoEnUsoException : ReglaNegocioException
{
    public RecursoEnUsoException(string mensaje) : base("RECURSO_EN_USO", mensaje) { }
}
