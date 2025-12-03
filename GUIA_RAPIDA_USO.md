# 🚀 Guía Rápida de Uso - Sistema de Validación de Productos

## 📋 Paso 1: Poblar la Base de Datos

Antes de usar el sistema, ejecuta el script SQL para agregar productos y aliases:

```sql
-- Ejecutar en SQL Server Management Studio o Azure Data Studio
-- Ruta: SeedProductsAndAliases.sql
```

Este script creará:
- ✅ 6 Categorías (Bebidas, Alimentos, Papelería, Transporte, Tecnología, Servicios)
- ✅ 11 Productos comunes
- ✅ 40+ Aliases para búsqueda inteligente

---

## 🎯 Paso 2: Flujo de Trabajo Completo

### Como Administrador:

#### 1️⃣ Ver Tickets Pendientes
- Ve a: **`/ExpenseTickets`** (Dashboard de Admin)
- Verás todos los tickets con estados: Pending, Approved, Rejected
- Filtra por estado si es necesario

#### 2️⃣ Abrir un Ticket Pendiente
- Click en "Detalles" de un ticket pendiente
- Verás:
  - 📄 Información del ticket (empleado, fecha, vendor, monto)
  - 🖼️ **Imagen del recibo** (panel derecho)
  - 📦 Lista de productos (inicialmente vacía)

#### 3️⃣ Agregar Productos del Recibo

**Paso a paso con ejemplo real:**

Supón que tienes este recibo de Soriana:
```
SORIANA
30/11/2025

POLLO ROST          $125.00
TARJETA TORNETO     $250.00
AGUA BONAFON        $20.00
```

**Para cada producto:**

1. **Click en "Agregar Producto"**
   - Se abre el formulario

2. **Escribir exactamente como aparece en el recibo**
   - Ejemplo: `POLLO ROST`
   - Espera 300ms, aparece autocomplete

3. **Seleccionar del autocomplete**
   ```
   ✅ Pollo Rostizado - Soriana
      Categoría: Alimentos
      Badge: Deducible
   ```

4. **Ingresar cantidad y precio**
   - Cantidad: `1`
   - Precio Unitario: `125.00`
   - Total: `$125.00` (calculado automáticamente)

5. **Ver validación automática**
   ```
   ✓ Aprobado
   Producto: Pollo Rostizado - Soriana
   Categoría: Alimentos
   Deducible: Sí
   ```

6. **Click en "Agregar al Ticket"**
   - Se agrega a la tabla
   - Página se recarga

7. **Repetir para los demás productos**

---

## 📊 Ejemplo Completo: Recibo de Soriana

### Recibo Original:
```
SORIANA
30/11/2025
Ticket: 123456789

POLLO ROST          $125.00
TARJETA TORNETO     $250.00
AGUA BONAFON 2      $ 40.00
TOTAL               $415.00
```

### Proceso de Validación:

#### Producto 1: Pollo Rostizado
1. Escribir: `POLLO ROST`
2. Sistema encuentra: **Pollo Rostizado** (Alimentos)
3. Ingresar: Cantidad=1, Precio=$125.00
4. **Resultado**: ✅ **APROBADO** - Deducible
5. Agregar al ticket

#### Producto 2: Tarjetas Torneto
1. Escribir: `TARJETA TORNETO`
2. Sistema encuentra: **Tarjetas Torneto** (Papelería)
3. Ingresar: Cantidad=1, Precio=$250.00
4. **Resultado**: ✅ **APROBADO** - Deducible
5. Agregar al ticket

#### Producto 3: Agua Bonafont
1. Escribir: `AGUA BONAFON`
2. Sistema encuentra: **Agua Bonafont 1.5L** (Bebidas)
3. Ingresar: Cantidad=2, Precio=$20.00 (2 botellas)
4. **Resultado**: ❌ **RECHAZADO** - No Deducible
   - Nota: "Las bebidas no son deducibles según política de la empresa"
5. Agregar al ticket de todas formas (para registro)

### Resultado Final:

**Tabla de Items del Ticket:**

| Producto | Categoría | Cant. | Precio Unit. | Total | Estado | Notas |
|----------|-----------|-------|--------------|-------|--------|-------|
| Pollo Rostizado<br>Soriana<br><small>Del recibo: POLLO ROST</small> | Alimentos | 1 | $125.00 | $125.00 | <span class="badge bg-success">Deducible</span> | |
| Tarjetas Torneto<br>Torneto<br><small>Del recibo: TARJETA TORNETO</small> | Papelería | 1 | $250.00 | $250.00 | <span class="badge bg-success">Deducible</span> | |
| Agua Bonafont 1.5L<br>Bonafont<br><small>Del recibo: AGUA BONAFON 2</small> | Bebidas | 2 | $10.00 | $20.00 | <span class="badge bg-danger">No Deducible</span> | No deducible según política |

**Totales del Ticket:**
- Monto Total: $415.00
- Deducible: $375.00
- No Deducible: $40.00

---

## 🗑️ Eliminar Productos

Si agregaste un producto por error:

1. **Click en el botón 🗑️** en la columna "Acciones"
2. Confirmar eliminación
3. El producto se elimina y los totales se actualizan automáticamente

**Nota**: Solo puedes eliminar productos en tickets **Pendientes**

---

## ✅ Aprobar o Rechazar el Ticket

Una vez que hayas agregado todos los productos:

### Aprobar:
1. Click en **"Aprobar"**
2. Seleccionar quién aprueba
3. Confirmar
4. El ticket cambia a estado **"Approved"**

### Rechazar:
1. Click en **"Rechazar"**
2. Escribir razón del rechazo
3. Confirmar
4. El ticket cambia a estado **"Rejected"**

---

## 🔍 Casos Especiales

### 1. Producto No Encontrado
Si escribes algo y no aparece en el autocomplete:
- Verifica la ortografía
- Intenta con menos palabras (ej: "COCA" en vez de "COCA COLA 600ML")
- Si no existe, necesitas agregarlo a la base de datos

### 2. Múltiples Resultados
Si aparecen varios productos similares:
- Lee las descripciones completas
- Verifica la marca
- Selecciona el que mejor coincida con el recibo

### 3. Producto con Límite Excedido
Ejemplo: Comida de $600 cuando el límite es $500
```
❌ Rechazado
Producto: Comida Restaurante
Categoría: Alimentos
Deducible: No
Notas:
- Excede el monto máximo permitido de $500.00
```

---

## 🎨 Interfaz Visual

### Panel Izquierdo:
- 📝 Información del Ticket
- ➕ Botón "Agregar Producto"
- 📋 Tabla de Items
- ✅ Botones Aprobar/Rechazar

### Panel Derecho:
- 🖼️ Imagen del Recibo (para referencia visual)
- ℹ️ Información Adicional (fechas)

---

## 💡 Tips y Mejores Prácticas

### ✅ HACER:
- ✅ Revisar la imagen del recibo mientras agregas productos
- ✅ Copiar exactamente la descripción del recibo
- ✅ Verificar cantidades y precios dos veces
- ✅ Agregar todos los productos antes de aprobar/rechazar
- ✅ Revisar los totales finales

### ❌ NO HACER:
- ❌ No adivinar productos si no estás seguro
- ❌ No aprobar sin verificar todos los items
- ❌ No editar tickets ya aprobados/rechazados
- ❌ No omitir productos no deducibles (agrégalos para registro)

---

## 📱 Atajos de Teclado

- **Enter** en búsqueda: Selecciona primer resultado
- **Esc**: Cierra autocomplete
- **Tab**: Navega entre campos
- **Ctrl + R**: Recarga página (después de agregar)

---

## 🆘 Solución de Problemas

### Problema: "No se encontraron productos"
**Solución**:
- Verifica que ejecutaste el script SQL
- Intenta con menos texto
- Revisa que la base de datos tenga productos

### Problema: El botón "Agregar al Ticket" está deshabilitado
**Solución**:
- Selecciona un producto del autocomplete
- Ingresa cantidad > 0
- Ingresa precio > 0
- Espera a que aparezca la validación

### Problema: Error al agregar producto
**Solución**:
- Verifica conexión a la base de datos
- Revisa que el ticket esté en estado "Pending"
- Verifica que seas Admin

---

## 📊 Reportes y Estadísticas

Después de procesar tickets, puedes ver:
- Total deducible vs no deducible por departamento
- Productos más frecuentes
- Tickets pendientes de revisión
- Historial de aprobaciones

---

## 🔐 Seguridad

- Solo usuarios con rol **Admin** pueden agregar/eliminar productos
- Solo en tickets con estado **Pending**
- Todas las acciones quedan registradas en AuditLog
- Los cambios son irreversibles después de Aprobar/Rechazar

---

## 📞 Soporte

Si tienes problemas:
1. Verifica que ejecutaste `SeedProductsAndAliases.sql`
2. Verifica que tu usuario tiene rol "Admin"
3. Revisa la consola del navegador (F12) para errores
4. Verifica los logs del servidor

---

## 🎯 Próximos Pasos

Después de dominar el sistema básico:
1. ⬆️ Agregar más productos a la base de datos
2. 🏷️ Crear más aliases para variantes comunes
3. 📜 Configurar políticas de negocio específicas
4. 🤖 (Futuro) Integración con OCR automático

---

**¡Listo! Ahora estás preparado para usar el sistema de validación de productos.** 🎉
