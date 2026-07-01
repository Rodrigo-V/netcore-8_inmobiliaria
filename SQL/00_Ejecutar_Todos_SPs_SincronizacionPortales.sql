-- =============================================
-- Script Maestro: Ejecutar todos los Stored Procedures de Sincronización de Portales
-- Descripción: Ejecuta todos los scripts SQL para crear los SPs necesarios
-- Fecha: 2025
-- =============================================

USE [JCF_DEV]
GO

PRINT '=========================================='
PRINT 'INICIANDO CREACIÓN DE STORED PROCEDURES'
PRINT 'Sincronización de Portales'
PRINT '=========================================='
PRINT ''

-- =============================================
-- 1. SP_ObtenerMatrizClics (Ya existe en la base)
-- =============================================
PRINT '1. Verificando SP_ObtenerMatrizClics...'
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerMatrizClics')
BEGIN
    PRINT '   ✓ SP_ObtenerMatrizClics ya existe en la base de datos'
END
ELSE
BEGIN
    PRINT '   ✗ ERROR: SP_ObtenerMatrizClics NO existe. Debe ser creado.'
END
PRINT ''

-- =============================================
-- 2. SP_ObtenerResumenClicsPortales (Ya existe en la base)
-- =============================================
PRINT '2. Verificando SP_ObtenerResumenClicsPortales...'
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerResumenClicsPortales')
BEGIN
    PRINT '   ✓ SP_ObtenerResumenClicsPortales ya existe en la base de datos'
END
ELSE
BEGIN
    PRINT '   ✗ ERROR: SP_ObtenerResumenClicsPortales NO existe. Debe ser creado.'
END
PRINT ''

-- =============================================
-- 3. SP_ObtenerPropiedadesConClics (NUEVO)
-- =============================================
PRINT '3. Creando SP_ObtenerPropiedadesConClics...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerPropiedadesConClics')
    DROP PROCEDURE SP_ObtenerPropiedadesConClics
GO

CREATE PROCEDURE SP_ObtenerPropiedadesConClics
    @BuscarPropiedad NVARCHAR(255) = NULL,
    @OrdenarPor NVARCHAR(50) = 'clicks',
    @TopRegistros INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SQL NVARCHAR(MAX);

    SET @SQL = '
    SELECT TOP ' + CAST(@TopRegistros AS NVARCHAR(10)) + '
        p.ID_Propiedad,
        p.Title AS Titulo,
        p.Direccion,
        p.Comuna,
        p.Region,
        p.Valor AS Precio,
        p.Tipo_elemento AS TipoPropiedad,
        p.Dormitorios,
        p.Banos,
        p.M2_Construidos AS MetrosConstruidos,
        p.M2_Terreno AS MetrosTerreno,
        p.Agente_Responsable,
        p.Url_Imagen,
        ISNULL((SELECT COUNT(*) FROM ClicsPortales cp WHERE cp.ID_Propiedad = p.ID_Propiedad AND cp.NombrePortal = ''PortalInmobiliario.com''), 0) AS ClicsPortalInmobiliario,
        ISNULL((SELECT COUNT(*) FROM ClicsPortales cp WHERE cp.ID_Propiedad = p.ID_Propiedad AND cp.NombrePortal = ''Proppit''), 0) AS ClicsProppit,
        ISNULL((SELECT COUNT(*) FROM ClicsPortales cp WHERE cp.ID_Propiedad = p.ID_Propiedad AND cp.NombrePortal = ''ChilePropiedades''), 0) AS ClicsChilePropiedades,
        ISNULL((SELECT COUNT(*) FROM ClicsPortales cp WHERE cp.ID_Propiedad = p.ID_Propiedad AND cp.NombrePortal = ''TocToc''), 0) AS ClicsTocToc,
        ISNULL((SELECT COUNT(*) FROM ClicsPortales cp WHERE cp.ID_Propiedad = p.ID_Propiedad), 0) AS TotalClicsTodosPortales,
        (SELECT MAX(Fecha_Clic) FROM ClicsPortales cp WHERE cp.ID_Propiedad = p.ID_Propiedad) AS UltimoClick
    FROM Propiedades p
    WHERE 1=1';

    IF @BuscarPropiedad IS NOT NULL AND @BuscarPropiedad != ''
    BEGIN
        SET @SQL = @SQL + ' 
        AND (p.ID_Propiedad LIKE ''%' + @BuscarPropiedad + '%'' 
             OR p.Title LIKE ''%' + @BuscarPropiedad + '%'' 
             OR p.Direccion LIKE ''%' + @BuscarPropiedad + '%''
             OR p.Comuna LIKE ''%' + @BuscarPropiedad + '%'')';
    END

    IF @OrdenarPor = 'precio'
        SET @SQL = @SQL + ' ORDER BY p.Valor DESC';
    ELSE IF @OrdenarPor = 'nombre'
        SET @SQL = @SQL + ' ORDER BY p.Title ASC';
    ELSE
        SET @SQL = @SQL + ' ORDER BY TotalClicsTodosPortales DESC, p.Title ASC';

    EXEC sp_executesql @SQL;
END
GO

PRINT '   ✓ SP_ObtenerPropiedadesConClics creado exitosamente'
PRINT ''

-- =============================================
-- 4. SP_ObtenerEstadisticasGenerales (NUEVO)
-- =============================================
PRINT '4. Creando SP_ObtenerEstadisticasGenerales...'
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerEstadisticasGenerales')
    DROP PROCEDURE SP_ObtenerEstadisticasGenerales
GO

CREATE PROCEDURE SP_ObtenerEstadisticasGenerales
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        COUNT(DISTINCT p.ID_Propiedad) AS TotalPropiedades,
        ISNULL((SELECT COUNT(*) FROM ClicsPortales), 0) AS TotalClics,
        COUNT(DISTINCT CASE 
            WHEN EXISTS (SELECT 1 FROM ClicsPortales cp WHERE cp.ID_Propiedad = p.ID_Propiedad) 
            THEN p.ID_Propiedad 
            ELSE NULL 
        END) AS PropiedadesConClics,
        CASE 
            WHEN COUNT(DISTINCT p.ID_Propiedad) > 0 
            THEN CAST(ISNULL((SELECT COUNT(*) FROM ClicsPortales), 0) AS FLOAT) / COUNT(DISTINCT p.ID_Propiedad)
            ELSE 0 
        END AS PromedioClicsPorPropiedad
    FROM Propiedades p;
END
GO

PRINT '   ✓ SP_ObtenerEstadisticasGenerales creado exitosamente'
PRINT ''

-- =============================================
-- 4. SP_ObtenerDatosExcelMatriz
-- =============================================
PRINT '4. Creando SP_ObtenerDatosExcelMatriz...'

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerDatosExcelMatriz')
    DROP PROCEDURE SP_ObtenerDatosExcelMatriz
GO

CREATE PROCEDURE SP_ObtenerDatosExcelMatriz
    @FechaDesde DATE = NULL,
    @FechaHasta DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Si no se especifican fechas, usar últimos 30 días
    IF @FechaDesde IS NULL
        SET @FechaDesde = DATEADD(DAY, -30, GETDATE());
        
    IF @FechaHasta IS NULL
        SET @FechaHasta = GETDATE();

    -- Obtener datos agrupados por fecha y propiedad
    SELECT 
        CAST(cp.Fecha_Clic AS DATE) AS FechaSincronizacion,
        cp.ID_Propiedad,
        p.Title AS TituloPropiedad,
        p.Comuna,
        COUNT(*) AS TotalClics,
        SUM(CASE WHEN cp.NombrePortal = 'PortalInmobiliario.com' THEN 1 ELSE 0 END) AS ClicsPortalInmobiliario,
        SUM(CASE WHEN cp.NombrePortal = 'Proppit' THEN 1 ELSE 0 END) AS ClicsProppit,
        SUM(CASE WHEN cp.NombrePortal = 'ChilePropiedades' THEN 1 ELSE 0 END) AS ClicsChilePropiedades,
        SUM(CASE WHEN cp.NombrePortal = 'TocToc' THEN 1 ELSE 0 END) AS ClicsTocToc
    FROM ClicsPortales cp
    INNER JOIN Propiedades p ON cp.ID_Propiedad = p.ID_Propiedad
    WHERE CAST(cp.Fecha_Clic AS DATE) BETWEEN @FechaDesde AND @FechaHasta
    GROUP BY CAST(cp.Fecha_Clic AS DATE), cp.ID_Propiedad, p.Title, p.Comuna
    ORDER BY FechaSincronizacion DESC, TotalClics DESC;
END
GO

PRINT '   ✓ SP_ObtenerDatosExcelMatriz creado exitosamente'
PRINT ''

-- =============================================
-- VERIFICACIÓN FINAL
-- =============================================
PRINT '=========================================='
PRINT 'VERIFICACIÓN FINAL'
PRINT '=========================================='

DECLARE @TotalSPs INT = 0;
DECLARE @SPsCreados INT = 0;

SET @TotalSPs = 4;

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerMatrizClics')
    SET @SPsCreados = @SPsCreados + 1;

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerResumenClicsPortales')
    SET @SPsCreados = @SPsCreados + 1;

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerPropiedadesConClics')
    SET @SPsCreados = @SPsCreados + 1;

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerEstadisticasGenerales')
    SET @SPsCreados = @SPsCreados + 1;

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerDatosExcelMatriz')
    SET @SPsCreados = @SPsCreados + 1;

PRINT 'Stored Procedures creados: ' + CAST(@SPsCreados AS NVARCHAR) + ' de ' + CAST(@TotalSPs AS NVARCHAR);

IF @SPsCreados = @TotalSPs
    PRINT '✓ TODOS los Stored Procedures han sido creados exitosamente'
ELSE
    PRINT '✗ ADVERTENCIA: Faltan algunos Stored Procedures por crear'

PRINT '=========================================='
PRINT 'PROCESO COMPLETADO'
PRINT '=========================================='

