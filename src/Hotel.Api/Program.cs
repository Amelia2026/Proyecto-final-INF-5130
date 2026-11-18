using System.Text;
using System.Text.Json.Serialization;
using Hotel.Api.Auth;
using Hotel.Api.Errores;
using Hotel.Application.Reservas;
using Hotel.Infrastructure;
using Hotel.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1) Base de datos, Identity y repositorios (todo lo de Infrastructure)
var cadena = builder.Configuration.GetConnectionString("Hotel")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'Hotel' en appsettings.json.");
builder.Services.AgregarInfraestructura(cadena);

// 2) Casos de uso de Application
builder.Services.AddScoped<ReservaServicio>();

// 3) Seguridad: JWT
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOpciones>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en appsettings.json.");
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<GeneradorDeToken>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false; // dejamos los nombres de claims tal cual ("role", "name")
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emisor,
            ValidateAudience = true,
            ValidAudience = jwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
            ValidateLifetime = true,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

// 4) Controllers (JSON en camelCase; los enums viajan como texto: "Reservada", "Tarjeta"...)
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

// 5) Errores como application/problem+json
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorDeExcepciones>();

// 6) CORS: solo el origen exacto del frontend
var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(o => o.AddPolicy("Frontend", p => p
    .WithOrigins(origenes)
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
else app.UseHttpsRedirection();

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// En desarrollo: crea la base desde las migraciones, los roles, los usuarios y los datos de ejemplo.
if (app.Environment.IsDevelopment())
    await DatosIniciales.SembrarAsync(app.Services);

app.Run();
