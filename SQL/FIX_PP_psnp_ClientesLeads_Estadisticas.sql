-- Corrige columnas Estado inexistentes en Clientes_Leads
IF OBJECT_ID('dbo.PP_psnp_ClientesLeads_Estadisticas', 'P') IS NOT NULL
    DROP PROCEDURE dbo.PP_psnp_ClientesLeads_Estadisticas;
GO

CREATE PROCEDURE [dbo].[PP_psnp_ClientesLeads_Estadisticas]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        COUNT(*) AS TotalLeads,
        COUNT(CASE WHEN Seguimiento = 'Nuevo' THEN 1 END) AS Nuevos,
        COUNT(CASE WHEN Seguimiento = 'En Seguimiento' THEN 1 END) AS EnSeguimiento,
        COUNT(CASE WHEN Seguimiento = 'Con Visita Programada' THEN 1 END) AS ConVisita,
        COUNT(CASE WHEN Seguimiento = 'En Espera' THEN 1 END) AS EnEspera,
        COUNT(CASE WHEN Seguimiento = 'Terminado' THEN 1 END) AS Terminados,
        COUNT(CASE WHEN Visita_Realizada = 1 THEN 1 END) AS ConVisitaRealizada,
        COUNT(CASE WHEN Seguimiento = 'RESERVA' THEN 1 END) AS Reservas
    FROM [dbo].[Clientes_Leads];

    SELECT
        Portal,
        COUNT(*) AS Cantidad,
        COUNT(CASE WHEN Seguimiento = 'Nuevo' THEN 1 END) AS Nuevos,
        COUNT(CASE WHEN Seguimiento = 'Terminado' THEN 1 END) AS Terminados
    FROM [dbo].[Clientes_Leads]
    WHERE Portal IS NOT NULL
    GROUP BY Portal
    ORDER BY Cantidad DESC;

    SELECT
        Asistente,
        COUNT(*) AS Cantidad,
        COUNT(CASE WHEN Seguimiento = 'Nuevo' THEN 1 END) AS Nuevos,
        COUNT(CASE WHEN Seguimiento = 'Terminado' THEN 1 END) AS Terminados,
        COUNT(CASE WHEN Visita_Realizada = 1 THEN 1 END) AS ConVisitaRealizada
    FROM [dbo].[Clientes_Leads]
    WHERE Asistente IS NOT NULL
    GROUP BY Asistente
    ORDER BY Cantidad DESC;
END
GO

GRANT EXECUTE ON PP_psnp_ClientesLeads_Estadisticas TO PUBLIC;
GO
