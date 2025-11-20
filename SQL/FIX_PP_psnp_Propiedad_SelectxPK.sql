-- ============================================================
-- FIX: PP_psnp_Propiedad_SelectxPK - Agregar Url_Imagen
-- ============================================================
USE [JCF_DEV]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_Propiedad_SelectxPK')
    DROP PROCEDURE PP_psnp_Propiedad_SelectxPK
GO

CREATE PROCEDURE [dbo].[PP_psnp_Propiedad_SelectxPK]
(
    @IDPropiedad VARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ID_Propiedad,
        '' AS Codigo_Referencia,
        Title AS Titulo,
        '' AS Descripcion,
        Tipo_elemento AS Tipo_Propiedad,
        Direccion,
        Comuna,
        '' AS Ciudad,
        Region,
        Valor AS Precio,
        '' AS Precio_UF,
        Dormitorios,
        Banos,
        M2_Construidos AS Metros_Construidos,
        M2_Terreno AS Metros_Terreno,
        Estado AS Estado_Propiedad,
        '' AS Fecha_Publicacion,
        Agente_Responsable,
        '' AS Telefono_Contacto,
        '' AS Email_Contacto,
        '' AS Visitas_Totales,
        '' AS Url_Proppit,
        Url_Imagen,  -- AGREGADO: Campo de imagen
        id_TocToc,
        id_ChilePropiedades,
        id_PortalInmobiliario,
        id_Proppit,
        id_PortalRosch,
        1 AS TotalRowCount
    FROM Propiedades 
    WHERE ID_Propiedad = @IDPropiedad
END
GO

PRINT '✓ SP PP_psnp_Propiedad_SelectxPK actualizado con Url_Imagen';
GO

-- Probar el SP
DECLARE @TestID VARCHAR(50);
SELECT TOP 1 @TestID = ID_Propiedad FROM Propiedades ORDER BY ID_Propiedad DESC;
PRINT 'Probando con ID: ' + @TestID;
EXEC PP_psnp_Propiedad_SelectxPK @IDPropiedad = @TestID;
GO

