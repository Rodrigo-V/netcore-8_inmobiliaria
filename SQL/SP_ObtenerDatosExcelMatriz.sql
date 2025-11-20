-- =============================================
-- Stored Procedure: SP_ObtenerDatosExcelMatriz
-- Descripción: Obtiene datos de clics por fecha y propiedad para generar matriz en Excel
-- Parámetros:
--   @FechaDesde: Fecha inicial del rango (opcional)
--   @FechaHasta: Fecha final del rango (opcional)
-- Retorna: Matriz de fechas x propiedades con conteo de clics
-- =============================================

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
        CAST(cp.FechaClic AS DATE) AS FechaSincronizacion,
        cp.PropiedadId AS ID_Propiedad,
        MAX(cp.PropiedadTitulo) AS TituloPropiedad,
        MAX(p.Comuna) AS Comuna,
        COUNT(*) AS TotalClics,
        SUM(CASE WHEN cp.PortalId = 1 THEN 1 ELSE 0 END) AS ClicsPortalInmobiliario,
        SUM(CASE WHEN cp.PortalId = 2 THEN 1 ELSE 0 END) AS ClicsProppit,
        SUM(CASE WHEN cp.PortalId = 3 THEN 1 ELSE 0 END) AS ClicsChilePropiedades,
        SUM(CASE WHEN cp.PortalId = 4 THEN 1 ELSE 0 END) AS ClicsTocToc
    FROM ClicsPortales cp
    LEFT JOIN Propiedades p ON cp.PropiedadId = p.ID_Propiedad
    WHERE CAST(cp.FechaClic AS DATE) BETWEEN @FechaDesde AND @FechaHasta
    GROUP BY CAST(cp.FechaClic AS DATE), cp.PropiedadId
    ORDER BY FechaSincronizacion DESC, TotalClics DESC;
END
GO

-- Script de prueba
-- EXEC SP_ObtenerDatosExcelMatriz NULL, NULL
-- EXEC SP_ObtenerDatosExcelMatriz '2024-01-01', '2024-12-31'

