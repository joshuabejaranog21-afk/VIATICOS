# 🚀 INICIO RÁPIDO - Sistema de Validación

## ⚡ Instalación en 3 Pasos

### **Paso 1: Instalar Paquetes NuGet**

**Windows:**
```powershell
.\install-packages.ps1
```

**Linux/Mac:**
```bash
chmod +x install-packages.sh
./install-packages.sh
```

**O manualmente:**
```bash
# En el proyecto API
cd OneCardExpenseValidator.API
dotnet add package QRCoder --version 1.4.3
dotnet add package Microsoft.AspNetCore.SignalR --version 1.1.0

# En el proyecto Application
cd ../OneCardExpenseValidator.Application
dotnet add package System.Net.Http.Json --version 8.0.0
cd ..
```

---

### **Paso 2: Configurar Claude API Key**

1. Ve a [https://console.anthropic.com/](https://console.anthropic.com/)
2. Crea una cuenta o inicia sesión
3. Genera una nueva API Key
4. Edita `OneCardExpenseValidator.API/appsettings.json`:

```json
{
  "Claude": {
    "ApiKey": "sk-ant-api03-XXXXXXXXXXXXXXXXX"  // ⬅️ Pega tu API Key aquí
  }
}
```

---

### **Paso 3: Ejecutar la Aplicación**

```bash
dotnet build
dotnet run --project OneCardExpenseValidator.API
```

Abre en tu navegador:
```
http://localhost:5190/Validation/Admin
```

---

## 📱 Cómo Usar

### **En la PC (Admin):**
1. Abre `http://localhost:5190/Validation/Admin`
2. Verás un código QR en pantalla
3. Espera a que el móvil se conecte

### **En el Móvil:**
1. Escanea el código QR con la cámara de tu teléfono
2. Se abrirá automáticamente la página móvil
3. Toca "📷 Tomar Foto"
4. Toma una foto del producto
5. Toca "✅ Validar Producto"

### **Resultados:**
- **PC**: Muestra análisis completo con imagen
- **Móvil**: Muestra resultado simple (✅ DEDUCIBLE o ❌ NO DEDUCIBLE)

---

## ✅ Verificar Instalación

### **1. Verificar que los paquetes estén instalados:**
```bash
dotnet list package
```

Deberías ver:
- `QRCoder` (1.4.3 o superior)
- `Microsoft.AspNetCore.SignalR` (si aparece)
- `System.Net.Http.Json`

### **2. Verificar compilación:**
```bash
dotnet build
```

Debe compilar sin errores.

### **3. Verificar que SignalR esté configurado:**
Busca en `Program.cs`:
```csharp
builder.Services.AddSignalR();  // ✅ Debe existir
app.MapHub<ValidationHub>("/validationHub");  // ✅ Debe existir
```

---

## 🐛 Problemas Comunes

### **Error: "Claude API Key no configurada"**
❌ Falta configurar el API key
✅ Agrega tu API key en `appsettings.json` → `Claude:ApiKey`

### **Error al compilar: "QRCoder no encontrado"**
❌ Paquete no instalado
✅ Ejecuta: `dotnet add package QRCoder --version 1.4.3`

### **Error: "ValidationHub not found"**
❌ Falta el using en Program.cs
✅ Agrega: `using OneCardExpenseValidator.Application.Hubs;`

### **El móvil no se conecta**
❌ CORS o firewall bloqueando
✅ Verifica que CORS esté habilitado en Program.cs
✅ Verifica que el móvil esté en la misma red

### **Claude retorna error 401**
❌ API key inválida
✅ Genera una nueva API key en console.anthropic.com

### **Claude retorna error 429**
❌ Límite de requests excedido
✅ Espera unos minutos o actualiza tu plan en Anthropic

---

## 📊 Políticas Rápidas

### ✅ **SÍ es deducible:**
- Agua, café básico
- Material de oficina
- Tecnología < $5,000 MXN
- Transporte laboral

### ❌ **NO es deducible:**
- Alcohol
- Restaurantes caros
- Entretenimiento personal
- Comida rápida (tacos, hamburguesas)

---

## 📖 Documentación Completa

Para más detalles, consulta:
- `VALIDATION_SYSTEM_README.md` - Documentación completa
- `PRODUCT_VALIDATION_README.md` - Sistema de productos

---

## 🎯 URLs Importantes

- **Vista Admin (PC)**: http://localhost:5190/Validation/Admin
- **API Crear Sesión**: http://localhost:5190/api/validation/session/create
- **API Analizar**: http://localhost:5190/api/validation/analyze
- **SignalR Hub**: ws://localhost:5190/validationHub
- **Swagger**: http://localhost:5190/swagger

---

## ✨ ¡Listo!

Ya puedes validar productos con IA en tiempo real 🚀

**Prueba con estos productos:**
- ✅ Cable USB-C → Deducible
- ✅ Agua embotellada → Deducible
- ❌ Taco de carne → No deducible
- ❌ Cerveza → No deducible
- ✅ Libreta → Deducible
- ✅ Uber → Deducible

---

📞 **¿Necesitas ayuda?** Revisa los logs en la consola o consulta el README completo.
