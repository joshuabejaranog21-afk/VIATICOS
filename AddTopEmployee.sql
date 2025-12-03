-- Script para agregar el empleado TOP a la base de datos
-- Fecha: 2025-12-01

-- Insertar el empleado TOP
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
    'EMP006',                           -- EmployeeCode
    'TOP',                              -- FirstName
    'Usuario',                          -- LastName
    'top@empresa.com',                  -- Email
    1,                                  -- DepartmentId (ajustar según necesidad)
    'Empleado TOP',                     -- Position
    1000.00,                            -- DailyExpenseLimit
    15000.00,                           -- MonthlyExpenseLimit
    1,                                  -- IsActive (1 = true)
    GETDATE()                           -- CreatedAt
);

-- Obtener el ID del empleado recién insertado
DECLARE @NewEmployeeId INT = SCOPE_IDENTITY();

-- Mostrar el ID del nuevo empleado
SELECT @NewEmployeeId AS 'Nuevo EmployeeId para TOP';

-- Actualizar el usuario TOP con el nuevo EmployeeId
UPDATE Users
SET EmployeeId = @NewEmployeeId,
    UpdatedAt = GETDATE()
WHERE Username = 'TOP' OR Email = '1220593@alumno.um.edu.mx';

-- Verificar que el usuario fue actualizado
SELECT UserId, Username, Email, EmployeeId, FirstName, LastName
FROM Users
WHERE Username = 'TOP';

SELECT 'Usuario TOP actualizado exitosamente con EmployeeId: ' + CAST(@NewEmployeeId AS VARCHAR(10)) AS Mensaje;

-- Verificar que el empleado fue creado correctamente
SELECT * FROM Employees WHERE EmployeeCode = 'EMP006';
