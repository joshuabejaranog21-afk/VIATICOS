-- Script para crear usuario Administrador
-- Fecha: 2025-12-01
-- Contraseña por defecto: Admin123!
-- IMPORTANTE: Cambia la contraseña después del primer login

-- ========================================
-- PASO 1: Crear empleado para el Admin
-- ========================================

-- Verificar si ya existe el empleado admin
IF NOT EXISTS (SELECT 1 FROM Employees WHERE EmployeeCode = 'EMP000')
BEGIN
    INSERT INTO Employees (
        EmployeeCode,
        FirstName,
        LastName,
        Email,
        DepartmentId,
        Position,
        DailyExpenseLimit,
        MonthlyExpenseLimit,
        IsActive,
        CreatedAt
    )
    VALUES (
        'EMP000',                           -- EmployeeCode
        'Administrador',                    -- FirstName
        'Sistema',                          -- LastName
        'admin@empresa.com',                -- Email
        1,                                  -- DepartmentId (ajustar según necesidad)
        'Administrador del Sistema',        -- Position
        10000.00,                           -- DailyExpenseLimit (mayor que empleados normales)
        100000.00,                          -- MonthlyExpenseLimit (mayor que empleados normales)
        1,                                  -- IsActive (1 = true)
        GETDATE()                           -- CreatedAt
    );

    PRINT 'Empleado administrador creado exitosamente';
END
ELSE
BEGIN
    PRINT 'El empleado administrador ya existe';
END

-- Obtener el EmployeeId del admin
DECLARE @AdminEmployeeId INT = (SELECT EmployeeId FROM Employees WHERE EmployeeCode = 'EMP000');

-- ========================================
-- PASO 2: Crear usuario Admin
-- ========================================

-- Hash generado para la contraseña "Admin123!"
-- Generado con GeneratePasswordHash.ps1 el 2025-12-01
DECLARE @PasswordHash VARCHAR(MAX) = 'GKhr1XEJ+tkLP1EA4IKGvW+EZy9YJXZXKVnwKZ0fhaDi5bxvm2EepL6zDTI4ZWNfgzEefEhGtmkuZcALxbxsZg==';
DECLARE @PasswordSalt VARCHAR(MAX) = 'WBcs2kGYNE9P1kcEcKdxQLjIHBs7C6niQS/79967kf5OBlWADcr0QrEJ+EC97VgIWYdGMYcHu+o5m5u3isiJ0CapAFqefMbAH4VEeqiC6mm75jbkABdQ3/5U0LLTi0r59q8AJxoq8Y+qYpqOTypI6R0pdyUEFgWoAhQizEyifxQ=';

-- Verificar si ya existe el usuario admin
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Users (
        EmployeeId,
        Username,
        Email,
        PasswordHash,
        PasswordSalt,
        FirstName,
        LastName,
        IsActive,
        CreatedAt,
        UpdatedAt
    )
    VALUES (
        @AdminEmployeeId,                   -- EmployeeId
        'admin',                            -- Username
        'admin@empresa.com',                -- Email
        @PasswordHash,                      -- PasswordHash
        @PasswordSalt,                      -- PasswordSalt
        'Administrador',                    -- FirstName
        'Sistema',                          -- LastName
        1,                                  -- IsActive
        GETDATE(),                          -- CreatedAt
        GETDATE()                           -- UpdatedAt
    );

    PRINT 'Usuario administrador creado exitosamente';
END
ELSE
BEGIN
    PRINT 'El usuario administrador ya existe';
END

-- Obtener el UserId del admin
DECLARE @AdminUserId INT = (SELECT UserId FROM Users WHERE Username = 'admin');

-- ========================================
-- PASO 3: Asignar rol de Admin
-- ========================================

-- Obtener el RoleId de Admin
DECLARE @AdminRoleId INT = (SELECT RoleId FROM Roles WHERE RoleName = 'Admin');

-- Verificar si el rol existe
IF @AdminRoleId IS NULL
BEGIN
    -- Crear el rol Admin si no existe
    INSERT INTO Roles (RoleName, Description, IsActive, CreatedAt)
    VALUES ('Admin', 'Administrador del Sistema', 1, GETDATE());

    SET @AdminRoleId = SCOPE_IDENTITY();
    PRINT 'Rol Admin creado';
END

-- Asignar el rol al usuario si no lo tiene
IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
BEGIN
    INSERT INTO UserRoles (
        UserId,
        RoleId,
        AssignedAt
    )
    VALUES (
        @AdminUserId,
        @AdminRoleId,
        GETDATE()
    );

    PRINT 'Rol Admin asignado al usuario';
END
ELSE
BEGIN
    PRINT 'El usuario ya tiene el rol Admin';
END

-- ========================================
-- VERIFICACIÓN FINAL
-- ========================================

PRINT '';
PRINT '=== Verificación de Credenciales ===';
SELECT
    u.UserId,
    u.Username,
    u.Email,
    u.FirstName,
    u.LastName,
    u.EmployeeId,
    e.EmployeeCode,
    u.IsActive
FROM Users u
LEFT JOIN Employees e ON u.EmployeeId = e.EmployeeId
WHERE u.Username = 'admin';

PRINT '';
PRINT '=== Roles Asignados ===';
SELECT
    r.RoleName,
    r.Description,
    ur.AssignedAt
FROM UserRoles ur
INNER JOIN Roles r ON ur.RoleId = r.RoleId
WHERE ur.UserId = @AdminUserId;

PRINT '';
PRINT '========================================';
PRINT 'CREDENCIALES DE ACCESO:';
PRINT 'Usuario: admin';
PRINT 'Contraseña: Admin123!';
PRINT '';
PRINT 'IMPORTANTE: Cambia la contraseña después del primer login';
PRINT '========================================';
