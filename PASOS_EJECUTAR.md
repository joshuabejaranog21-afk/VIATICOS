# PASOS PARA EJECUTAR LOS SCRIPTS

## Opción 1: BORRAR TODO Y EMPEZAR DESDE CERO (Recomendado)

Ejecuta estos comandos EN ORDEN en PowerShell:

```powershell
# Paso 1: Borrar TODOS los datos
Invoke-Sqlcmd -ServerInstance "MAMALONA" -Database "OneCardExpenseValidator" -InputFile "DeleteAllData.sql"

# Paso 2: Insertar los datos nuevos
Invoke-Sqlcmd -ServerInstance "MAMALONA" -Database "OneCardExpenseValidator" -InputFile "SeedProductsAndAliases.sql"
```

## Opción 2: Solo limpiar datos corruptos (Menos agresivo)

```powershell
# Paso 1: Limpiar solo datos del seed y corruptos
Invoke-Sqlcmd -ServerInstance "MAMALONA" -Database "OneCardExpenseValidator" -InputFile "CleanupSeedData.sql"

# Paso 2: Insertar los datos nuevos
Invoke-Sqlcmd -ServerInstance "MAMALONA" -Database "OneCardExpenseValidator" -InputFile "SeedProductsAndAliases.sql"
```

## Scripts disponibles:

1. **DeleteAllData.sql** - Borra TODO de las tablas (Products, ProductAliases, Categories, ExpenseItems)
2. **CleanupSeedData.sql** - Solo borra datos del seed y productos corruptos (GTIN = NULL)
3. **SeedProductsAndAliases.sql** - Inserta los productos y aliases nuevos

## Notas importantes:

- **SIEMPRE** ejecuta el script de limpieza PRIMERO
- **DESPUÉS** ejecuta el script de seed
- Si ves errores, copia y pega el error completo
