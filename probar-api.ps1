# Recorre el flujo completo contra la API (debe estar corriendo en otra ventana).
# Uso: powershell -ExecutionPolicy Bypass -File .\probar-api.ps1
$ErrorActionPreference = "Stop"
$base = "http://localhost:5001"
$hoy = Get-Date -Format "yyyy-MM-dd"
$salida = (Get-Date).AddDays(2).ToString("yyyy-MM-dd")

function Llamar([string]$metodo, [string]$ruta, $cuerpo = $null, [string]$token = "") {
    $headers = @{}
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    $p = @{ Method = $metodo; Uri = "$base$ruta"; Headers = $headers; UseBasicParsing = $true }
    if ($null -ne $cuerpo) {
        $p.Body = [System.Text.Encoding]::UTF8.GetBytes(($cuerpo | ConvertTo-Json))
        $p.ContentType = "application/json; charset=utf-8"
    }
    try {
        $r = Invoke-WebRequest @p
        $datos = $null
        if ($r.Content) { $datos = $r.Content | ConvertFrom-Json }
        return [pscustomobject]@{ Estado = [int]$r.StatusCode; Datos = $datos }
    } catch {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { throw }
        $texto = ""
        try { $texto = (New-Object System.IO.StreamReader($resp.GetResponseStream())).ReadToEnd() } catch {}
        $datos = $null
        if ($texto) { try { $datos = $texto | ConvertFrom-Json } catch {} }
        return [pscustomobject]@{ Estado = [int]$resp.StatusCode; Datos = $datos }
    }
}

function Paso([string]$titulo, $r, [int]$esperado) {
    $marca = if ($r.Estado -eq $esperado) { "OK    " } else { "FALLO " }
    $extra = ""
    if ($r.Datos -and $r.Datos.codigo) { $extra = " codigo=" + $r.Datos.codigo }
    Write-Host ("{0} {1} -> HTTP {2} (esperado {3}){4}" -f $marca, $titulo, $r.Estado, $esperado, $extra)
}

Write-Host "=== Seguridad ===" -ForegroundColor Cyan
Paso "Listar reservas SIN token" (Llamar GET "/api/reservas") 401
$admin = Llamar POST "/api/auth/login" @{ email = "admin@tf02.example.invalid"; contrasena = "TF02-Demo" }
Paso "Login Administrador" $admin 200
$gestor = Llamar POST "/api/auth/login" @{ email = "gestor@tf02.example.invalid"; contrasena = "TF02-Demo" }
Paso "Login Gestor" $gestor 200
Paso "Login con clave incorrecta" (Llamar POST "/api/auth/login" @{ email = "admin@tf02.example.invalid"; contrasena = "mala" }) 401
$tA = $admin.Datos.accessToken
$tG = $gestor.Datos.accessToken

Write-Host "=== Catalogos y tablero ===" -ForegroundColor Cyan
Paso "Dashboard (Gestor)" (Llamar GET "/api/dashboard" $null $tG) 200
Paso "Gestor lista tipos" (Llamar GET "/api/tipos-habitacion" $null $tG) 200
Paso "Gestor lista habitaciones" (Llamar GET "/api/habitaciones" $null $tG) 200
Paso "Gestor lista clientes" (Llamar GET "/api/clientes" $null $tG) 200
$nombreTipo = "Suite " + (Get-Random -Maximum 99999)
$tipoNuevo = @{ nombre = $nombreTipo; tarifaBase = 9000; estrategia = "FinDeSemana" }
Paso "Gestor intenta CREAR tipo (sin permiso)" (Llamar POST "/api/tipos-habitacion" $tipoNuevo $tG) 403
$creado = Llamar POST "/api/tipos-habitacion" $tipoNuevo $tA
Paso "Admin crea tipo" $creado 201
Paso "Admin crea tipo duplicado" (Llamar POST "/api/tipos-habitacion" $tipoNuevo $tA) 409
Paso "Admin tarifa invalida (0)" (Llamar POST "/api/tipos-habitacion" @{ nombre = "Otro"; tarifaBase = 0; estrategia = "Plana" } $tA) 400
Paso "Admin borra tipo con habitaciones" (Llamar DELETE "/api/tipos-habitacion/1" $null $tA) 409
Paso "Admin borra el tipo que acaba de crear" (Llamar DELETE "/api/tipos-habitacion/$($creado.Datos.id)" $null $tA) 204

Write-Host "=== Flujo de reserva ===" -ForegroundColor Cyan
$nueva = @{ habitacionId = 2; clienteId = 1; entrada = $hoy; salida = $salida }
$r1 = Llamar POST "/api/reservas" $nueva $tA
Paso "Reservar hab. 102 (hoy a +2 dias)" $r1 201
Write-Host ("       importe = {0}" -f $r1.Datos.importe)
$id = $r1.Datos.id
Paso "Reservar el MISMO periodo (solapado)" (Llamar POST "/api/reservas" $nueva $tA) 409
Paso "Check-in" (Llamar POST "/api/reservas/$id/check-in" $null $tA) 200
Paso "Cancelar tras check-in" (Llamar POST "/api/reservas/$id/cancelacion" $null $tA) 409
$co = Llamar POST "/api/reservas/$id/check-out" @{ metodoPago = "Tarjeta" } $tG
Paso "Check-out (como Gestor)" $co 200
Write-Host ("       estado = {0}, pago = {1}" -f $co.Datos.estado, $co.Datos.pago.monto)
Paso "Check-out repetido" (Llamar POST "/api/reservas/$id/check-out" @{ metodoPago = "Efectivo" } $tG) 409
Paso "Reserva inexistente" (Llamar GET "/api/reservas/99999" $null $tA) 404
Write-Host "=== Fin ===" -ForegroundColor Cyan
