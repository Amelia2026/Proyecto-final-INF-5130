using Hotel.Domain.Excepciones;

namespace Hotel.Application.Excepciones;

/// <summary>Ya existe otro registro con ese valor único (número, nombre, correo, documento). HTTP 409.</summary>
public class DuplicadoException : ReglaNegocioException
{
    public DuplicadoException(string entidad, string campo, string valor)
        : base("DUPLICADO", $"Ya existe {entidad} con {campo} '{valor}'.") { }
}
