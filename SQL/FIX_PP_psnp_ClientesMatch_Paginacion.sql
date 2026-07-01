IF OBJECT_ID('dbo.PP_psnp_ClientesMatch_SelectAll', 'P') IS NOT NULL
    DROP PROCEDURE dbo.PP_psnp_ClientesMatch_SelectAll;
GO

CREATE PROCEDURE [dbo].[PP_psnp_ClientesMatch_SelectAll]
    @ID_Interno NVARCHAR(50) = '',
    @Nombre NVARCHAR(200) = '',
    @Telefono NVARCHAR(20) = '',
    @Correo NVARCHAR(100) = '',
    @Tipo_Match NVARCHAR(100) = '',
    @Busqueda NVARCHAR(200) = '',
    @Columna NVARCHAR(50) = 'ID_Interno',
    @Direccion NVARCHAR(4) = 'ASC',
    @Min INT = 1,
    @Max INT = 100
AS
BEGIN
    SET NOCOUNT ON;

    IF @Columna NOT IN ('ID_Interno', 'Nombre', 'Telefono', 'Correo', 'Tipo_Match', 'Rut', 'Comuna', 'Profesion')
        SET @Columna = 'ID_Interno';

    IF @Direccion NOT IN ('ASC', 'DESC')
        SET @Direccion = 'ASC';

    DECLARE @SearchSafe NVARCHAR(200) = NULL;
    IF @Busqueda IS NOT NULL AND LTRIM(RTRIM(@Busqueda)) <> ''
        SET @SearchSafe = REPLACE(LTRIM(RTRIM(@Busqueda)), '''', '''''');

    ;WITH Filtered AS (
        SELECT
            ID_Interno,
            Tipo_Match,
            Nombre,
            Rut,
            Datos_adjuntos,
            Direccion,
            Comuna,
            Estado_Civil,
            Profesion,
            Telefono,
            Correo,
            Giro_Razon_Social,
            COUNT(*) OVER() AS TotalRowCount,
            ROW_NUMBER() OVER (ORDER BY
                CASE WHEN @Columna = 'ID_Interno' AND @Direccion = 'ASC' THEN ID_Interno END ASC,
                CASE WHEN @Columna = 'ID_Interno' AND @Direccion = 'DESC' THEN ID_Interno END DESC,
                CASE WHEN @Columna = 'Nombre' AND @Direccion = 'ASC' THEN Nombre END ASC,
                CASE WHEN @Columna = 'Nombre' AND @Direccion = 'DESC' THEN Nombre END DESC,
                CASE WHEN @Columna = 'Telefono' AND @Direccion = 'ASC' THEN Telefono END ASC,
                CASE WHEN @Columna = 'Telefono' AND @Direccion = 'DESC' THEN Telefono END DESC,
                CASE WHEN @Columna = 'Correo' AND @Direccion = 'ASC' THEN Correo END ASC,
                CASE WHEN @Columna = 'Correo' AND @Direccion = 'DESC' THEN Correo END DESC,
                CASE WHEN @Columna = 'Tipo_Match' AND @Direccion = 'ASC' THEN Tipo_Match END ASC,
                CASE WHEN @Columna = 'Tipo_Match' AND @Direccion = 'DESC' THEN Tipo_Match END DESC,
                CASE WHEN @Columna = 'Rut' AND @Direccion = 'ASC' THEN Rut END ASC,
                CASE WHEN @Columna = 'Rut' AND @Direccion = 'DESC' THEN Rut END DESC,
                CASE WHEN @Columna = 'Comuna' AND @Direccion = 'ASC' THEN Comuna END ASC,
                CASE WHEN @Columna = 'Comuna' AND @Direccion = 'DESC' THEN Comuna END DESC,
                CASE WHEN @Columna = 'Profesion' AND @Direccion = 'ASC' THEN Profesion END ASC,
                CASE WHEN @Columna = 'Profesion' AND @Direccion = 'DESC' THEN Profesion END DESC,
                ID_Interno ASC
            ) AS RowNum
        FROM Clientes_Match
        WHERE (@ID_Interno = '' OR ID_Interno LIKE '%' + @ID_Interno + '%')
          AND (@Nombre = '' OR Nombre LIKE '%' + @Nombre + '%')
          AND (@Telefono = '' OR Telefono LIKE '%' + @Telefono + '%')
          AND (@Correo = '' OR Correo LIKE '%' + @Correo + '%')
          AND (@Tipo_Match = '' OR Tipo_Match LIKE '%' + @Tipo_Match + '%')
          AND (@SearchSafe IS NULL
               OR ID_Interno LIKE '%' + @SearchSafe + '%'
               OR Nombre LIKE '%' + @SearchSafe + '%'
               OR Telefono LIKE '%' + @SearchSafe + '%'
               OR Correo LIKE '%' + @SearchSafe + '%'
               OR Rut LIKE '%' + @SearchSafe + '%'
               OR Comuna LIKE '%' + @SearchSafe + '%')
    )
    SELECT
        ID_Interno, Tipo_Match, Nombre, Rut, Datos_adjuntos, Direccion, Comuna,
        Estado_Civil, Profesion, Telefono, Correo, Giro_Razon_Social, TotalRowCount
    FROM Filtered
    WHERE RowNum BETWEEN @Min AND @Max
    ORDER BY RowNum;
END
GO

GRANT EXECUTE ON PP_psnp_ClientesMatch_SelectAll TO PUBLIC;
GO
