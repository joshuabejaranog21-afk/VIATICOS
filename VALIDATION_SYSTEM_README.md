# 📱💻 Sistema de Validación de Productos en Tiempo Real

Sistema de validación de gastos empresariales con conexión QR entre dispositivos móviles y PC, utilizando **SignalR** para comunicación en tiempo real y **Claude Vision API** para análisis inteligente de productos.

---

## 🎯 Características Principales

✅ **Conexión QR**: Escanea código QR desde el móvil para conectarte instantáneamente
✅ **Tiempo Real**: SignalR sincroniza imágenes y resultados entre dispositivos
✅ **Claude Vision AI**: Análisis inteligente de productos con Claude 3.5 Sonnet
✅ **Búsqueda por Keywords**: Sistema de fallback usando base de datos local
✅ **UI Moderna**: Interfaz responsive y optimizada para PC y móvil
✅ **Historial de Sesión**: Tracking de todas las validaciones realizadas

---

## 📦 Paquetes NuGet Requeridos

Ejecuta los siguientes comandos en la consola de Package Manager o terminal:

### Para `OneCardExpenseValidator.API`:
```bash
dotnet add package Microsoft.AspNetCore.SignalR --version 1.1.0
dotnet add package QRCoder --version 1.4.3
```

### Para `OneCardExpenseValidator.Application`:
```bash
cd OneCardExpenseValidator.Application
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
dotnet add package System.Net.Http.Json --version 8.0.0
```

---

## ⚙️ Configuración

### 1. **Configurar Claude API Key**

Edita `appsettings.json` y agrega tu API key de Claude:

```json
"Claude": {
  "ApiKey": "sk-ant-api03-XXXXXXXXXXXXXXXXX",
  "Model": "claude-3-5-sonnet-20241022",
  "MaxTokens": 1024,
  "Temperature": 0.7
}
```

📌 **¿Cómo obtener tu API Key?**
1. Ve a [https://console.anthropic.com/](https://console.anthropic.com/)
2. Crea una cuenta o inicia sesión
3. Ve a "API Keys" y genera una nueva clave
4. Copia la clave y pégala en `appsettings.json`

### 2. **Configurar Base URL**

Si tu aplicación corre en un puerto diferente, actualiza:

```json
"AppSettings": {
  "BaseUrl": "http://localhost:5190"
}
```

### 3. **Base de Datos**

Asegúrate de que la base de datos esté configurada y que exista la tabla `CategoryKeywords` con datos:

```sql
-- La tabla CategoryKeywords debe tener estos campos:
-- CategoryKeywordId, CategoryId, Keyword, IsActive, Priority

-- Ejemplo de datos recomendados:
INSERT INTO CategoryKeywords (CategoryId, Keyword, IsActive, Priority)
VALUES
  (1, 'AGUA', 1, 10),
  (1, 'CAFE', 1, 10),
  (2, 'CABLE', 1, 8),
  (2, 'CARGADOR', 1, 8),
  (3, 'UBER', 1, 9),
  (3, 'TAXI', 1, 9);
```

---

## 🚀 Uso del Sistema

### **Flujo Completo**

#### **Paso 1: Admin abre la vista de PC**

1. Ejecuta la aplicación:
   ```bash
   dotnet run --project OneCardExpenseValidator.API
   ```

2. Abre en tu navegador:
   ```
   http://localhost:5190/Validation/Admin
   ```

3. Se generará automáticamente:
   - ✅ Una sesión única
   - ✅ Un código QR en pantalla
   - ✅ Conexión activa de SignalR

#### **Paso 2: Empleado escanea QR desde su móvil**

1. Abre la cámara de tu teléfono
2. Escanea el código QR mostrado en la PC
3. Se abrirá automáticamente la página móvil
4. Verás "✅ Conectado" en ambos dispositivos

#### **Paso 3: Captura y validación**

1. 📷 **En el móvil**: Toca "Tomar Foto"
2. 📸 Toma una foto del producto
3. ✅ Toca "Validar Producto"
4. ⏳ El sistema analiza con Claude AI
5. 📊 **En la PC**: Se muestra la imagen y resultado en tiempo real
6. 📱 **En el móvil**: Se muestra resultado simplificado

#### **Paso 4: Resultados**

**Si es DEDUCIBLE (fondo verde):**
- ✅ DEDUCIBLE
- Nombre del producto
- Categoría identificada
- Razón por la que es deducible
- Nivel de confianza

**Si NO es DEDUCIBLE (fondo rojo):**
- ❌ NO DEDUCIBLE
- Nombre del producto
- Categoría identificada
- Razón por la que NO es deducible
- Nivel de confianza

---

## 📁 Estructura de Archivos Creados

```
OneCardExpenseValidator/
│
├── OneCardExpenseValidator.Application/
│   ├── DTOs/
│   │   └── ValidationDtos.cs              # DTOs para validación
│   ├── Services/
│   │   └── CategorizationService.cs       # Servicio con Claude API
│   └── Hubs/
│       └── ValidationHub.cs               # Hub de SignalR
│
├── OneCardExpenseValidator.API/
│   ├── Controllers/
│   │   ├── ValidationController.cs        # Controlador MVC (vistas)
│   │   └── API/
│   │       └── ValidationController.cs    # Controlador API REST
│   ├── Views/
│   │   └── Validation/
│   │       ├── Admin.cshtml               # Vista PC/Admin
│   │       └── Mobile.cshtml              # Vista Móvil
│   └── wwwroot/
│       ├── js/
│       │   ├── validation-admin.js        # JavaScript Admin
│       │   └── validation-mobile.js       # JavaScript Móvil
│       └── css/
│           └── validation-styles.css      # Estilos completos
│
└── Program.cs                              # Configuración actualizada
```

---

## 🔧 Endpoints de la API

### **Crear Sesión**
```http
POST /api/validation/session/create
```
**Response:**
```json
{
  "sessionId": "abc123...",
  "qrCodeBase64": "iVBORw0KGgo...",
  "mobileUrl": "http://localhost:5190/Validation/Mobile?session=abc123",
  "status": "Created",
  "createdAt": "2025-12-02T10:00:00Z",
  "expiresAt": "2025-12-02T10:10:00Z"
}
```

### **Obtener Sesión**
```http
GET /api/validation/session/{sessionId}
```

### **Analizar Producto**
```http
POST /api/validation/analyze
Content-Type: application/json

{
  "sessionId": "abc123...",
  "imageBase64": "data:image/jpeg;base64,/9j/4AAQ...",
  "description": "Cable USB-C"
}
```

**Response:**
```json
{
  "validationId": "xyz789...",
  "productName": "Cable USB-C",
  "category": "Tecnología",
  "isDeductible": true,
  "confidence": 0.92,
  "reason": "Tecnología para uso laboral es deducible",
  "analysisMethod": "Claude",
  "requiresManualReview": false
}
```

---

## 📡 Eventos de SignalR

### **Eventos del Cliente → Hub**

| Método | Descripción |
|--------|-------------|
| `CreateSession(sessionId)` | Admin crea sesión |
| `JoinSession(sessionId)` | Móvil se une a sesión |
| `SendImage(sessionId, imageBase64, description)` | Móvil envía imagen |
| `GetSessionStatus(sessionId)` | Consultar estado |
| `CloseSession(sessionId)` | Cerrar sesión |

### **Eventos del Hub → Cliente**

| Evento | Descripción |
|--------|-------------|
| `SessionCreated` | Sesión creada exitosamente |
| `JoinedSession` | Móvil unido a sesión |
| `MobileConnected` | Móvil conectado (notifica a admin) |
| `MobileDisconnected` | Móvil desconectado |
| `ImageReceived` | Imagen recibida (notifica a admin) |
| `ValidationResult` | Resultado del análisis |
| `Error` | Error en operación |
| `SessionClosed` | Sesión cerrada |

---

## 🎨 Características de UI

### **Vista Admin (PC)**

- 🔲 Código QR grande y visible
- 📊 Estado de conexión en tiempo real
- 📸 Preview de imagen recibida
- ✅/❌ Resultado con colores (verde/rojo)
- 📋 Historial de validaciones
- 📈 Estadísticas de sesión

### **Vista Móvil**

- 📱 Interfaz optimizada para móvil
- 📷 Acceso directo a cámara nativa
- 🔄 Preview antes de enviar
- ⚡ Feedback instantáneo
- 🎨 Gradientes modernos
- 💚/❤️ Resultados coloridos

---

## 🐛 Troubleshooting

### **Error: "Claude API Key no configurada"**
✅ **Solución**: Agrega tu API key en `appsettings.json` → `Claude:ApiKey`

### **Error: "Sesión no encontrada"**
✅ **Solución**: El código QR expiró (10 minutos). Genera uno nuevo con "Nueva Sesión"

### **Error: "No se pudo conectar al servidor"**
✅ **Solución**: Verifica que:
1. La aplicación esté corriendo
2. El firewall no bloquee el puerto
3. CORS esté configurado correctamente

### **Error: "SignalR no se conecta"**
✅ **Solución**:
1. Verifica que el Hub esté mapeado: `app.MapHub<ValidationHub>("/validationHub")`
2. Revisa la consola del navegador (F12)
3. Verifica que SignalR esté agregado: `builder.Services.AddSignalR()`

### **Claude API retorna error 401**
✅ **Solución**: API key inválida o expirada. Genera una nueva en console.anthropic.com

### **Claude API retorna error 429**
✅ **Solución**: Has excedido el límite de requests. Espera o actualiza tu plan en Anthropic

---

## 📊 Políticas de Deducibilidad

### ✅ **DEDUCIBLE**
- Agua embotellada y café básico
- Material de oficina (papelería, folders)
- Tecnología < $5,000 MXN (cables, cargadores, memorias USB)
- Transporte laboral (Uber, taxi, gasolina)
- Comidas de negocios (no restaurantes de lujo)

### ❌ **NO DEDUCIBLE**
- Alcohol de cualquier tipo
- Restaurantes caros o de lujo
- Entretenimiento personal (cine, videojuegos)
- Artículos de lujo o personales
- Comida rápida (tacos, hamburguesas, snacks)
- Tecnología costosa (> $5,000 MXN)

---

## 🔐 Seguridad

- ✅ Sesiones expiran en 10 minutos
- ✅ IDs únicos con GUID
- ✅ Validación de sessionId en cada request
- ✅ CORS configurado correctamente
- ✅ API key de Claude en appsettings (no en código)

---

## 📈 Próximas Mejoras (Opcional)

- [ ] Persistir sesiones en Redis para escalabilidad
- [ ] Agregar autenticación de usuarios en sesiones
- [ ] Implementar compresión de imágenes antes de enviar
- [ ] Agregar soporte para múltiples imágenes por producto
- [ ] Dashboard de analytics y reportes
- [ ] Notificaciones push
- [ ] Modo offline con sincronización posterior

---

## 📞 Soporte

Si encuentras problemas:
1. Revisa los logs en la consola
2. Verifica que todos los paquetes NuGet estén instalados
3. Asegúrate de que la base de datos esté actualizada
4. Revisa que el API key de Claude sea válido

---

## ✨ ¡Listo para Usar!

Ahora tienes un sistema completo de validación de productos en tiempo real.

**Para probarlo:**
```bash
dotnet run --project OneCardExpenseValidator.API
```

Luego abre:
- 💻 **PC**: `http://localhost:5190/Validation/Admin`
- 📱 **Móvil**: Escanea el QR generado

¡Disfruta validando productos con IA! 🚀
