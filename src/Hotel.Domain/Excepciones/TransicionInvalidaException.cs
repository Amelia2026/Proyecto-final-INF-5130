namespace Hotel.Domain.Excepciones;

/// <summary>Cambio de estado no permitido (ej. cancelar una estadía iniciada). La API la traduce a HTTP 409.</summary>
public class TransicionInvalidaException : ReglaNegocioException
{
    public TransicionInvalidaException(string mensaje) : base("TRANSICION_INVALIDA", mensaje) { }
}
