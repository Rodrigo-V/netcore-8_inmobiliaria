-- ============================================================
-- Stored Procedures para Catálogos de Propiedades
-- ============================================================
-- Autor: Sistema Inmobiliaria .NET 8
-- Fecha: 2025-11-14
-- ============================================================

USE [JCF_DEV]
GO

-- ============================================================
-- 1. SP para obtener Tipos de Propiedad
-- ============================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_Propiedad_ObtenerTipos')
    DROP PROCEDURE PP_psnp_Propiedad_ObtenerTipos
GO

CREATE PROCEDURE [dbo].[PP_psnp_Propiedad_ObtenerTipos]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Retornar tipos de propiedad comunes
    SELECT 'Casa' AS Tipo
    UNION SELECT 'Departamento'
    UNION SELECT 'Oficina'
    UNION SELECT 'Local Comercial'
    UNION SELECT 'Terreno'
    UNION SELECT 'Parcela'
    UNION SELECT 'Bodega'
    UNION SELECT 'Estacionamiento'
    UNION SELECT 'Elemento'
    ORDER BY Tipo;
END
GO

GRANT EXECUTE ON PP_psnp_Propiedad_ObtenerTipos TO PUBLIC;
GO

PRINT '✓ SP_ObtenerTipos creado exitosamente';
GO


-- ============================================================
-- 2. SP para obtener Estados de Propiedad
-- ============================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_Propiedad_ObtenerEstados')
    DROP PROCEDURE PP_psnp_Propiedad_ObtenerEstados
GO

CREATE PROCEDURE [dbo].[PP_psnp_Propiedad_ObtenerEstados]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT Estado
    FROM Propiedades
    WHERE Estado IS NOT NULL AND Estado != ''
    UNION
    SELECT 'Disponible'
    UNION SELECT 'Reservada'
    UNION SELECT 'Vendida'
    UNION SELECT 'Arrendada'
    UNION SELECT 'En Accion'
    UNION SELECT 'En Carpeta'
    UNION SELECT 'En Edicion'
    UNION SELECT 'Suspendida'
    ORDER BY Estado;
END
GO

GRANT EXECUTE ON PP_psnp_Propiedad_ObtenerEstados TO PUBLIC;
GO

PRINT '✓ SP_ObtenerEstados creado exitosamente';
GO


-- ============================================================
-- 3. SP para obtener Agentes Responsables
-- ============================================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_Propiedad_ObtenerAgentes')
    DROP PROCEDURE PP_psnp_Propiedad_ObtenerAgentes
GO

CREATE PROCEDURE [dbo].[PP_psnp_Propiedad_ObtenerAgentes]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Intentar obtener de usuarios del sistema primero
    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Usuarios')
    BEGIN
        SELECT DISTINCT Nombre AS Agente
        FROM Usuarios
        WHERE Nombre IS NOT NULL AND Nombre != ''
        ORDER BY Nombre;
    END
    ELSE
    BEGIN
        -- Si no hay tabla de usuarios, obtener de propiedades existentes
        SELECT DISTINCT Agente_Responsable AS Agente
        FROM Propiedades
        WHERE Agente_Responsable IS NOT NULL AND Agente_Responsable != ''
        UNION
        SELECT 'Sin Asignar'
        ORDER BY Agente;
    END
END
GO

GRANT EXECUTE ON PP_psnp_Propiedad_ObtenerAgentes TO PUBLIC;
GO

PRINT '✓ SP_ObtenerAgentes creado exitosamente';
GO


-- ============================================================
-- Verificación de creación
-- ============================================================
PRINT '';
PRINT '=========================================================';
PRINT '✓ Todos los SPs de catálogos creados exitosamente';
PRINT '=========================================================';
PRINT '';
PRINT 'SPs creados:';
PRINT '  1. PP_psnp_Propiedad_ObtenerTipos';
PRINT '  2. PP_psnp_Propiedad_ObtenerEstados';
PRINT '  3. PP_psnp_Propiedad_ObtenerAgentes';
PRINT '';
PRINT 'Ejemplo de uso:';
PRINT '  EXEC PP_psnp_Propiedad_ObtenerTipos';
PRINT '  EXEC PP_psnp_Propiedad_ObtenerEstados';
PRINT '  EXEC PP_psnp_Propiedad_ObtenerAgentes';
PRINT '';
GO

