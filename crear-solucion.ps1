# Ejecutar UNA vez, desde la carpeta TF02 (la que contiene src/ y tests/).
# Crea la solución con los 5 proyectos + 3 de pruebas y las referencias de Clean Architecture.
# Los proyectos Hotel.Domain y Hotel.Domain.Tests ya vienen hechos: no se recrean.

$ErrorActionPreference = "Stop"
dotnet new sln -n INF5130.TF02.Hotel

dotnet new classlib -n Hotel.Application    -o src/Hotel.Application    -f net10.0
dotnet new classlib -n Hotel.Infrastructure -o src/Hotel.Infrastructure -f net10.0
dotnet new webapi   -n Hotel.Api            -o src/Hotel.Api            -f net10.0 --use-controllers
dotnet new razor    -n Hotel.Web            -o src/Hotel.Web            -f net10.0
dotnet new xunit    -n Hotel.Application.Tests    -o tests/Hotel.Application.Tests    -f net10.0
dotnet new xunit    -n Hotel.Infrastructure.Tests -o tests/Hotel.Infrastructure.Tests -f net10.0

Remove-Item src/Hotel.Application/Class1.cs, src/Hotel.Infrastructure/Class1.cs -ErrorAction SilentlyContinue

# Regla de dependencias: todo apunta hacia Domain
dotnet add src/Hotel.Application    reference src/Hotel.Domain
dotnet add src/Hotel.Infrastructure reference src/Hotel.Application
dotnet add src/Hotel.Api            reference src/Hotel.Application src/Hotel.Infrastructure
dotnet add src/Hotel.Web            reference src/Hotel.Application
dotnet add tests/Hotel.Application.Tests    reference src/Hotel.Application src/Hotel.Domain
dotnet add tests/Hotel.Infrastructure.Tests reference src/Hotel.Infrastructure src/Hotel.Application src/Hotel.Domain

Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object { dotnet sln add $_.FullName }

dotnet build
dotnet test
