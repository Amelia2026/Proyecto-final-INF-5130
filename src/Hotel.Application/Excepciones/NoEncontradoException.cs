using Hotel.Domain.Excepciones;

namespace Hotel.Application.Excepciones;

/// <summary>El recurso pedido no existe. La API la traduce a HTTP 404.</summary>
public class NoEncontradoException : ReglaNegocioException
{
    public NoEncontradoException(string entidad, int id)
        : base("NO_ENCONTRADO", $"{entidad} {id} no existe.") { }
}
