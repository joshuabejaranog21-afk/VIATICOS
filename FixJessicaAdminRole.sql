-- Script para asignar el rol Admin (RoleId 3) a JESSICA
-- Fecha: 2025-12-01

PRINT '========================================';
PRINT 'Asignando rol Admin a JESSICA';
PRINT '========================================';
PRINT '';

-- Obtener el UserId de JESSICA
DECLARE @JessicaUserId INT = (SELECT UserId FROM Users WHERE Username = 'JESSICA');

IF @JessicaUserId IS NULL
BEGIN
    PRINT 'ERROR: No se encontró el usuario JESSICA';
END
ELSE
BEGIN
    PRINT 'Usuario JESSICA encontrado con UserId: ' + CAST(@JessicaUserId AS VARCHAR(10));

    -- Obtener el RoleId del rol "Admin" (RoleId 3)
    DECLARE @AdminRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Admin');

    IF @AdminRoleId IS NULL
    BEGIN
        PRINT 'ERROR: No se encontró el rol Admin';
    END
    ELSE
    BEGIN
        PRINT 'Rol Admin encontrado con RoleId: ' + CAST(@AdminRoleId AS VARCHAR(10));

        -- Verificar si ya tiene el rol Admin asignado
        IF EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @JessicaUserId AND RoleId = @AdminRoleId)
        BEGIN
            PRINT 'El usuario JESSICA ya tiene el rol Admin asignado';
        END
        ELSE
        BEGIN
            -- Asignar el rol Admin
            INSERT INTO UserRoles (
                UserId,
                RoleId,
                AssignedAt
            )
            VALUES (
                @JessicaUserId,
                @AdminRoleId,
                GETDATE()
            );

            PRINT 'Rol Admin asignado exitosamente a JESSICA';
        END

        PRINT '';
        PRINT '=== Todos los Roles de JESSICA ===';
        SELECT
            u.Username,
            r.RoleId,
            r.RoleName,
            r.Description,
            ur.AssignedAt
        FROM Users u
        INNER JOIN UserRoles ur ON u.UserId = ur.UserId
        INNER JOIN Roles r ON ur.RoleId = r.RoleId
        WHERE u.Username = 'JESSICA'
        ORDER BY r.RoleName;

        PRINT '';
        PRINT '========================================';
        PRINT 'IMPORTANTE:';
        PRINT '1. Cierra sesión completamente';
        PRINT '2. Cierra el navegador';
        PRINT '3. Vuelve a abrir e inicia sesión como JESSICA';
        PRINT '4. Ahora tendrás acceso al dashboard de administrador';
        PRINT '========================================';
    END
END
