# ============================================
# Script de instalación de paquetes NuGet
# Sistema de Validación de Productos en Tiempo Real
# ============================================

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Instalando paquetes NuGet necesarios..." -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Navegar al directorio del proyecto API
Write-Host "📦 Instalando paquetes en OneCardExpenseValidator.API..." -ForegroundColor Yellow
Set-Location -Path "OneCardExpenseValidator.API"

# Instalar QRCoder para generación de códigos QR
Write-Host "  - Instalando QRCoder..." -ForegroundColor Gray
dotnet add package QRCoder --version 1.4.3

# Instalar SignalR (si no está incluido en .NET 8)
Write-Host "  - Verificando Microsoft.AspNetCore.SignalR..." -ForegroundColor Gray
dotnet add package Microsoft.AspNetCore.SignalR --version 1.1.0

Write-Host "  ✅ Paquetes de API instalados" -ForegroundColor Green
Write-Host ""

# Navegar al directorio del proyecto Application
Set-Location -Path ".."
Write-Host "📦 Instalando paquetes en OneCardExpenseValidator.Application..." -ForegroundColor Yellow
Set-Location -Path "OneCardExpenseValidator.Application"

# Instalar System.Net.Http.Json para llamadas HTTP
Write-Host "  - Instalando System.Net.Http.Json..." -ForegroundColor Gray
dotnet add package System.Net.Http.Json --version 8.0.0

Write-Host "  ✅ Paquetes de Application instalados" -ForegroundColor Green
Write-Host ""

# Regresar al directorio raíz
Set-Location -Path ".."

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "✅ Todos los paquetes instalados exitosamente" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "📋 Siguientes pasos:" -ForegroundColor Yellow
Write-Host "  1. Configura tu Claude API Key en appsettings.json" -ForegroundColor White
Write-Host "  2. Ejecuta: dotnet build" -ForegroundColor White
Write-Host "  3. Ejecuta: dotnet run --project OneCardExpenseValidator.API" -ForegroundColor White
Write-Host "  4. Abre: http://localhost:5190/Validation/Admin" -ForegroundColor White
Write-Host ""
Write-Host "📖 Lee VALIDATION_SYSTEM_README.md para instrucciones completas" -ForegroundColor Cyan
Write-Host ""

# Pausa para que el usuario pueda ver los mensajes
Read-Host "Presiona Enter para continuar..."
