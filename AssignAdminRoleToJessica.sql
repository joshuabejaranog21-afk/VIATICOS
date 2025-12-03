-- Script para asignar rol Admin a JESSICA
-- Fecha: 2025-12-01

PRINT '========================================';
PRINT 'Asignando rol Admin a JESSICA';
PRINT '========================================';
PRINT '';

-- Obtener el UserId de JESSICA
DECLARE @JessicaUserId INT = (SELECT UserId FROM Users WHERE Username = 'JESSICA');

-- Verificar si existe el usuario
IF @JessicaUserId IS NULL
BEGIN
    PRINT 'ERROR: No se encontró el usuario JESSICA';
END
ELSE
BEGIN
    PRINT 'Usuario JESSICA encontrado con UserId: ' + CAST(@JessicaUserId AS VARCHAR(10));

    -- Obtener el RoleId de Admin
    DECLARE @AdminRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Admin');

    -- Verificar si el rol existe
    IF @AdminRoleId IS NULL
    BEGIN
        PRINT 'ERROR: No se encontró el rol Admin';
        PRINT 'Creando el rol Admin...';

        INSERT INTO Roles (RoleName, Description, IsActive, CreatedAt)
        VALUES ('Admin', 'Administrador del Sistema', 1, GETDATE());

        SET @AdminRoleId = SCOPE_IDENTITY();
        PRINT 'Rol Admin creado con RoleId: ' + CAST(@AdminRoleId AS VARCHAR(10));
    END
    ELSE
    BEGIN
        PRINT 'Rol Admin encontrado con RoleId: ' + CAST(@AdminRoleId AS VARCHAR(10));
    END

    -- Verificar si ya tiene el rol asignado
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
    PRINT '=== Verificación de Roles ===';

    -- Mostrar todos los roles del usuario
    SELECT
        u.UserId,
        u.Username,
        u.Email,
        r.RoleName,
        r.Description,
        ur.AssignedAt
    FROM Users u
    INNER JOIN UserRoles ur ON u.UserId = ur.UserId
    INNER JOIN Roles r ON ur.RoleId = r.RoleId
    WHERE u.Username = 'JESSICA';

    PRINT '';
    PRINT '========================================';
    PRINT 'IMPORTANTE:';
    PRINT '- Cierra sesión (logout)';
    PRINT '- Vuelve a iniciar sesión como JESSICA';
    PRINT '- Ahora tendrás acceso al dashboard de administrador';
    PRINT '========================================';
END
