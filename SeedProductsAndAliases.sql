-- Script para poblar productos comunes y sus aliases
-- Fecha: 2025-12-01
-- Propósito: Crear catálogo de productos con variantes de nombres para búsqueda inteligente

PRINT '========================================';
PRINT 'Poblando Productos y Aliases';
PRINT '========================================';
PRINT '';

-- ============================================
-- 1. BEBIDAS
-- ============================================
PRINT 'Insertando productos de BEBIDAS...';

DECLARE @CategoryBebidas INT = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'BEB');

-- Si no existe la categoría Bebidas, crearla
IF @CategoryBebidas IS NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryCode = 'BEB')
    BEGIN
        INSERT INTO Categories (CategoryCode, CategoryName, Description, IsDeductible, RequiresApproval, CreatedAt, UpdatedAt)
        VALUES ('BEB', 'Bebidas', 'Bebidas y refrescos', 0, 0, GETDATE(), GETDATE());
        SET @CategoryBebidas = SCOPE_IDENTITY();
        PRINT 'Categoría Bebidas creada';
    END
    ELSE
    BEGIN
        SET @CategoryBebidas = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'BEB');
        PRINT 'Categoría Bebidas ya existe, reutilizando';
    END
END

-- Coca-Cola 600ml
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'COCA600')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('COCA600', '7501234500001', 'Coca-Cola 600ml', 'Coca-Cola', @CategoryBebidas, 1, GETDATE(), GETDATE());

    DECLARE @CocaId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CocaId, 'COCA 600', 'Soriana', 1, GETDATE()),
        (@CocaId, 'COCA COLA 600ML', 'Walmart', 1, GETDATE()),
        (@CocaId, 'COCA-COLA 600', 'Bodega Aurrera', 1, GETDATE()),
        (@CocaId, 'REFRESCO COCA 600', 'OXXO', 1, GETDATE()),
        (@CocaId, 'COCA COLA PET 600', '7-Eleven', 1, GETDATE());

    PRINT 'Coca-Cola 600ml agregado con aliases';
END

-- Agua Bonafont 1.5L
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'AGUABON15')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('AGUABON15', '7501234500002', 'Agua Bonafont 1.5L', 'Bonafont', @CategoryBebidas, 1, GETDATE(), GETDATE());

    DECLARE @AguaBonId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@AguaBonId, 'AGUA BONAFON', 'Soriana', 1, GETDATE()),
        (@AguaBonId, 'BONAFONT 1.5L', 'Walmart', 1, GETDATE()),
        (@AguaBonId, 'AGUA BONAFONT 1.5 LITROS', 'Bodega Aurrera', 1, GETDATE()),
        (@AguaBonId, 'BONAFON 1.5', 'OXXO', 1, GETDATE()),
        (@AguaBonId, 'AGUA NAT BONAFONT', '7-Eleven', 1, GETDATE());

    PRINT 'Agua Bonafont 1.5L agregado con aliases';
END

-- Café Starbucks
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CAFESTB')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CAFESTB', '7501234500003', 'Café Americano Grande', 'Starbucks', @CategoryBebidas, 1, GETDATE(), GETDATE());

    DECLARE @CafeStbId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CafeStbId, 'CAFE AMERICANO G', 'Starbucks', 1, GETDATE()),
        (@CafeStbId, 'AMERICANO GRANDE', 'Starbucks', 1, GETDATE()),
        (@CafeStbId, 'CAFE GRANDE', 'Starbucks', 1, GETDATE());

    PRINT 'Café Starbucks agregado con aliases';
END

-- ============================================
-- 2. ALIMENTOS
-- ============================================
PRINT '';
PRINT 'Insertando productos de ALIMENTOS...';

DECLARE @CategoryAlimentos INT = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'ALI');

IF @CategoryAlimentos IS NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryCode = 'ALI')
    BEGIN
        INSERT INTO Categories (CategoryCode, CategoryName, Description, IsDeductible, RequiresApproval, MaxAmountAllowed, CreatedAt, UpdatedAt)
        VALUES ('ALI', 'Alimentos', 'Alimentos y comida preparada', 1, 0, 500.00, GETDATE(), GETDATE());
        SET @CategoryAlimentos = SCOPE_IDENTITY();
        PRINT 'Categoría Alimentos creada';
    END
    ELSE
    BEGIN
        SET @CategoryAlimentos = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'ALI');
        PRINT 'Categoría Alimentos ya existe, reutilizando';
    END
END

-- Pollo Rostizado
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'POLLOROST')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('POLLOROST', '7501234500004', 'Pollo Rostizado', 'Soriana', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @PolloId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@PolloId, 'POLLO ROST', 'Soriana', 1, GETDATE()),
        (@PolloId, 'POLLO ROSTIZADO', 'Walmart', 1, GETDATE()),
        (@PolloId, 'POLLO ASADO', 'HEB', 1, GETDATE()),
        (@PolloId, 'POLLO ENTERO ROSTIZADO', 'Chedraui', 1, GETDATE());

    PRINT 'Pollo Rostizado agregado con aliases';
END

-- Torta
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'TORTAJAM')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('TORTAJAM', '7501234500005', 'Torta de Jamón', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @TortaId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@TortaId, 'TORTA JAMON', 'OXXO', 1, GETDATE()),
        (@TortaId, 'TORTA DE JAMON', '7-Eleven', 1, GETDATE()),
        (@TortaId, 'TORTA JAM QUESO', 'Subway', 1, GETDATE());

    PRINT 'Torta de Jamón agregado con aliases';
END

-- Taco de Carne
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'TACOCARNE')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('TACOCARNE', '7501234500014', 'Taco de Carne Asada', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @TacoCarneId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@TacoCarneId, 'TACO CARNE', 'Taquería', 1, GETDATE()),
        (@TacoCarneId, 'TACO CARNE ASADA', 'Taquería', 1, GETDATE()),
        (@TacoCarneId, 'TACO CARNE (D)', 'La Conquista', 1, GETDATE()),
        (@TacoCarneId, 'TACOS CARNE', 'Taquería', 1, GETDATE()),
        (@TacoCarneId, 'TACO DE CARNE', 'Taquería', 1, GETDATE());

    PRINT 'Taco de Carne agregado con aliases';
END

-- Taco de Pastor
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'TACOPASTOR')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('TACOPASTOR', '7501234500015', 'Taco de Pastor', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @TacoPastorId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@TacoPastorId, 'TACO PASTOR', 'Taquería', 1, GETDATE()),
        (@TacoPastorId, 'TACO DE PASTOR', 'Taquería', 1, GETDATE()),
        (@TacoPastorId, 'TACOS PASTOR', 'Taquería', 1, GETDATE()),
        (@TacoPastorId, 'TACO PASTOR (D)', 'Taquería', 1, GETDATE());

    PRINT 'Taco de Pastor agregado con aliases';
END

-- Taco de Pollo
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'TACOPOLLO')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('TACOPOLLO', '7501234500016', 'Taco de Pollo', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @TacoPolloId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@TacoPolloId, 'TACO POLLO', 'Taquería', 1, GETDATE()),
        (@TacoPolloId, 'TACO DE POLLO', 'Taquería', 1, GETDATE()),
        (@TacoPolloId, 'TACOS POLLO', 'Taquería', 1, GETDATE());

    PRINT 'Taco de Pollo agregado con aliases';
END

-- Taco de Suadero
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'TACOSUADERO')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('TACOSUADERO', '7501234500017', 'Taco de Suadero', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @TacoSuaderoId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@TacoSuaderoId, 'TACO SUADERO', 'Taquería', 1, GETDATE()),
        (@TacoSuaderoId, 'TACO DE SUADERO', 'Taquería', 1, GETDATE()),
        (@TacoSuaderoId, 'TACOS SUADERO', 'Taquería', 1, GETDATE());

    PRINT 'Taco de Suadero agregado con aliases';
END

-- Taco de Tripa
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'TACOTRIPA')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('TACOTRIPA', '7501234500018', 'Taco de Tripa', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @TacoTripaId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@TacoTripaId, 'TACO TRIPA', 'Taquería', 1, GETDATE()),
        (@TacoTripaId, 'TACO DE TRIPA', 'Taquería', 1, GETDATE()),
        (@TacoTripaId, 'TACOS TRIPA', 'Taquería', 1, GETDATE());

    PRINT 'Taco de Tripa agregado con aliases';
END

-- Taco de Chorizo
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'TACOCHORIZO')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('TACOCHORIZO', '7501234500019', 'Taco de Chorizo', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @TacoChorizoId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@TacoChorizoId, 'TACO CHORIZO', 'Taquería', 1, GETDATE()),
        (@TacoChorizoId, 'TACO DE CHORIZO', 'Taquería', 1, GETDATE()),
        (@TacoChorizoId, 'TACOS CHORIZO', 'Taquería', 1, GETDATE());

    PRINT 'Taco de Chorizo agregado con aliases';
END

-- Pirata (Taco grande con tortilla de harina)
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'PIRATA')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('PIRATA', '7501234500020', 'Pirata', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @PirataId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@PirataId, 'PIRATA', 'Taquería', 1, GETDATE()),
        (@PirataId, 'PIRATA CARNE', 'Taquería', 1, GETDATE()),
        (@PirataId, 'PIRATA DE CARNE', 'Taquería', 1, GETDATE()),
        (@PirataId, 'PIRATA PASTOR', 'Taquería', 1, GETDATE()),
        (@PirataId, 'PIRATA DE PASTOR', 'Taquería', 1, GETDATE());

    PRINT 'Pirata agregado con aliases';
END

-- Gringa (similar a pirata pero con queso)
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'GRINGA')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('GRINGA', '7501234500021', 'Gringa', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @GringaId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@GringaId, 'GRINGA', 'Taquería', 1, GETDATE()),
        (@GringaId, 'GRAEA', 'Taquería', 1, GETDATE()),
        (@GringaId, 'GRINGA PASTOR', 'Taquería', 1, GETDATE()),
        (@GringaId, 'GRINGA DE PASTOR', 'Taquería', 1, GETDATE()),
        (@GringaId, 'GRINGA CARNE', 'Taquería', 1, GETDATE());

    PRINT 'Gringa agregado con aliases';
END

-- Hamburguesa Simple
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'HAMBSIMPLE')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('HAMBSIMPLE', '7501234500022', 'Hamburguesa Simple', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @HambSimpleId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@HambSimpleId, 'HAMBURGUESA', 'General', 1, GETDATE()),
        (@HambSimpleId, 'HAMB SIMPLE', 'General', 1, GETDATE()),
        (@HambSimpleId, 'HAMBURGUESA SENCILLA', 'General', 1, GETDATE()),
        (@HambSimpleId, 'BURGER', 'General', 1, GETDATE());

    PRINT 'Hamburguesa Simple agregado con aliases';
END

-- Hamburguesa con Queso
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'HAMBQUESO')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('HAMBQUESO', '7501234500023', 'Hamburguesa con Queso', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @HambQuesoId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@HambQuesoId, 'HAMBURGUESA CON QUESO', 'General', 1, GETDATE()),
        (@HambQuesoId, 'HAMB QUESO', 'General', 1, GETDATE()),
        (@HambQuesoId, 'CHEESEBURGER', 'McDonald''s', 1, GETDATE()),
        (@HambQuesoId, 'HAMBURGUESA QUESO', 'General', 1, GETDATE());

    PRINT 'Hamburguesa con Queso agregado con aliases';
END

-- Hamburguesa Doble
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'HAMBDOBLE')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('HAMBDOBLE', '7501234500024', 'Hamburguesa Doble', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @HambDobleId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@HambDobleId, 'HAMBURGUESA DOBLE', 'General', 1, GETDATE()),
        (@HambDobleId, 'HAMB DOBLE', 'General', 1, GETDATE()),
        (@HambDobleId, 'DOBLE CARNE', 'General', 1, GETDATE()),
        (@HambDobleId, 'DOUBLE BURGER', 'General', 1, GETDATE());

    PRINT 'Hamburguesa Doble agregado con aliases';
END

-- Pizza (por porción)
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'PIZZAPORCION')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('PIZZAPORCION', '7501234500025', 'Pizza por Porción', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @PizzaId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@PizzaId, 'PIZZA', 'General', 1, GETDATE()),
        (@PizzaId, 'PORCION PIZZA', 'General', 1, GETDATE()),
        (@PizzaId, 'REBANADA PIZZA', 'General', 1, GETDATE()),
        (@PizzaId, 'PIZZA SLICE', 'General', 1, GETDATE());

    PRINT 'Pizza por Porción agregado con aliases';
END

-- Burrito
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'BURRITO')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('BURRITO', '7501234500026', 'Burrito', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @BurritoId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@BurritoId, 'BURRITO', 'General', 1, GETDATE()),
        (@BurritoId, 'BURRITO CARNE', 'General', 1, GETDATE()),
        (@BurritoId, 'BURRITO DE CARNE', 'General', 1, GETDATE()),
        (@BurritoId, 'BURRITO POLLO', 'General', 1, GETDATE());

    PRINT 'Burrito agregado con aliases';
END

-- Quesadilla
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'QUESADILLA')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('QUESADILLA', '7501234500027', 'Quesadilla', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @QuesadillaId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@QuesadillaId, 'QUESADILLA', 'General', 1, GETDATE()),
        (@QuesadillaId, 'QUESADILLA QUESO', 'General', 1, GETDATE()),
        (@QuesadillaId, 'QUESADILLA DE QUESO', 'General', 1, GETDATE()),
        (@QuesadillaId, 'QUESADILLA CARNE', 'General', 1, GETDATE());

    PRINT 'Quesadilla agregado con aliases';
END

-- Hot Dog
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'HOTDOG')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('HOTDOG', '7501234500028', 'Hot Dog', 'General', @CategoryAlimentos, 1, GETDATE(), GETDATE());

    DECLARE @HotDogId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@HotDogId, 'HOTDOG', 'General', 1, GETDATE()),
        (@HotDogId, 'HOT DOG', 'General', 1, GETDATE()),
        (@HotDogId, 'PERRO CALIENTE', 'General', 1, GETDATE()),
        (@HotDogId, 'HOT-DOG', 'General', 1, GETDATE());

    PRINT 'Hot Dog agregado con aliases';
END

-- ============================================
-- 3. PAPELERÍA
-- ============================================
PRINT '';
PRINT 'Insertando productos de PAPELERÍA...';

DECLARE @CategoryPapeleria INT = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'PAP');

IF @CategoryPapeleria IS NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryCode = 'PAP')
    BEGIN
        INSERT INTO Categories (CategoryCode, CategoryName, Description, IsDeductible, RequiresApproval, MaxAmountAllowed, CreatedAt, UpdatedAt)
        VALUES ('PAP', 'Papelería', 'Artículos de oficina y papelería', 1, 0, 1000.00, GETDATE(), GETDATE());
        SET @CategoryPapeleria = SCOPE_IDENTITY();
        PRINT 'Categoría Papelería creada';
    END
    ELSE
    BEGIN
        SET @CategoryPapeleria = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'PAP');
        PRINT 'Categoría Papelería ya existe, reutilizando';
    END
END

-- Bolígrafos
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'BOLIBIC')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('BOLIBIC', '7501234500006', 'Bolígrafo BIC Azul', 'BIC', @CategoryPapeleria, 1, GETDATE(), GETDATE());

    DECLARE @BoliId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@BoliId, 'BOLIGRAFO BIC', 'Office Depot', 1, GETDATE()),
        (@BoliId, 'BOLI BIC AZUL', 'Walmart', 1, GETDATE()),
        (@BoliId, 'PLUMA BIC', 'Soriana', 1, GETDATE()),
        (@BoliId, 'BIC CRISTAL AZUL', 'Office Max', 1, GETDATE());

    PRINT 'Bolígrafo BIC agregado con aliases';
END

-- Cuaderno
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CUAD100')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CUAD100', '7501234500007', 'Cuaderno 100 Hojas', 'Scribe', @CategoryPapeleria, 1, GETDATE(), GETDATE());

    DECLARE @CuadId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CuadId, 'CUADERNO 100 HJS', 'Office Depot', 1, GETDATE()),
        (@CuadId, 'CUAD 100 HOJAS', 'Walmart', 1, GETDATE()),
        (@CuadId, 'LIBRETA 100 HJS', 'Soriana', 1, GETDATE()),
        (@CuadId, 'CUADERNO PROFESIONAL', 'Office Max', 1, GETDATE());

    PRINT 'Cuaderno 100 Hojas agregado con aliases';
END

-- Tarjetas Torneto
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'TARJTORN')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('TARJTORN', '7501234500008', 'Tarjetas Torneto', 'Torneto', @CategoryPapeleria, 1, GETDATE(), GETDATE());

    DECLARE @TarjId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@TarjId, 'TARJETA TORNETO', 'Soriana', 1, GETDATE()),
        (@TarjId, 'TORNETO', 'Office Depot', 1, GETDATE()),
        (@TarjId, 'TARJ TORNETO', 'Walmart', 1, GETDATE());

    PRINT 'Tarjetas Torneto agregado con aliases';
END

-- ============================================
-- 4. TRANSPORTE
-- ============================================
PRINT '';
PRINT 'Insertando productos de TRANSPORTE...';

DECLARE @CategoryTransporte INT = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'TRA');

IF @CategoryTransporte IS NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryCode = 'TRA')
    BEGIN
        INSERT INTO Categories (CategoryCode, CategoryName, Description, IsDeductible, RequiresApproval, MaxAmountAllowed, CreatedAt, UpdatedAt)
        VALUES ('TRA', 'Transporte', 'Gastos de transporte y combustible', 1, 1, 2000.00, GETDATE(), GETDATE());
        SET @CategoryTransporte = SCOPE_IDENTITY();
        PRINT 'Categoría Transporte creada';
    END
    ELSE
    BEGIN
        SET @CategoryTransporte = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'TRA');
        PRINT 'Categoría Transporte ya existe, reutilizando';
    END
END

-- Gasolina
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'GASPREM')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('GASPREM', '7501234500009', 'Gasolina Premium', 'Pemex', @CategoryTransporte, 1, GETDATE(), GETDATE());

    DECLARE @GasId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@GasId, 'GAS PREMIUM', 'Pemex', 1, GETDATE()),
        (@GasId, 'GASOLINA PREM', 'Pemex', 1, GETDATE()),
        (@GasId, 'PREMIUM', 'Pemex', 1, GETDATE()),
        (@GasId, 'GAS ROJA', 'Pemex', 1, GETDATE());

    PRINT 'Gasolina Premium agregado con aliases';
END

-- Uber
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'UBERVIAJE')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('UBERVIAJE', '7501234500010', 'Viaje Uber', 'Uber', @CategoryTransporte, 1, GETDATE(), GETDATE());

    DECLARE @UberId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@UberId, 'UBER', 'Uber App', 1, GETDATE()),
        (@UberId, 'VIAJE UBER', 'Uber App', 1, GETDATE()),
        (@UberId, 'UBER TRIP', 'Uber App', 1, GETDATE());

    PRINT 'Viaje Uber agregado con aliases';
END

-- ============================================
-- 5. TECNOLOGÍA
-- ============================================
PRINT '';
PRINT 'Insertando productos de TECNOLOGÍA...';

DECLARE @CategoryTecnologia INT = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'TEC');

IF @CategoryTecnologia IS NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryCode = 'TEC')
    BEGIN
        INSERT INTO Categories (CategoryCode, CategoryName, Description, IsDeductible, RequiresApproval, MaxAmountAllowed, CreatedAt, UpdatedAt)
        VALUES ('TEC', 'Tecnología', 'Equipos y accesorios tecnológicos', 1, 1, 5000.00, GETDATE(), GETDATE());
        SET @CategoryTecnologia = SCOPE_IDENTITY();
        PRINT 'Categoría Tecnología creada';
    END
    ELSE
    BEGIN
        SET @CategoryTecnologia = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'TEC');
        PRINT 'Categoría Tecnología ya existe, reutilizando';
    END
END

-- Mouse USB
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'MOUSEUSB')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('MOUSEUSB', '7501234500011', 'Mouse USB Óptico', 'Logitech', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @MouseId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@MouseId, 'MOUSE USB', 'Office Depot', 1, GETDATE()),
        (@MouseId, 'RATON USB', 'Liverpool', 1, GETDATE()),
        (@MouseId, 'MOUSE OPTICO', 'Best Buy', 1, GETDATE()),
        (@MouseId, 'MOUSE LOGITECH', 'Amazon', 1, GETDATE());

    PRINT 'Mouse USB agregado con aliases';
END

-- Cable HDMI
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CABLEHDMI')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CABLEHDMI', '7501234500012', 'Cable HDMI 2m', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @CableId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CableId, 'CABLE HDMI', 'Office Depot', 1, GETDATE()),
        (@CableId, 'HDMI 2M', 'Best Buy', 1, GETDATE()),
        (@CableId, 'CABLE HDMI 2 MTS', 'Liverpool', 1, GETDATE());

    PRINT 'Cable HDMI agregado con aliases';
END

-- Cable USB-C
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CABLEUSBC')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CABLEUSBC', '7501234500029', 'Cable USB-C', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @CableUSBCId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CableUSBCId, 'CABLE USB-C', 'Office Depot', 1, GETDATE()),
        (@CableUSBCId, 'CABLE USBC', 'Best Buy', 1, GETDATE()),
        (@CableUSBCId, 'CABLE USB C', 'Liverpool', 1, GETDATE()),
        (@CableUSBCId, 'USB-C CABLE', 'Amazon', 1, GETDATE()),
        (@CableUSBCId, 'CABLE TYPE-C', 'Best Buy', 1, GETDATE());

    PRINT 'Cable USB-C agregado con aliases';
END

-- Cable Lightning (iPhone)
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CABLELIGHTNING')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CABLELIGHTNING', '7501234500030', 'Cable Lightning', 'Apple', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @CableLightningId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CableLightningId, 'CABLE LIGHTNING', 'Apple Store', 1, GETDATE()),
        (@CableLightningId, 'LIGHTNING CABLE', 'Best Buy', 1, GETDATE()),
        (@CableLightningId, 'CABLE IPHONE', 'Office Depot', 1, GETDATE()),
        (@CableLightningId, 'CABLE APPLE', 'Liverpool', 1, GETDATE()),
        (@CableLightningId, 'CABLE LIGHTNING USB', 'Amazon', 1, GETDATE());

    PRINT 'Cable Lightning agregado con aliases';
END

-- Cable Micro-USB
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CABLEMICROUSB')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CABLEMICROUSB', '7501234500031', 'Cable Micro-USB', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @CableMicroUSBId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CableMicroUSBId, 'CABLE MICRO USB', 'Office Depot', 1, GETDATE()),
        (@CableMicroUSBId, 'CABLE MICROUSB', 'Best Buy', 1, GETDATE()),
        (@CableMicroUSBId, 'MICRO USB CABLE', 'Liverpool', 1, GETDATE()),
        (@CableMicroUSBId, 'CABLE MICRO-USB', 'Amazon', 1, GETDATE());

    PRINT 'Cable Micro-USB agregado con aliases';
END

-- Cable USB-A a USB-B
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CABLEUSBAB')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CABLEUSBAB', '7501234500032', 'Cable USB-A a USB-B', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @CableUSBABId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CableUSBABId, 'CABLE USB A B', 'Office Depot', 1, GETDATE()),
        (@CableUSBABId, 'CABLE USB IMPRESORA', 'Best Buy', 1, GETDATE()),
        (@CableUSBABId, 'CABLE PRINTER', 'Liverpool', 1, GETDATE()),
        (@CableUSBABId, 'USB A TO B', 'Amazon', 1, GETDATE());

    PRINT 'Cable USB-A a USB-B agregado con aliases';
END

-- Cable Ethernet (Red)
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CABLEETHERNET')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CABLEETHERNET', '7501234500033', 'Cable Ethernet Cat6', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @CableEthernetId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CableEthernetId, 'CABLE ETHERNET', 'Office Depot', 1, GETDATE()),
        (@CableEthernetId, 'CABLE RED', 'Best Buy', 1, GETDATE()),
        (@CableEthernetId, 'CABLE RJ45', 'Liverpool', 1, GETDATE()),
        (@CableEthernetId, 'CABLE CAT6', 'Amazon', 1, GETDATE()),
        (@CableEthernetId, 'ETHERNET CABLE', 'Office Depot', 1, GETDATE());

    PRINT 'Cable Ethernet agregado con aliases';
END

-- Cargador USB
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CARGADORUSB')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CARGADORUSB', '7501234500034', 'Cargador USB de Pared', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @CargadorUSBId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CargadorUSBId, 'CARGADOR USB', 'Office Depot', 1, GETDATE()),
        (@CargadorUSBId, 'CARGADOR DE PARED', 'Best Buy', 1, GETDATE()),
        (@CargadorUSBId, 'ADAPTADOR USB', 'Liverpool', 1, GETDATE()),
        (@CargadorUSBId, 'WALL CHARGER', 'Amazon', 1, GETDATE()),
        (@CargadorUSBId, 'CUBO CARGADOR', 'OXXO', 1, GETDATE());

    PRINT 'Cargador USB agregado con aliases';
END

-- Cargador USB-C (Carga Rápida)
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CARGADORUSBC')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CARGADORUSBC', '7501234500035', 'Cargador USB-C Carga Rápida', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @CargadorUSBCId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CargadorUSBCId, 'CARGADOR USB-C', 'Office Depot', 1, GETDATE()),
        (@CargadorUSBCId, 'CARGADOR USBC', 'Best Buy', 1, GETDATE()),
        (@CargadorUSBCId, 'CARGADOR CARGA RAPIDA', 'Liverpool', 1, GETDATE()),
        (@CargadorUSBCId, 'FAST CHARGER', 'Amazon', 1, GETDATE()),
        (@CargadorUSBCId, 'CARGADOR TIPO C', 'Best Buy', 1, GETDATE());

    PRINT 'Cargador USB-C agregado con aliases';
END

-- Cargador iPhone (Lightning)
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CARGADORIPHONE')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CARGADORIPHONE', '7501234500036', 'Cargador iPhone', 'Apple', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @CargadorIPhoneId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CargadorIPhoneId, 'CARGADOR IPHONE', 'Apple Store', 1, GETDATE()),
        (@CargadorIPhoneId, 'CARGADOR APPLE', 'Best Buy', 1, GETDATE()),
        (@CargadorIPhoneId, 'IPHONE CHARGER', 'Liverpool', 1, GETDATE()),
        (@CargadorIPhoneId, 'CARGADOR LIGHTNING', 'Office Depot', 1, GETDATE());

    PRINT 'Cargador iPhone agregado con aliases';
END

-- Cargador Inalámbrico
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CARGADORINALAMBRICO')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CARGADORINALAMBRICO', '7501234500037', 'Cargador Inalámbrico', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @CargadorInalambricoId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CargadorInalambricoId, 'CARGADOR INALAMBRICO', 'Best Buy', 1, GETDATE()),
        (@CargadorInalambricoId, 'WIRELESS CHARGER', 'Amazon', 1, GETDATE()),
        (@CargadorInalambricoId, 'CARGADOR WIRELESS', 'Liverpool', 1, GETDATE()),
        (@CargadorInalambricoId, 'QI CHARGER', 'Best Buy', 1, GETDATE());

    PRINT 'Cargador Inalámbrico agregado con aliases';
END

-- Cargador de Auto (Mechero)
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'CARGADORAUTO')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('CARGADORAUTO', '7501234500038', 'Cargador de Auto USB', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @CargadorAutoId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@CargadorAutoId, 'CARGADOR AUTO', 'Office Depot', 1, GETDATE()),
        (@CargadorAutoId, 'CARGADOR DE CARRO', 'Best Buy', 1, GETDATE()),
        (@CargadorAutoId, 'CARGADOR VEHICULAR', 'Liverpool', 1, GETDATE()),
        (@CargadorAutoId, 'CAR CHARGER', 'Amazon', 1, GETDATE()),
        (@CargadorAutoId, 'CARGADOR MECHERO', 'OXXO', 1, GETDATE());

    PRINT 'Cargador de Auto agregado con aliases';
END

-- Adaptador HDMI a VGA
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'ADAPTADORHDMIVGA')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('ADAPTADORHDMIVGA', '7501234500039', 'Adaptador HDMI a VGA', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @AdaptadorHDMIVGAId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@AdaptadorHDMIVGAId, 'ADAPTADOR HDMI VGA', 'Office Depot', 1, GETDATE()),
        (@AdaptadorHDMIVGAId, 'HDMI TO VGA', 'Best Buy', 1, GETDATE()),
        (@AdaptadorHDMIVGAId, 'ADAPTADOR HDMI-VGA', 'Liverpool', 1, GETDATE()),
        (@AdaptadorHDMIVGAId, 'CONVERSOR HDMI VGA', 'Amazon', 1, GETDATE());

    PRINT 'Adaptador HDMI a VGA agregado con aliases';
END

-- Adaptador USB-C a HDMI
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'ADAPTADORUSBCHDMI')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('ADAPTADORUSBCHDMI', '7501234500040', 'Adaptador USB-C a HDMI', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @AdaptadorUSBCHDMIId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@AdaptadorUSBCHDMIId, 'ADAPTADOR USB-C HDMI', 'Office Depot', 1, GETDATE()),
        (@AdaptadorUSBCHDMIId, 'USB-C TO HDMI', 'Best Buy', 1, GETDATE()),
        (@AdaptadorUSBCHDMIId, 'ADAPTADOR USBC HDMI', 'Liverpool', 1, GETDATE()),
        (@AdaptadorUSBCHDMIId, 'HUB USB-C HDMI', 'Amazon', 1, GETDATE());

    PRINT 'Adaptador USB-C a HDMI agregado con aliases';
END

-- Memoria USB
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'MEMORIAUSB')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('MEMORIAUSB', '7501234500041', 'Memoria USB 32GB', 'SanDisk', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @MemoriaUSBId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@MemoriaUSBId, 'MEMORIA USB', 'Office Depot', 1, GETDATE()),
        (@MemoriaUSBId, 'USB FLASH', 'Best Buy', 1, GETDATE()),
        (@MemoriaUSBId, 'PENDRIVE', 'Liverpool', 1, GETDATE()),
        (@MemoriaUSBId, 'FLASH DRIVE', 'Amazon', 1, GETDATE()),
        (@MemoriaUSBId, 'USB 32GB', 'Office Depot', 1, GETDATE());

    PRINT 'Memoria USB agregado con aliases';
END

-- Teclado USB
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'TECLADOUSB')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('TECLADOUSB', '7501234500042', 'Teclado USB', 'Logitech', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @TecladoUSBId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@TecladoUSBId, 'TECLADO USB', 'Office Depot', 1, GETDATE()),
        (@TecladoUSBId, 'TECLADO', 'Best Buy', 1, GETDATE()),
        (@TecladoUSBId, 'KEYBOARD', 'Liverpool', 1, GETDATE()),
        (@TecladoUSBId, 'TECLADO LOGITECH', 'Amazon', 1, GETDATE());

    PRINT 'Teclado USB agregado con aliases';
END

-- Audífonos
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'AUDIFONOS')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('AUDIFONOS', '7501234500043', 'Audífonos', 'Genérico', @CategoryTecnologia, 1, GETDATE(), GETDATE());

    DECLARE @AudifonosId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@AudifonosId, 'AUDIFONOS', 'Office Depot', 1, GETDATE()),
        (@AudifonosId, 'AURICULARES', 'Best Buy', 1, GETDATE()),
        (@AudifonosId, 'HEADPHONES', 'Liverpool', 1, GETDATE()),
        (@AudifonosId, 'EARPHONES', 'Amazon', 1, GETDATE()),
        (@AudifonosId, 'AUDIFONO', 'OXXO', 1, GETDATE());

    PRINT 'Audífonos agregado con aliases';
END

-- ============================================
-- 6. SERVICIOS
-- ============================================
PRINT '';
PRINT 'Insertando productos de SERVICIOS...';

DECLARE @CategoryServicios INT = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'SER');

IF @CategoryServicios IS NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Categories WHERE CategoryCode = 'SER')
    BEGIN
        INSERT INTO Categories (CategoryCode, CategoryName, Description, IsDeductible, RequiresApproval, MaxAmountAllowed, CreatedAt, UpdatedAt)
        VALUES ('SER', 'Servicios', 'Servicios profesionales y empresariales', 1, 1, 10000.00, GETDATE(), GETDATE());
        SET @CategoryServicios = SCOPE_IDENTITY();
        PRINT 'Categoría Servicios creada';
    END
    ELSE
    BEGIN
        SET @CategoryServicios = (SELECT TOP 1 CategoryId FROM Categories WHERE CategoryCode = 'SER');
        PRINT 'Categoría Servicios ya existe, reutilizando';
    END
END

-- Hospedaje Hotel
IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = 'HOTELNOCHE')
BEGIN
    INSERT INTO Products (SKU, GTIN, ProductName, Brand, DefaultCategoryId, IsActive, CreatedAt, UpdatedAt)
    VALUES ('HOTELNOCHE', '7501234500013', 'Hospedaje Hotel (noche)', 'General', @CategoryServicios, 1, GETDATE(), GETDATE());

    DECLARE @HotelId INT = SCOPE_IDENTITY();

    INSERT INTO ProductAliases (ProductId, Alias, Source, IsActive, CreatedAt)
    VALUES
        (@HotelId, 'HOSPEDAJE', 'Hoteles', 1, GETDATE()),
        (@HotelId, 'HABITACION HOTEL', 'Hoteles', 1, GETDATE()),
        (@HotelId, 'HOTEL NOCHE', 'Hoteles', 1, GETDATE()),
        (@HotelId, 'ALOJAMIENTO', 'Hoteles', 1, GETDATE());

    PRINT 'Hospedaje Hotel agregado con aliases';
END

PRINT '';
PRINT '========================================';
PRINT 'Resumen de productos agregados:';

-- Resumen corregido sin aggregate anidados
SELECT
    c.CategoryName,
    COUNT(DISTINCT p.ProductId) AS TotalProductos,
    COUNT(pa.AliasId) AS TotalAliases
FROM Products p
INNER JOIN Categories c ON p.DefaultCategoryId = c.CategoryId
LEFT JOIN ProductAliases pa ON p.ProductId = pa.ProductId
WHERE p.IsActive = 1
GROUP BY c.CategoryName
ORDER BY c.CategoryName;

PRINT '';
PRINT '========================================';
PRINT 'Script completado exitosamente!';
PRINT '========================================';
