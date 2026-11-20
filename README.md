# INF-5130 · TF02 · Sistema de Reservaciones de Hotel

Proyecto final de Laboratorio de Lenguaje de Programación I (UASD). Solución en C# y .NET 10 para administrar habitaciones por tipo, reservar por noches y registrar la llegada (check-in) y la salida (check-out) de los huéspedes. Cada tipo de habitación tiene su estrategia de tarifa (Plana o FinDeSemana) y el check-out registra un único pago por el importe pactado.

## Arquitectura

```text
src/
  Hotel.Domain/          Entidades, reglas de negocio, estrategias de tarifa y excepciones de dominio
  Hotel.Application/     Casos de uso (reservar, cancelar, check-in, check-out, catálogos, tablero)
  Hotel.Infrastructure/  EF Core, SQL Server, repositorios, migraciones y datos iniciales
  Hotel.Api/             API REST con ASP.NET Core Identity, JWT y roles
  Hotel.Web/             Web en Razor Pages (ver "Estado del proyecto")
tests/
  Hotel.Domain.Tests/
  Hotel.Application.Tests/
  Hotel.Infrastructure.Tests/
database/                Scripts SQL del profesor (00 a 03)
Evidencias/              Capturas de pruebas, compilación y autorización
```

Las dependencias van hacia adentro: Api e Infrastructure dependen de Application, y Application depende de Domain. Domain no depende de ningún otro proyecto.

## Requisitos

- SDK de .NET 10.
- SQL Server local accesible como `localhost` con autenticación de Windows (SQL Server Express o Developer).
- Visual Studio (opcional) o cualquier terminal con `dotnet`.

## Reconstruir y ejecutar

### 1. Compilar

```powershell
dotnet build INF5130.TF02.Hotel.slnx
```

### 2. Base de datos de la aplicación

La API usa la base `INF5130_TF02_App` (cadena de conexión `Hotel` en `src/Hotel.Api/appsettings.json`). **No hay que crearla a mano:** en desarrollo, al arrancar, la API aplica las migraciones de EF Core, crea las tablas de negocio y de Identity, los dos roles, las cuentas de prueba y los datos de ejemplo. Se puede arrancar varias veces: solo agrega lo que falta.

### 3. Certificado de desarrollo (una sola vez)

```powershell
dotnet dev-certs https --trust
```

### 4. Iniciar la API

```powershell
cd src\Hotel.Api
dotnet run --launch-profile https
```

La API queda en `https://localhost:7001` (y `http://localhost:5001`). La descripción de los endpoints está en `https://localhost:7001/openapi/v1.json`.

> Se recomienda iniciar la API desde la terminal. En el equipo de desarrollo, ejecutarla desde Visual Studio con el perfil `http` la dejó escuchando en el puerto 5000 y no en el 5001 que declara `launchSettings.json`; desde la terminal los puertos son los esperados.

### 5. Cuentas de prueba

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | `admin@tf02.example.invalid` | `TF02-Demo` |
| Gestor | `gestor@tf02.example.invalid` | `TF02-Demo` |

Son cuentas de demostración para desarrollo. La clave de firma del JWT en `appsettings.json` también es solo de desarrollo y debe cambiarse en producción.

### 6. Frontend de demostración del profesor (Blazor WebAssembly)

El frontend lo suministra el profesor y no forma parte de este repositorio. Para conectarlo a esta API:

1. En `Hotel.Frontend/wwwroot/appsettings.json` use `"UseMockData": false` y `"ApiBaseUrl": "https://localhost:7001/"`.
2. Con la API iniciada, desde la carpeta `Frontend`:

```powershell
dotnet run --project .\Hotel.Frontend --urls http://localhost:5080
```

3. Abra `http://localhost:5080` e inicie sesión con una de las cuentas de prueba.

La API solo acepta (CORS) los orígenes `http://localhost:5080` y `https://localhost:7080`, configurados en `Cors:Origenes` de `appsettings.json`.

### 7. Pruebas automatizadas

```powershell
dotnet test INF5130.TF02.Hotel.slnx
```

Hay 37 pruebas xUnit: 25 de Domain, 10 de Application y 2 de Infrastructure.

Las 2 pruebas de Infrastructure (`MapeoTests`) son de solo lectura y usan la base `INF5130_TF02`, la del script del profesor. Para que existan, ejecute antes los scripts de `database/` en orden (`00_CreateDatabase.sql` a `03_VerificationQueries.sql`) desde SSMS o `sqlcmd`.

### 8. Recorrido automático de la API

Con la API iniciada, en otra terminal:

```powershell
powershell -ExecutionPolicy Bypass -File .\probar-api.ps1
```

El script comprueba seguridad (401 sin token, 403 del Gestor), validaciones, solapamiento de reservas, check-in, check-out y rechazo del check-out repetido. **Escribe datos de demostración** en la base: crea y borra un tipo de habitación, y deja una reserva finalizada con su pago.

## Roles y permisos

| Operación | Administrador | Gestor |
|---|:---:|:---:|
| Consultar tipos, habitaciones y clientes | Sí | Sí |
| Crear, editar y borrar tipos, habitaciones y clientes | Sí | No (403) |
| Reservar, cancelar, check-in y check-out | Sí | Sí |
| Consultar el dashboard | Sí | Sí |

Sin JWT, las rutas protegidas devuelven 401.

## Reglas de negocio principales

- Estadía de 1 a 30 noches. La entrada se incluye y la salida se excluye, por lo que las reservas contiguas son válidas.
- No se permiten reservas solapadas ni en habitaciones fuera de servicio.
- Tarifa Plana: precio base por noche. FinDeSemana: +20 % las noches de viernes y sábado, redondeado por noche a dos decimales. El importe se calcula al reservar y se conserva aunque cambie la tarifa después.
- Solo una reserva en estado Reservada puede cancelarse o hacer check-in. Solo una estadía activa admite check-out.
- El check-out cierra la estadía y crea un único pago por el importe pactado.

Los errores se devuelven como `application/problem+json` con un código estable (por ejemplo `HABITACION_NO_DISPONIBLE`, `TRANSICION_INVALIDA`, `DUPLICADO`, `RECURSO_EN_USO`, `VALIDACION`, `NO_ENCONTRADO`).

## Evidencias

La carpeta `Evidencias/` contiene:

- Resultado de las 37 pruebas xUnit superadas.
- Recompilación de la solución: 8 proyectos correctos, 0 errores.
- Salida de `probar-api.ps1`: 21 pasos con el código HTTP esperado (autorización por rol, validaciones y flujo de reserva).

## Estado del proyecto

- Implementado y probado: Domain, Application, Infrastructure, API REST con JWT y roles, pruebas xUnit.
- En desarrollo: la Web en Razor Pages (`Hotel.Web`) con acceso, Inicio, Habitaciones, Clientes y Reservas. Actualmente el proyecto contiene la plantilla inicial.
