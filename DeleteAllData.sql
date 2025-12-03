-- ============================================
-- Script para BORRAR TODO de las tablas
-- ¡CUIDADO! Este script elimina TODOS los datos
-- ============================================

PRINT '========================================';
PRINT 'ELIMINANDO TODOS LOS DATOS...';
PRINT '========================================';
PRINT '';

-- Desactivar todas las restricciones
ALTER TABLE ProductAliases NOCHECK CONSTRAINT ALL;
ALTER TABLE Products NOCHECK CONSTRAINT ALL;
ALTER TABLE ExpenseItems NOCHECK CONSTRAINT ALL;
ALTER TABLE BusinessPolicies NOCHECK CONSTRAINT ALL;
ALTER TABLE CategoryKeywords NOCHECK CONSTRAINT ALL;
ALTER TABLE CategorizationLog NOCHECK CONSTRAINT ALL;
ALTER TABLE Categories NOCHECK CONSTRAINT ALL;

-- 1. Eliminar TODOS los ProductAliases
PRINT '1. Eliminando TODOS los ProductAliases...';
DELETE FROM ProductAliases;
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' aliases eliminados';

-- 2. Eliminar TODOS los ExpenseItems
PRINT '2. Eliminando TODOS los ExpenseItems...';
DELETE FROM ExpenseItems;
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' expense items eliminados';

-- 3. Eliminar TODOS los Products
PRINT '3. Eliminando TODOS los Products...';
DELETE FROM Products;
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' productos eliminados';

-- 4. Eliminar TODOS los CategoryKeywords
PRINT '4. Eliminando TODOS los CategoryKeywords...';
DELETE FROM CategoryKeywords;
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' keywords eliminados';

-- 5. Eliminar TODAS las BusinessPolicies
PRINT '5. Eliminando TODAS las BusinessPolicies...';
DELETE FROM BusinessPolicies;
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' políticas eliminadas';

-- 6. Eliminar TODOS los CategorizationLog
PRINT '6. Eliminando TODOS los CategorizationLog...';
DELETE FROM CategorizationLog;
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' logs eliminados';

-- 7. Eliminar TODAS las Categories
PRINT '7. Eliminando TODAS las Categories...';
DELETE FROM Categories;
PRINT CAST(@@ROWCOUNT AS VARCHAR) + ' categorías eliminadas';

-- Reactivar todas las restricciones
ALTER TABLE ProductAliases CHECK CONSTRAINT ALL;
ALTER TABLE Products CHECK CONSTRAINT ALL;
ALTER TABLE ExpenseItems CHECK CONSTRAINT ALL;
ALTER TABLE BusinessPolicies CHECK CONSTRAINT ALL;
ALTER TABLE CategoryKeywords CHECK CONSTRAINT ALL;
ALTER TABLE CategorizationLog CHECK CONSTRAINT ALL;
ALTER TABLE Categories CHECK CONSTRAINT ALL;

-- Reset de identity seeds (opcional, reinicia los IDs a 1)
DBCC CHECKIDENT ('ProductAliases', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('Categories', RESEED, 0);
DBCC CHECKIDENT ('CategoryKeywords', RESEED, 0);
DBCC CHECKIDENT ('BusinessPolicies', RESEED, 0);

PRINT '';
PRINT '========================================';
PRINT 'TODOS LOS DATOS ELIMINADOS!';
PRINT '========================================';
