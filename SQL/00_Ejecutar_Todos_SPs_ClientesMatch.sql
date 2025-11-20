-- ============================================================
-- Script Maestro: Ejecutar todos los SPs de Clientes Match
-- ============================================================
-- Descripción: Este script ejecuta todos los stored procedures
--              necesarios para el módulo de Clientes Match
-- Autor: Sistema Inmobiliaria .NET 8
-- Fecha: 2025-01-07
-- ============================================================

USE [InmobiliariaDB]
GO

PRINT '';
PRINT '=========================================================';
PRINT '     CREANDO STORED PROCEDURES PARA CLIENTES MATCH      ';
PRINT '=========================================================';
PRINT '';

-- 1. SP_ConvertirLeadAClienteMatch (ya existe, pero se incluye referencia)
PRINT '1. SP_ConvertirLeadAClienteMatch - Ya creado previamente';
PRINT '';

-- 2. PP_psnp_ClientesMatch_SelectAll
PRINT '2. Ejecutando PP_psnp_ClientesMatch_SelectAll.sql...';
:r .\PP_psnp_ClientesMatch_SelectAll.sql
GO

-- 3. PP_psnp_ClientesMatch_SelectxPK
PRINT '3. Ejecutando PP_psnp_ClientesMatch_SelectxPK.sql...';
:r .\PP_psnp_ClientesMatch_SelectxPK.sql
GO

-- 4. PP_psnp_ClientesMatch_Insert
PRINT '4. Ejecutando PP_psnp_ClientesMatch_Insert.sql...';
:r .\PP_psnp_ClientesMatch_Insert.sql
GO

-- 5. PP_psnp_ClientesMatch_Update
PRINT '5. Ejecutando PP_psnp_ClientesMatch_Update.sql...';
:r .\PP_psnp_ClientesMatch_Update.sql
GO

-- 6. PP_psnp_ClientesMatch_Delete
PRINT '6. Ejecutando PP_psnp_ClientesMatch_Delete.sql...';
:r .\PP_psnp_ClientesMatch_Delete.sql
GO

PRINT '';
PRINT '=========================================================';
PRINT '  ✅ TODOS LOS STORED PROCEDURES CREADOS EXITOSAMENTE   ';
PRINT '=========================================================';
PRINT '';
PRINT 'Stored Procedures creados:';
PRINT '  - SP_ConvertirLeadAClienteMatch (Conversión de Lead)';
PRINT '  - PP_psnp_ClientesMatch_SelectAll (Listar todos)';
PRINT '  - PP_psnp_ClientesMatch_SelectxPK (Obtener por ID)';
PRINT '  - PP_psnp_ClientesMatch_Insert (Insertar - opcional)';
PRINT '  - PP_psnp_ClientesMatch_Update (Actualizar)';
PRINT '  - PP_psnp_ClientesMatch_Delete (Eliminar)';
PRINT '';
PRINT 'Puedes verificar con:';
PRINT 'SELECT name FROM sys.procedures WHERE name LIKE ''%ClientesMatch%''';
PRINT 'OR name LIKE ''%ConvertirLeadAClienteMatch%''';
PRINT '';
PRINT '=========================================================';
GO

-- Verificación
SELECT 
    name AS 'Stored Procedure',
    create_date AS 'Fecha Creación',
    modify_date AS 'Última Modificación'
FROM sys.procedures 
WHERE name LIKE '%ClientesMatch%'
   OR name LIKE '%ConvertirLeadAClienteMatch%'
ORDER BY name;
GO

