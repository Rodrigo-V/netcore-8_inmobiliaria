IF OBJECT_ID('dbo.PP_psnp_ClientesLeads_SelectxTipoEmpresa', 'P') IS NOT NULL
    DROP PROCEDURE dbo.PP_psnp_ClientesLeads_SelectxTipoEmpresa;
GO

CREATE PROCEDURE [dbo].[PP_psnp_ClientesLeads_SelectxTipoEmpresa]
    @ID_Cliente VARCHAR(50) = '',
    @Nombres VARCHAR(50) = '',
    @Apellidos VARCHAR(50) = '',
    @Portal VARCHAR(50) = '',
    @Asistente VARCHAR(50) = '',
    @Seguimiento VARCHAR(50) = '',
    @Busqueda VARCHAR(100) = '',
    @Columna VARCHAR(20) = 'Creado',
    @Direccion VARCHAR(50) = 'desc',
    @Min INT = 1,
    @Max INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @Columna NOT IN ('ID_Cliente', 'Nombres', 'Apellidos', 'Portal', 'Asistente', 'Seguimiento', 'Creado', 'Correo_Electronico', 'Telefono')
        SET @Columna = 'Creado';

    IF UPPER(@Direccion) NOT IN ('ASC', 'DESC')
        SET @Direccion = 'DESC';

    DECLARE @sql NVARCHAR(MAX);

    SET @sql = '
    SELECT *
    FROM (
        SELECT
            C.Creado, C.Fecha_Contacto, C.Asistente, C.ID_Cliente, C.Seguimiento, C.Portal, C.Respuesta,
            C.ID_Unidad_Consultada, C.Unidad_Consultada, C.Nombres, C.Apellidos, C.Sexo, C.Telefono,
            C.Correo_Electronico, C.Visita_Realizada,
            P.Url_Imagen AS Url_Imagen_Propiedad,
            COUNT(*) OVER() AS TotalRowCount,
            ROW_NUMBER() OVER(ORDER BY C.' + QUOTENAME(@Columna) + ' ' + @Direccion + ') AS NUMBER
        FROM Clientes_Leads C
        LEFT JOIN Propiedades P ON P.ID_Propiedad = C.ID_Unidad_Consultada
        WHERE 1 = 1
            AND (@ID_Cliente = '''' OR C.ID_Cliente LIKE ''%'' + @ID_Cliente + ''%'')
            AND (@Nombres = '''' OR C.Nombres LIKE ''%'' + @Nombres + ''%'')
            AND (@Apellidos = '''' OR C.Apellidos LIKE ''%'' + @Apellidos + ''%'')
            AND (@Portal = '''' OR C.Portal LIKE ''%'' + @Portal + ''%'')
            AND (@Asistente = '''' OR C.Asistente LIKE ''%'' + @Asistente + ''%'')
            AND (@Seguimiento = '''' OR C.Seguimiento LIKE ''%'' + @Seguimiento + ''%'')
            AND (@Busqueda = '''' OR C.ID_Cliente LIKE ''%'' + @Busqueda + ''%''
                OR C.Nombres LIKE ''%'' + @Busqueda + ''%''
                OR C.Apellidos LIKE ''%'' + @Busqueda + ''%''
                OR C.Telefono LIKE ''%'' + @Busqueda + ''%''
                OR C.Correo_Electronico LIKE ''%'' + @Busqueda + ''%''
                OR C.Portal LIKE ''%'' + @Busqueda + ''%'')
    ) T
    WHERE T.NUMBER BETWEEN ' + CAST(@Min AS VARCHAR(10)) + ' AND ' + CAST(@Max AS VARCHAR(10));

    EXEC sp_executesql @sql,
        N'@ID_Cliente VARCHAR(50), @Nombres VARCHAR(50), @Apellidos VARCHAR(50), @Portal VARCHAR(50), @Asistente VARCHAR(50), @Seguimiento VARCHAR(50), @Busqueda VARCHAR(100)',
        @ID_Cliente, @Nombres, @Apellidos, @Portal, @Asistente, @Seguimiento, @Busqueda;
END
GO

GRANT EXECUTE ON PP_psnp_ClientesLeads_SelectxTipoEmpresa TO PUBLIC;
GO
