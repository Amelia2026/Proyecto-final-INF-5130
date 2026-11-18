namespace Hotel.Application.Abstracciones;

/// <summary>Da la fecha de "hoy" en Santo Domingo. Es una interfaz para poder fingir la fecha en las pruebas.</summary>
public interface IReloj
{
    DateOnly Hoy();
}
