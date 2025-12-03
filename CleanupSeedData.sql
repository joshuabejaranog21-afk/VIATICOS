-- Script para limpiar datos de prueba antes de re-ejecutar el seed
-- Fecha: 2025-12-01
-- Propósito: Eliminar productos, aliases y categorías creadas por el script de seed
-- TAMBIÉN elimina productos corruptos con GTIN = NULL de ejecuciones fallidas

PRINT '========================================';
PRINT 'Limpiando datos de seed...';
PRINT '========================================';
PRINT '';

-- Desactivar restricciones de clave foránea temporalmente
ALTER TABLE ProductAliases NOCHECK CONSTRAINT ALL;
ALTER TABLE Products NOCHECK CONSTRAINT ALL;
ALTER TABLE ExpenseItems NOCHECK CONSTRAINT ALL;

-- 1. Primero eliminar aliases de productos corruptos
PRINT 'Eliminando ProductAliases de productos corruptos...';
DELETE FROM ProductAliases
WHERE ProductId IN (
    SELECT ProductId FROM Products
    WHERE GTIN IS NULL OR SKU IN (
        'COCA600', 'AGUABON15', 'CAFESTB',
        'POLLOROST', 'TORTAJAM',
        'BOLIBIC', 'CUAD100', 'TARJTORN',
        'GASPREM', 'UBERVIAJE',
        'MOUSEUSB', 'CABLEHDMI',
        'HOTELNOCHE'
    )
);
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' aliases eliminados';

-- 2. Eliminar ExpenseItems relacionados a productos corruptos
PRINT 'Eliminando ExpenseItems de productos corruptos...';
DELETE FROM ExpenseItems
WHERE ProductId IN (
    SELECT ProductId FROM Products
    WHERE GTIN IS NULL OR SKU IN (
        'COCA600', 'AGUABON15', 'CAFESTB',
        'POLLOROST', 'TORTAJAM',
        'BOLIBIC', 'CUAD100', 'TARJTORN',
        'GASPREM', 'UBERVIAJE',
        'MOUSEUSB', 'CABLEHDMI',
        'HOTELNOCHE'
    )
);
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' expense items eliminados';

-- 3. Eliminar productos corruptos
PRINT 'Eliminando Products corruptos y del seed...';
DELETE FROM Products
WHERE GTIN IS NULL OR SKU IN (
    'COCA600', 'AGUABON15', 'CAFESTB',
    'POLLOROST', 'TORTAJAM',
    'BOLIBIC', 'CUAD100', 'TARJTORN',
    'GASPREM', 'UBERVIAJE',
    'MOUSEUSB', 'CABLEHDMI',
    'HOTELNOCHE'
);
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' productos eliminados';

-- 4. Eliminar categorías del seed (solo si no tienen otros productos)
PRINT 'Eliminando Categories del seed...';
DELETE FROM Categories
WHERE CategoryCode IN ('BEB', 'ALI', 'PAP', 'TRA', 'TEC', 'SER')
AND NOT EXISTS (
    SELECT 1 FROM Products WHERE DefaultCategoryId = Categories.CategoryId
);
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' categorías eliminadas';

-- Reactivar restricciones de clave foránea
ALTER TABLE ProductAliases CHECK CONSTRAINT ALL;
ALTER TABLE Products CHECK CONSTRAINT ALL;
ALTER TABLE ExpenseItems CHECK CONSTRAINT ALL;

PRINT '';
PRINT '========================================';
PRINT 'Limpieza completada!';
PRINT '========================================';
