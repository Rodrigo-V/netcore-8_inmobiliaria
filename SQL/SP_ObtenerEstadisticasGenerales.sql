-- =============================================
-- Stored Procedure: SP_ObtenerEstadisticasGenerales
-- Descripción: Obtiene estadísticas generales de propiedades y clics
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerEstadisticasGenerales')
    DROP PROCEDURE SP_ObtenerEstadisticasGenerales
GO

CREATE PROCEDURE SP_ObtenerEstadisticasGenerales
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        COUNT(DISTINCT p.ID_Propiedad) AS TotalPropiedades,
        ISNULL((SELECT COUNT(*) FROM Clics_Portales), 0) AS TotalClics,
        COUNT(DISTINCT CASE 
            WHEN EXISTS (SELECT 1 FROM Clics_Portales cp WHERE cp.ID_Propiedad = p.ID_Propiedad) 
            THEN p.ID_Propiedad 
            ELSE NULL 
        END) AS PropiedadesConClics,
        CASE 
            WHEN COUNT(DISTINCT p.ID_Propiedad) > 0 
            THEN CAST(ISNULL((SELECT COUNT(*) FROM Clics_Portales), 0) AS FLOAT) / COUNT(DISTINCT p.ID_Propiedad)
            ELSE 0 
        END AS PromedioClicsPorPropiedad
    FROM Propiedades p;
END
GO

-- Script de prueba
-- EXEC SP_ObtenerEstadisticasGenerales

