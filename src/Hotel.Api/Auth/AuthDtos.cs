namespace Hotel.Api.Auth;

/// <summary>Lo que se envía para iniciar sesión (LoginInput del contrato).</summary>
public record LoginInput(string Email, string Contrasena);

/// <summary>Lo que se devuelve al iniciar sesión (Sesion del contrato).</summary>
public record Sesion(string AccessToken, string Nombre, IReadOnlyList<string> Roles);
