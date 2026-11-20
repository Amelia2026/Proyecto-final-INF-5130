using Hotel.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Páginas Razor: todas exigen sesión, excepto el inicio de sesión y la página de error.
builder.Services.AddRazorPages(o =>
{
    o.Conventions.AuthorizeFolder("/");
    o.Conventions.AllowAnonymousToPage("/Login");
    o.Conventions.AllowAnonymousToPage("/Error");
});

// La sesión de la Web vive en una cookie. Dentro guardamos el JWT que entrega la API.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Login";
        o.AccessDeniedPath = "/Login";
        o.ExpireTimeSpan = TimeSpan.FromMinutes(120); // igual que MinutosExpiracion del JWT
    });
builder.Services.AddAuthorization();

// Cliente HTTP hacia la API REST. El TokenHandler le agrega el JWT a cada petición.
var urlApi = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Falta 'Api:BaseUrl' en appsettings.json.");
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<TokenHandler>();
builder.Services.AddHttpClient<ApiCliente>(c => c.BaseAddress = new Uri(urlApi))
    .AddHttpMessageHandler<TokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
