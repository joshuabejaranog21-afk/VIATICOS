    -- Script de diagnóstico para verificar roles de usuario
    -- Fecha: 2025-12-01

    PRINT '========================================';
    PRINT 'DIAGNÓSTICO DE ROLES DE USUARIO';
    PRINT '========================================';
    PRINT '';

    -- Mostrar información de JESSICA
    PRINT '=== Usuario: JESSICA ===';
    SELECT
        u.UserId,
        u.Username,
        u.Email,
        u.EmployeeId,
        u.IsActive,
        e.EmployeeCode
    FROM Users u
    LEFT JOIN Employees e ON u.EmployeeId = e.EmployeeId
    WHERE u.Username = 'JESSICA';

    PRINT '';
    PRINT '=== Roles de JESSICA ===';
    SELECT
        r.RoleId,
        r.RoleName,
        r.Description,
        ur.AssignedAt
    FROM UserRoles ur
    INNER JOIN Roles r ON ur.RoleId = r.RoleId
    WHERE ur.UserId = (SELECT UserId FROM Users WHERE Username = 'JESSICA');

    PRINT '';
    PRINT '=== Todos los Roles Disponibles ===';
    SELECT * FROM Roles;

    PRINT '';
    PRINT '=== Todos los Usuarios con Rol Admin ===';
    SELECT
        u.UserId,
        u.Username,
        u.Email,
        r.RoleName,
        ur.AssignedAt
    FROM Users u
    INNER JOIN UserRoles ur ON u.UserId = ur.UserId
    INNER JOIN Roles r ON ur.RoleId = r.RoleId
    WHERE r.RoleName = 'Admin';

    PRINT '';
    PRINT '========================================';
