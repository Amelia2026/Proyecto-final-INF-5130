using Hotel.Application.Abstracciones;

namespace Hotel.Infrastructure.Servicios;

public class RelojSantoDomingo : IReloj
{
    private static readonly TimeZoneInfo Zona = BuscarZona();

    private static TimeZoneInfo BuscarZona()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo"); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SA Western Standard Time"); // nombre en Windows
        }
    }

    public DateOnly Hoy() => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.UtcNow, Zona));
}
