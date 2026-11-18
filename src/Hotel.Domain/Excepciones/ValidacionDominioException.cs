namespace Hotel.Domain.Excepciones;

/// <summary>Datos inválidos (fechas, tarifa, formato). La API la traduce a HTTP 400.</summary>
public class ValidacionDominioException : ReglaNegocioException
{
    public ValidacionDominioException(string mensaje) : base("VALIDACION", mensaje) { }
}
