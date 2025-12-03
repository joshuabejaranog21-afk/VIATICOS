# OneCard Expense Validator - Vistas MVC

## Descripción
Se han creado las vistas MVC para la aplicación OneCard Expense Validator. El proyecto ahora incluye una interfaz web completa con Bootstrap 5 para gestionar tickets de gastos, empleados, departamentos y categorías.

## Estructura de Vistas Creadas

### 1. Layout Principal y Vistas Compartidas
- **_Layout.cshtml**: Layout principal con navegación Bootstrap 5
- **_ViewStart.cshtml**: Configuración del layout por defecto
- **_ViewImports.cshtml**: Importaciones de namespaces comunes
- **_ValidationScriptsPartial.cshtml**: Scripts de validación jQuery

### 2. Home (Dashboard)
**Controller:** `HomeController.cs`
**Vistas:**
- `Index.cshtml`: Dashboard principal con estadísticas y acceso rápido

### 3. Empleados (Employees)
**Controller:** `EmployeesViewController.cs`
**Ruta Base:** `/Employees`
**Vistas:**
- `Index.cshtml`: Lista de empleados con búsqueda y filtros
- `Create.cshtml`: Formulario para crear nuevo empleado
- `Edit.cshtml`: Formulario para editar empleado existente
- `Details.cshtml`: Detalles completos del empleado
- `Delete.cshtml`: Confirmación de desactivación de empleado

**Funcionalidades:**
- CRUD completo de empleados
- Asociación con departamentos
- Gestión de límites de gasto diario y mensual
- Soft delete (desactivación en lugar de eliminación)

### 4. Tickets de Gastos (ExpenseTickets)
**Controller:** `ExpenseTicketsViewController.cs`
**Ruta Base:** `/ExpenseTickets`
**Vistas:**
- `Index.cshtml`: Lista de tickets con filtros por estado (Pendiente, Aprobado, Rechazado)
- `Create.cshtml`: Formulario para crear nuevo ticket de gasto
- `Details.cshtml`: Detalles completos del ticket con items de gasto
- `Approve.cshtml`: Formulario de aprobación de ticket
- `Reject.cshtml`: Formulario de rechazo de ticket con razón

**Funcionalidades:**
- CRUD completo de tickets
- Workflow de aprobación/rechazo
- Visualización de items del gasto
- Filtrado por estado y empleado
- Gestión de montos deducibles/no deducibles

### 5. Departamentos (Departments)
**Controller:** `DepartmentsViewController.cs`
**Ruta Base:** `/Departments`
**Vistas:**
- `Index.cshtml`: Lista de departamentos con conteo de empleados

**Funcionalidades:**
- Listado de departamentos
- Visualización de límites presupuestales
- Conteo de empleados por departamento

### 6. Categorías (Categories)
**Controller:** `CategoriesViewController.cs`
**Ruta Base:** `/Categories`
**Vistas:**
- `Index.cshtml`: Lista de categorías con indicadores de deducibilidad

**Funcionalidades:**
- Listado de categorías
- Indicadores visuales de deducibilidad fiscal
- Indicadores de requisitos de aprobación
- Límites de monto permitido

### 7. Autenticación (Auth)
**Controller:** `AuthViewController.cs`
**Ruta Base:** `/Auth`
**Vistas:**
- `Login.cshtml`: Página de inicio de sesión
- `Register.cshtml`: Página de registro de usuarios

**Funcionalidades:**
- Login con usuario/email y contraseña
- Registro de nuevos usuarios
- Hash de contraseñas con HMACSHA512
- Asignación automática de rol "Empleado"
- Validación de contraseñas

## Características Principales

### Diseño y UX
- **Framework CSS:** Bootstrap 5.3
- **Iconos:** Bootstrap Icons
- **Diseño Responsive:** Adaptable a móviles, tablets y desktop
- **Tema:** Moderno con gradientes y sombras

### Componentes Visuales
- Cards con sombras para mejor organización
- Badges de colores para estados (Pendiente, Aprobado, Rechazado)
- Alertas para mensajes de éxito y error
- Tablas responsivas con hover effects
- Botones con iconos para mejor UX

### Navegación
- Navbar fijo superior con logo
- Menú desplegable para catálogos
- Menú de usuario con opción de logout
- Breadcrumbs implícitos con botones "Volver"

### Validación
- Validación del lado del servidor con ModelState
- Validación del lado del cliente con jQuery Validate
- Scripts de validación no intrusivos
- Mensajes de error claros y específicos

## Configuración Realizada

### Program.cs
```csharp
// Soporte para MVC con vistas
builder.Services.AddControllersWithViews();

// Archivos estáticos (CSS, JS, imágenes)
app.UseStaticFiles();

// Routing para MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

### Routing
Los controllers de vistas utilizan routing por atributos:
- `[Route("Employees")]` para EmployeesViewController
- `[Route("ExpenseTickets")]` para ExpenseTicketsViewController
- `[Route("Departments")]` para DepartmentsViewController
- `[Route("Categories")]` para CategoriesViewController
- `[Route("Auth")]` para AuthViewController

## Cómo Usar

### 1. Ejecutar la Aplicación
```bash
cd OneCardExpenseValidator.API
dotnet run
```

### 2. Acceder a la Aplicación
Abrir el navegador en: `https://localhost:5001` o `http://localhost:5000`

### 3. Iniciar Sesión
- URL: `/Auth/Login`
- Si no tienes usuario, regístrate en: `/Auth/Register`

### 4. Navegar por las Secciones
- **Dashboard:** `/` o `/Home/Index`
- **Tickets:** `/ExpenseTickets`
- **Empleados:** `/Employees`
- **Departamentos:** `/Departments`
- **Categorías:** `/Categories`

## Funcionalidades por Implementar (Futuras Mejoras)

### Vistas Pendientes
- Edit para ExpenseTickets
- Create/Edit/Details para Departments
- Create/Edit/Details para Categories
- Vistas para Products
- Vistas para Reports

### Funcionalidades
- Autenticación con JWT o Cookies
- Autorización por roles
- Upload de imágenes de tickets
- OCR para extracción de texto de tickets
- Reportes y gráficas
- Exportación a Excel/PDF
- Notificaciones por email
- Historial de auditoría

### Mejoras de UX
- Paginación en listados
- Búsqueda avanzada
- Ordenamiento de columnas
- DataTables para tablas interactivas
- SweetAlert para confirmaciones
- Toastr para notificaciones

## Estructura de Carpetas

```
OneCardExpenseValidator.API/
├── Controllers/
│   ├── HomeController.cs
│   ├── EmployeesViewController.cs
│   ├── ExpenseTicketsViewController.cs
│   ├── DepartmentsViewController.cs
│   ├── CategoriesViewController.cs
│   └── AuthViewController.cs
├── Views/
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   └── _ValidationScriptsPartial.cshtml
│   ├── Home/
│   │   └── Index.cshtml
│   ├── Employees/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   ├── Details.cshtml
│   │   └── Delete.cshtml
│   ├── ExpenseTickets/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Details.cshtml
│   │   ├── Approve.cshtml
│   │   └── Reject.cshtml
│   ├── Departments/
│   │   └── Index.cshtml
│   ├── Categories/
│   │   └── Index.cshtml
│   ├── Auth/
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── _ViewStart.cshtml
│   └── _ViewImports.cshtml
└── wwwroot/
    └── (archivos estáticos)
```

## Notas Técnicas

### Anti-Forgery Tokens
Todos los formularios incluyen `@Html.AntiForgeryToken()` para protección contra CSRF.

### ViewBag vs ViewData
Se utiliza ViewBag para pasar datos del controller a las vistas (ej: listas para dropdowns).

### TempData
Se utiliza TempData para mensajes de éxito/error que persisten entre redirects.

### Convenciones de Nombres
- Controllers: `[Nombre]ViewController.cs`
- Vistas: Nombradas según la acción (Index, Create, Edit, etc.)
- Rutas: `/[Nombre]/[Accion]/[id?]`

## Soporte y Contacto

Para preguntas o problemas, contactar al equipo de desarrollo.

---
**Versión:** 1.0
**Fecha:** Diciembre 2025
**Framework:** ASP.NET Core 8.0
