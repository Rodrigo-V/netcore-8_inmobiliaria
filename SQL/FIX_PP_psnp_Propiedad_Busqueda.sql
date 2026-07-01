-- Búsqueda por ID, título, dirección o comuna (LIKE)
IF OBJECT_ID('dbo.PP_psnp_Propiedad_SelectxTipoEmpresa', 'P') IS NOT NULL
    DROP PROCEDURE dbo.PP_psnp_Propiedad_SelectxTipoEmpresa;
GO

CREATE PROCEDURE [dbo].[PP_psnp_Propiedad_SelectxTipoEmpresa]
(
    @IDPropiedad VARCHAR(50) = NULL,
    @Columna VARCHAR(50) = NULL,
    @Direccion VARCHAR(10) = 'ASC',
    @Min INT = 1,
    @Max INT = 100
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SQL NVARCHAR(MAX);
    DECLARE @TotalRecords INT;
    DECLARE @OrderBy VARCHAR(50);
    DECLARE @SearchSafe NVARCHAR(50) = NULL;

    IF @IDPropiedad IS NOT NULL AND LTRIM(RTRIM(@IDPropiedad)) <> ''
        SET @SearchSafe = REPLACE(LTRIM(RTRIM(@IDPropiedad)), '''', '''''');

    SELECT @TotalRecords = COUNT(*)
    FROM Propiedades
    WHERE @SearchSafe IS NULL
       OR ID_Propiedad LIKE '%' + @SearchSafe + '%'
       OR Title LIKE '%' + @SearchSafe + '%'
       OR Direccion LIKE '%' + @SearchSafe + '%'
       OR Comuna LIKE '%' + @SearchSafe + '%'
       OR id_TocToc LIKE '%' + @SearchSafe + '%'
       OR id_ChilePropiedades LIKE '%' + @SearchSafe + '%'
       OR id_PortalInmobiliario LIKE '%' + @SearchSafe + '%'
       OR id_Proppit LIKE '%' + @SearchSafe + '%'
       OR id_PortalRosch LIKE '%' + @SearchSafe + '%';

    SET @OrderBy = CASE
        WHEN @Columna = 'Codigo_Referencia' THEN 'ID_Propiedad'
        WHEN @Columna = 'Titulo' THEN 'Title'
        WHEN @Columna = 'Direccion' THEN 'Direccion'
        WHEN @Columna = 'Tipo_Propiedad' THEN 'Tipo_elemento'
        WHEN @Columna = 'Precio' THEN 'Valor'
        WHEN @Columna = 'Comuna' THEN 'Comuna'
        WHEN @Columna = 'Agente_Responsable' THEN 'Agente_Responsable'
        WHEN @Columna = 'id_TocToc' THEN 'id_TocToc'
        WHEN @Columna = 'id_ChilePropiedades' THEN 'id_ChilePropiedades'
        WHEN @Columna = 'id_PortalInmobiliario' THEN 'id_PortalInmobiliario'
        WHEN @Columna = 'id_Proppit' THEN 'id_Proppit'
        WHEN @Columna = 'id_PortalRosch' THEN 'id_PortalRosch'
        ELSE 'ID_Propiedad'
    END;

    SET @SQL = '
    WITH PaginatedResult AS (
        SELECT
            ID_Propiedad,
            '''' AS Codigo_Referencia,
            Title AS Titulo,
            '''' AS Descripcion,
            Tipo_elemento AS Tipo_Propiedad,
            Direccion,
            Comuna,
            '''' AS Ciudad,
            Region,
            Valor AS Precio,
            '''' AS Precio_UF,
            Dormitorios,
            Banos,
            M2_Construidos AS Metros_Construidos,
            M2_Terreno AS Metros_Terreno,
            Estado AS Estado_Propiedad,
            '''' AS Fecha_Publicacion,
            Agente_Responsable,
            '''' AS Telefono_Contacto,
            '''' AS Email_Contacto,
            '''' AS Visitas_Totales,
            '''' AS Url_Proppit,
            ' + CAST(@TotalRecords AS VARCHAR) + ' AS TotalRowCount,
            Url_Imagen,
            id_TocToc,
            id_ChilePropiedades,
            id_PortalInmobiliario,
            id_Proppit,
            id_PortalRosch,
            ROW_NUMBER() OVER (ORDER BY ' + @OrderBy + ' ' + @Direccion + ') AS RowNum
        FROM Propiedades';

    IF @SearchSafe IS NOT NULL
        SET @SQL = @SQL + '
        WHERE ID_Propiedad LIKE ''%' + @SearchSafe + '%''
           OR Title LIKE ''%' + @SearchSafe + '%''
           OR Direccion LIKE ''%' + @SearchSafe + '%''
           OR Comuna LIKE ''%' + @SearchSafe + '%''
           OR id_TocToc LIKE ''%' + @SearchSafe + '%''
           OR id_ChilePropiedades LIKE ''%' + @SearchSafe + '%''
           OR id_PortalInmobiliario LIKE ''%' + @SearchSafe + '%''
           OR id_Proppit LIKE ''%' + @SearchSafe + '%''
           OR id_PortalRosch LIKE ''%' + @SearchSafe + '%''';

    SET @SQL = @SQL + '
    )
    SELECT * FROM PaginatedResult
    WHERE RowNum BETWEEN ' + CAST(@Min AS VARCHAR) + ' AND ' + CAST(@Max AS VARCHAR) + '
    ORDER BY RowNum';

    EXEC sp_executesql @SQL;
END
GO

GRANT EXECUTE ON PP_psnp_Propiedad_SelectxTipoEmpresa TO PUBLIC;
GO
