# Sistema de Validación de Productos para OneCard Expense Validator

## Descripción General

Este sistema permite al administrador agregar productos del recibo a un ticket de gastos y validar automáticamente si son deducibles según las políticas de la empresa.

## Características Implementadas

### 1. Búsqueda Inteligente de Productos
- **Endpoint**: `GET /api/products/search?query={texto}`
- **Funcionalidad**:
  - Busca productos por nombre, marca y aliases
  - Soporta diferentes descripciones del mismo producto (ej: "AGUA BONAFON" vs "BONAFON 1.5L")
  - Calcula un score de relevancia para ordenar resultados
  - Devuelve los 10 mejores resultados

### 2. Validación Automática de Deducibilidad
- **Endpoint**: `POST /api/products/validate`
- **Funcionalidad**:
  - Verifica si el producto es deducible según su categoría
  - Valida contra políticas de negocio activas
  - Verifica límites de monto (diario, mensual, máximo)
  - Identifica si requiere aprobación del gerente
  - Devuelve mensajes de validación detallados

### 3. Agregar Items con Validación
- **Endpoint**: `POST /api/expenseitems/add-with-validation`
- **Funcionalidad**:
  - Agrega un producto al ticket
  - Valida automáticamente según políticas
  - Actualiza totales deducibles/no deducibles del ticket
  - Registra notas de validación

### 4. Interfaz de Usuario Mejorada

#### Vista de Detalles del Ticket (`/ExpenseTickets/Details/{id}`)

**Botón "Agregar Producto"** (solo visible para Admins en tickets pendientes):
- Despliega un formulario para ingresar productos

**Formulario de Agregar Producto**:
1. **Campo de búsqueda**: Escribe la descripción del producto tal como aparece en el recibo
2. **Autocomplete en tiempo real**: Muestra resultados mientras escribes (mínimo 2 caracteres)
3. **Resultados de búsqueda**: Incluyen:
   - Nombre del producto y marca
   - Aliases coincidentes
   - Badge de Deducible/No Deducible
   - Categoría
4. **Campos de cantidad y precio**:
   - Cantidad (default: 1)
   - Precio unitario
   - Total (calculado automáticamente)
5. **Validación automática**: Al ingresar precio, valida automáticamente y muestra:
   - ✓ Aprobado / ✗ Rechazado
   - Información del producto y categoría
   - Mensajes de validación (límites excedidos, etc.)
6. **Botón "Agregar al Ticket"**: Se habilita solo cuando el producto está validado

**Tabla de Items Mejorada**:
- Muestra producto, marca, descripción original del recibo
- Estado: Deducible / No Deducible
- Notas de validación
- Categoría asignada

## Flujo de Trabajo

### Para el Administrador:

1. **Abrir un ticket pendiente**
   - Ir a `/ExpenseTickets/Details/{id}`
   - Ver la imagen del recibo en el panel derecho

2. **Agregar productos del recibo**
   - Clic en "Agregar Producto"
   - Escribir la descripción tal como aparece en el recibo (ej: "COCA 600ML")
   - Seleccionar el producto correcto de los resultados
   - Ingresar cantidad y precio del recibo
   - El sistema valida automáticamente
   - Ver resultado de validación (Aprobado/Rechazado)
   - Clic en "Agregar al Ticket"

3. **Revisar todos los productos**
   - La tabla muestra todos los productos agregados
   - Ver cuáles son deducibles y cuáles no
   - Ver notas de validación

4. **Aprobar o Rechazar el ticket**
   - Basado en la validación automática
   - Usar los botones "Aprobar" o "Rechazar"

## Beneficios del Sistema

### 1. Soluciona el Problema de Descripciones Variadas
- **Antes**: "AGUA BONAFON" en una tienda vs "BONAFON 1.5 LITROS" en otra
- **Ahora**: El sistema encuentra el producto correcto usando aliases
- **Ejemplo**:
  ```
  Producto: Agua Bonafont 1.5L
  Aliases:
    - AGUA BONAFON
    - BONAFONT 1.5L
    - AGUA BONAFONT 1.5 LITROS
  ```

### 2. Validación Automática contra Políticas
- Verifica límites de monto automáticamente
- Identifica productos no deducibles
- Alerta sobre aprobaciones requeridas
- Registra razones de rechazo

### 3. Eficiencia Mejorada
- Búsqueda en tiempo real (300ms debounce)
- Autocomplete inteligente
- Cálculo automático de totales
- Actualización automática de montos deducibles/no deducibles

### 4. Trazabilidad
- Registra la descripción original del recibo
- Mapea a productos estandarizados
- Guarda notas de validación
- Mantiene historial de cambios

## Estructura de Base de Datos

### Tablas Principales:
- **Products**: Catálogo de productos con nombre estándar
- **ProductAliases**: Diferentes nombres para el mismo producto
- **ExpenseItems**: Items del ticket con validación
- **BusinessPolicies**: Políticas de negocio por categoría
- **Categories**: Categorías con reglas de deducibilidad

### Campos Clave en ExpenseItems:
- `ItemDescription`: Nombre estándar del producto
- `OriginalDescription`: Texto original del recibo
- `ProductId`: Referencia al producto
- `CategoryId`: Categoría asignada
- `IsDeductible`: Si es deducible o no
- `PolicyValidation`: Estado (Approved/Rejected/Pending)
- `ValidationNotes`: Razones de aprobación/rechazo

## Ejemplo de Uso

### Escenario: Agregar "COCA 600ML" del recibo

1. Admin escribe "COCA 600"
2. Sistema busca y encuentra:
   ```
   Coca-Cola 600ml - Coca-Cola
   Categoría: Bebidas
   Deducible: No
   ```
3. Admin selecciona el producto
4. Ingresa: Cantidad=2, Precio Unit.=$15.00
5. Total calculado: $30.00
6. Sistema valida automáticamente:
   ```
   ✗ Rechazado
   Producto: Coca-Cola 600ml - Coca-Cola
   Categoría: Bebidas
   Deducible: No
   Notas: Las bebidas no son deducibles según política de la empresa
   ```
7. Admin agrega al ticket de todos modos (para registro)
8. El ticket muestra $30.00 como No Deducible

## APIs Disponibles

### 1. Búsqueda de Productos
```http
GET /api/products/search?query=coca
```
**Respuesta**:
```json
[
  {
    "productId": 1,
    "productName": "Coca-Cola 600ml",
    "brand": "Coca-Cola",
    "sku": "COCA600",
    "categoryName": "Bebidas",
    "categoryId": 5,
    "isDeductible": false,
    "matchedAliases": ["COCA 600", "COCA COLA 600ML"],
    "score": 95
  }
]
```

### 2. Validar Producto
```http
POST /api/products/validate
Content-Type: application/json

{
  "productId": 1,
  "amount": 30.00,
  "hasReceipt": true
}
```
**Respuesta**:
```json
{
  "productId": 1,
  "productName": "Coca-Cola 600ml",
  "brand": "Coca-Cola",
  "categoryName": "Bebidas",
  "categoryId": 5,
  "isDeductible": false,
  "requiresApproval": false,
  "validationMessages": [
    "Las bebidas no son deducibles según política de la empresa"
  ],
  "policyApplied": null,
  "status": "Rechazado"
}
```

### 3. Agregar Item con Validación
```http
POST /api/expenseitems/add-with-validation
Content-Type: application/json

{
  "ticketId": 1,
  "productId": 1,
  "itemDescription": "Coca-Cola 600ml",
  "originalDescription": "COCA 600",
  "quantity": 2,
  "unitPrice": 15.00,
  "totalPrice": 30.00
}
```
**Respuesta**:
```json
{
  "itemId": 123,
  "productName": "Coca-Cola 600ml",
  "brand": "Coca-Cola",
  "categoryName": "Bebidas",
  "isDeductible": false,
  "policyValidation": "Rejected",
  "validationMessages": [
    "Las bebidas no son deducibles según política de la empresa"
  ],
  "status": "Rejected"
}
```

## Notas Técnicas

### Frontend:
- **JavaScript vanilla** (no requiere frameworks)
- **Debouncing** de 300ms para búsquedas
- **Autocomplete** con cierre al hacer click fuera
- **Validación en tiempo real**
- **Cálculo automático** de totales

### Backend:
- **ASP.NET Core 8.0** con Entity Framework
- **SQL Server** como base de datos
- **Fuzzy matching** por score de relevancia
- **Validación de políticas** en tiempo real

### Seguridad:
- Solo usuarios con rol **Admin** pueden agregar productos
- Solo en tickets con estado **Pending**
- Validación anti-XSS con `escapeHtml()`
- Validación server-side de todas las operaciones

## Próximos Pasos Sugeridos

1. **Agregar ProductAliases**: Llenar la tabla con variantes comunes
2. **Configurar BusinessPolicies**: Definir límites por categoría
3. **Entrenar el sistema**: Usar productos reales y sus variantes
4. **OCR Integration**: Extraer productos automáticamente del recibo
5. **Machine Learning**: Mejorar matching con ML

---

**Fecha de implementación**: 2025-12-01
**Desarrollado para**: OneCard Expense Validator
**Hackathon Feature**: Sistema de validación automática de productos
