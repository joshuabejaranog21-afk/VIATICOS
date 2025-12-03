#!/bin/bash
# ============================================
# Script de instalación de paquetes NuGet
# Sistema de Validación de Productos en Tiempo Real
# ============================================

echo "============================================"
echo "Instalando paquetes NuGet necesarios..."
echo "============================================"
echo ""

# Navegar al directorio del proyecto API
echo "📦 Instalando paquetes en OneCardExpenseValidator.API..."
cd OneCardExpenseValidator.API

# Instalar QRCoder para generación de códigos QR
echo "  - Instalando QRCoder..."
dotnet add package QRCoder --version 1.4.3

# Instalar SignalR (si no está incluido en .NET 8)
echo "  - Verificando Microsoft.AspNetCore.SignalR..."
dotnet add package Microsoft.AspNetCore.SignalR --version 1.1.0

echo "  ✅ Paquetes de API instalados"
echo ""

# Navegar al directorio del proyecto Application
cd ..
echo "📦 Instalando paquetes en OneCardExpenseValidator.Application..."
cd OneCardExpenseValidator.Application

# Instalar System.Net.Http.Json para llamadas HTTP
echo "  - Instalando System.Net.Http.Json..."
dotnet add package System.Net.Http.Json --version 8.0.0

echo "  ✅ Paquetes de Application instalados"
echo ""

# Regresar al directorio raíz
cd ..

echo "============================================"
echo "✅ Todos los paquetes instalados exitosamente"
echo "============================================"
echo ""

echo "📋 Siguientes pasos:"
echo "  1. Configura tu Claude API Key en appsettings.json"
echo "  2. Ejecuta: dotnet build"
echo "  3. Ejecuta: dotnet run --project OneCardExpenseValidator.API"
echo "  4. Abre: http://localhost:5190/Validation/Admin"
echo ""
echo "📖 Lee VALIDATION_SYSTEM_README.md para instrucciones completas"
echo ""
