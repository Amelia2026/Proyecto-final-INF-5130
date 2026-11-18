# Ejecutar desde INF5130.TF02.BE, DESPUÉS de copiar las carpetas src y tests del zip.
$ErrorActionPreference = "Stop"

# EF Core para SQL Server (se descarga de internet)
dotnet add src/Hotel.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer

# Quitamos la prueba vacía de la plantilla, que ya no hace falta en Application
Remove-Item tests/Hotel.Application.Tests/UnitTest1.cs -ErrorAction SilentlyContinue

dotnet build
dotnet test
