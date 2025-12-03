# Script de PowerShell para generar hash de contraseña
# Compatible con OneCardExpenseValidator

param(
    [string]$Password = "Admin123!"
)

# Crear HMACSHA512
$hmac = New-Object System.Security.Cryptography.HMACSHA512

# Generar salt
$saltBytes = $hmac.Key
$salt = [Convert]::ToBase64String($saltBytes)

# Generar hash
$passwordBytes = [System.Text.Encoding]::UTF8.GetBytes($Password)
$hashBytes = $hmac.ComputeHash($passwordBytes)
$hash = [Convert]::ToBase64String($hashBytes)

# Mostrar resultados
Write-Host "`n=== Hash de Contraseña Generado ===" -ForegroundColor Green
Write-Host "Contraseña: $Password" -ForegroundColor Yellow
Write-Host "`nPasswordHash:" -ForegroundColor Cyan
Write-Host $hash
Write-Host "`nPasswordSalt:" -ForegroundColor Cyan
Write-Host $salt
Write-Host "`n=== Instrucciones ===" -ForegroundColor Green
Write-Host "1. Copia los valores de PasswordHash y PasswordSalt"
Write-Host "2. Reemplaza los valores en CreateAdminUser.sql (líneas 50-51)"
Write-Host "3. Ejecuta el script CreateAdminUser.sql en SQL Server"

# Limpiar
$hmac.Dispose()
