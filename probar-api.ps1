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
        return [pscustomobject]@{ Estado = [int]$r.StatusCode; Datos = ($r.Content | ConvertFrom-Json) }
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

Paso "Listar reservas SIN token" (Llamar GET "/api/reservas") 401

$admin = Llamar POST "/api/auth/login" @{ email = "admin@hotel.test"; contrasena = "Admin#2026" }
Paso "Login Administrador" $admin 200
$gestor = Llamar POST "/api/auth/login" @{ email = "gestor@hotel.test"; contrasena = "Gestor#2026" }
Paso "Login Gestor" $gestor 200
Paso "Login con clave incorrecta" (Llamar POST "/api/auth/login" @{ email = "admin@hotel.test"; contrasena = "mala" }) 401

$tA = $admin.Datos.accessToken
$tG = $gestor.Datos.accessToken

Paso "Gestor lista reservas" (Llamar GET "/api/reservas" $null $tG) 200

$nueva = @{ habitacionId = 3; clienteId = 1; entrada = $hoy; salida = $salida }
$r1 = Llamar POST "/api/reservas" $nueva $tA
Paso "Reservar hab. 201 (hoy a +2 dias)" $r1 201
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
