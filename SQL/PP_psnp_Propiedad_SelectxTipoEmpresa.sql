-- ============================================================
-- Stored Procedure: PP_psnp_Propiedad_SelectxTipoEmpresa
-- Descripción: Obtiene propiedades con paginación y filtros
-- ============================================================
-- Autor: Sistema Inmobiliaria .NET 8
-- Fecha: 2025-11-14
-- ============================================================

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_Propiedad_SelectxTipoEmpresa')
BEGIN
    DROP PROCEDURE PP_psnp_Propiedad_SelectxTipoEmpresa;
    PRINT '✓ Procedimiento anterior eliminado correctamente';
END
GO

CREATE PROCEDURE [dbo].[PP_psnp_Propiedad_SelectxTipoEmpresa]
    @IDPropiedad NVARCHAR(50) = NULL,
    @Columna NVARCHAR(50) = 'Fecha_Publicacion',
    @Direccion NVARCHAR(10) = 'desc',
    @Min INT = 1,
    @Max INT = 10
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Consulta con paginación y ordenamiento
        WITH PropiedadesPaginadas AS (
            SELECT 
                ID_Propiedad,
                Codigo_Referencia,
                Title,
                Description,
                Tipo_elemento,
                Direccion,
                Comuna,
                Ciudad,
                Region,
                Valor,
                '' AS Precio_UF,
                Dormitorios,
                Banos,
                M2_Construidos,
                M2_Terreno,
                Estado,
                Fecha_Publicacion,
                Agente_Responsable,
                '' AS Telefono_Contacto,
                '' AS Email_Contacto,
                '' AS Visitas_Totales,
                Url_Imagen,
                id_TocToc,
                id_ChilePropiedades,
                id_PortalInmobiliario,
                id_Proppit,
                id_PortalRosch,
                COUNT(*) OVER() AS TotalRowCount,
                ROW_NUMBER() OVER(
                    ORDER BY 
                        CASE WHEN @Columna = 'ID_Propiedad' AND @Direccion = 'asc' THEN ID_Propiedad END ASC,
                        CASE WHEN @Columna = 'ID_Propiedad' AND @Direccion = 'desc' THEN ID_Propiedad END DESC,
                        CASE WHEN @Columna = 'Codigo_Referencia' AND @Direccion = 'asc' THEN Codigo_Referencia END ASC,
                        CASE WHEN @Columna = 'Codigo_Referencia' AND @Direccion = 'desc' THEN Codigo_Referencia END DESC,
                        CASE WHEN @Columna = 'Title' AND @Direccion = 'asc' THEN Title END ASC,
                        CASE WHEN @Columna = 'Title' AND @Direccion = 'desc' THEN Title END DESC,
                        CASE WHEN @Columna = 'Tipo_elemento' AND @Direccion = 'asc' THEN Tipo_elemento END ASC,
                        CASE WHEN @Columna = 'Tipo_elemento' AND @Direccion = 'desc' THEN Tipo_elemento END DESC,
                        CASE WHEN @Columna = 'Valor' AND @Direccion = 'asc' THEN Valor END ASC,
                        CASE WHEN @Columna = 'Valor' AND @Direccion = 'desc' THEN Valor END DESC,
                        CASE WHEN @Columna = 'Comuna' AND @Direccion = 'asc' THEN Comuna END ASC,
                        CASE WHEN @Columna = 'Comuna' AND @Direccion = 'desc' THEN Comuna END DESC,
                        CASE WHEN @Columna = 'Fecha_Publicacion' AND @Direccion = 'asc' THEN Fecha_Publicacion END ASC,
                        CASE WHEN @Columna = 'Fecha_Publicacion' AND @Direccion = 'desc' THEN Fecha_Publicacion END DESC,
                        ID_Propiedad ASC
                ) AS RowNum
            FROM Propiedades WITH (NOLOCK)
            WHERE (@IDPropiedad IS NULL OR ID_Propiedad LIKE '%' + @IDPropiedad + '%' 
                   OR Title LIKE '%' + @IDPropiedad + '%'
                   OR Direccion LIKE '%' + @IDPropiedad + '%'
                   OR Comuna LIKE '%' + @IDPropiedad + '%')
        )
        SELECT 
            ID_Propiedad,
            Codigo_Referencia,
            Title,
            Description,
            Tipo_elemento,
            Direccion,
            Comuna,
            Ciudad,
            Region,
            Valor,
            Precio_UF,
            Dormitorios,
            Banos,
            M2_Construidos,
            M2_Terreno,
            Estado,
            Fecha_Publicacion,
            Agente_Responsable,
            Telefono_Contacto,
            Email_Contacto,
            Visitas_Totales,
            Url_Imagen,
            id_TocToc,
            id_ChilePropiedades,
            id_PortalInmobiliario,
            id_Proppit,
            id_PortalRosch,
            TotalRowCount
        FROM PropiedadesPaginadas
        WHERE RowNum BETWEEN @Min AND @Max
        ORDER BY RowNum;
        
    END TRY
    BEGIN CATCH
        DECLARE @Error NVARCHAR(MAX);
        SET @Error = ERROR_MESSAGE();
        RAISERROR(@Error, 16, 1);
    END CATCH
END
GO

GRANT EXECUTE ON PP_psnp_Propiedad_SelectxTipoEmpresa TO PUBLIC;
GO

PRINT '✓ Stored Procedure PP_psnp_Propiedad_SelectxTipoEmpresa creado exitosamente';
GO

