# Ejecutar desde INF5130.TF02.BE, DESPUÉS de copiar la carpeta src del zip.
# Instala paquetes, quita archivos de ejemplo de la plantilla y crea la migración de EF Core.
$ErrorActionPreference = "Stop"
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"

dotnet add src/Hotel.Infrastructure package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/Hotel.Api package Microsoft.EntityFrameworkCore.Design
dotnet add src/Hotel.Api package Microsoft.AspNetCore.Authentication.JwtBearer

Remove-Item src/Hotel.Api/WeatherForecast.cs -ErrorAction SilentlyContinue
Remove-Item src/Hotel.Api/Controllers/WeatherForecastController.cs -ErrorAction SilentlyContinue

dotnet build
if ($LASTEXITCODE -ne 0) { throw "La compilación falló. Copia el primer error rojo y envíalo." }

dotnet tool update --global dotnet-ef
dotnet ef migrations add InicialConIdentity --project src/Hotel.Infrastructure --startup-project src/Hotel.Api --output-dir Persistencia/Migraciones
if ($LASTEXITCODE -ne 0) { throw "No se pudo crear la migración. Copia el mensaje y envíalo." }

dotnet build
Write-Host "Listo. Ahora arranca la API con: dotnet run --project src/Hotel.Api --launch-profile http"
