-- Script para asociar el usuario admin con el empleado admin
-- Fecha: 2025-12-01
-- Este script actualiza el EmployeeId del usuario admin existente

PRINT '========================================';
PRINT 'Actualizando EmployeeId del usuario admin';
PRINT '========================================';
PRINT '';

-- Obtener el EmployeeId del empleado admin (código EMP000)
DECLARE @AdminEmployeeId INT = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'EMP000');

-- Verificar si existe el empleado admin
IF @AdminEmployeeId IS NULL
BEGIN
    PRINT 'ERROR: No se encontró el empleado admin (EMP000)';
    PRINT 'Ejecuta primero el script CreateAdminUser.sql para crear el empleado admin';
END
ELSE
BEGIN
    PRINT 'Empleado admin encontrado con EmployeeId: ' + CAST(@AdminEmployeeId AS VARCHAR(10));

    -- Actualizar el usuario admin
    UPDATE Users
    SET EmployeeId = @AdminEmployeeId,
        UpdatedAt = GETDATE()
    WHERE Username = 'admin';

    PRINT 'Usuario admin actualizado exitosamente';
    PRINT '';

    -- Verificar la actualización
    PRINT '=== Verificación ===';
    SELECT
        u.UserId,
        u.Username,
        u.Email,
        u.EmployeeId,
        e.EmployeeCode,
        e.FirstName + ' ' + e.LastName AS EmpleadoNombre,
        u.IsActive
    FROM Users u
    LEFT JOIN Employees e ON u.EmployeeId = e.EmployeeId
    WHERE u.Username = 'admin';

    PRINT '';
    PRINT '========================================';
    PRINT 'IMPORTANTE:';
    PRINT '- Cierra sesión si estás logueado';
    PRINT '- Vuelve a iniciar sesión para que se actualicen los claims';
    PRINT '- Ahora podrás acceder al dashboard de administrador';
    PRINT '========================================';
END
