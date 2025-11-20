-- ============================================================
-- Stored Procedure: PP_psnp_ClientesMatch_SelectAll
-- Descripción: Obtiene todos los Clientes Match con filtros y ordenamiento
-- ============================================================
-- Autor: Sistema Inmobiliaria .NET 8
-- Fecha: 2025-01-07
-- Basado en la estructura real de Clientes_Match
-- ============================================================

-- Eliminar el procedimiento si ya existe
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_ClientesMatch_SelectAll')
BEGIN
    DROP PROCEDURE PP_psnp_ClientesMatch_SelectAll;
    PRINT '✓ Procedimiento anterior eliminado correctamente';
END
GO

-- Crear el procedimiento
CREATE PROCEDURE [dbo].[PP_psnp_ClientesMatch_SelectAll]
    -- Parámetros de entrada para filtros
    @ID_Interno NVARCHAR(50) = '',
    @Nombre NVARCHAR(200) = '',
    @Telefono NVARCHAR(20) = '',
    @Correo NVARCHAR(100) = '',
    @Tipo_Match NVARCHAR(100) = '',
    @Columna NVARCHAR(50) = 'ID_Interno',
    @Direccion NVARCHAR(4) = 'ASC'
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Construir la consulta dinámica con filtros
        DECLARE @SQL NVARCHAR(MAX);
        DECLARE @OrderBy NVARCHAR(100);
        
        -- Validar columna de ordenamiento
        IF @Columna NOT IN ('ID_Interno', 'Nombre', 'Telefono', 'Correo', 'Tipo_Match', 'Rut')
            SET @Columna = 'ID_Interno';
            
        -- Validar dirección de ordenamiento
        IF @Direccion NOT IN ('ASC', 'DESC')
            SET @Direccion = 'ASC';
            
        SET @OrderBy = @Columna + ' ' + @Direccion;
        
        -- Consulta base
        SET @SQL = N'
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
            Giro_Razon_Social
        FROM Clientes_Match
        WHERE 1=1';
        
        -- Aplicar filtros dinámicos
        IF @ID_Interno <> ''
            SET @SQL = @SQL + ' AND ID_Interno LIKE ''%' + @ID_Interno + '%''';
            
        IF @Nombre <> ''
            SET @SQL = @SQL + ' AND Nombre LIKE ''%' + @Nombre + '%''';
            
        IF @Telefono <> ''
            SET @SQL = @SQL + ' AND Telefono LIKE ''%' + @Telefono + '%''';
            
        IF @Correo <> ''
            SET @SQL = @SQL + ' AND Correo LIKE ''%' + @Correo + '%''';
            
        IF @Tipo_Match <> ''
            SET @SQL = @SQL + ' AND Tipo_Match LIKE ''%' + @Tipo_Match + '%''';
        
        -- Agregar ordenamiento
        SET @SQL = @SQL + ' ORDER BY ' + @OrderBy;
        
        -- Ejecutar consulta
        EXEC sp_executesql @SQL;
        
    END TRY
    BEGIN CATCH
        -- Si ocurre algún error, lanzarlo
        DECLARE @Error NVARCHAR(MAX);
        SET @Error = ERROR_MESSAGE();
        RAISERROR(@Error, 16, 1);
    END CATCH
END
GO

-- Otorgar permisos de ejecución
GRANT EXECUTE ON PP_psnp_ClientesMatch_SelectAll TO PUBLIC;
GO

PRINT '';
PRINT '=========================================================';
PRINT '✓ Stored Procedure PP_psnp_ClientesMatch_SelectAll creado exitosamente';
PRINT '=========================================================';
PRINT '';
PRINT 'Parámetros de entrada:';
PRINT '  @ID_Interno: Filtro por ID (parcial)';
PRINT '  @Nombre: Filtro por nombre (parcial)';
PRINT '  @Telefono: Filtro por teléfono (parcial)';
PRINT '  @Correo: Filtro por correo (parcial)';
PRINT '  @Tipo_Match: Filtro por tipo de match (parcial)';
PRINT '  @Columna: Columna para ordenamiento (default: ID_Interno)';
PRINT '  @Direccion: ASC o DESC (default: ASC)';
PRINT '';
PRINT 'Ejemplo de uso:';
PRINT 'EXEC PP_psnp_ClientesMatch_SelectAll';
PRINT '    @Nombre = ''Juan'',';
PRINT '    @Tipo_Match = ''Lead Convertido'',';
PRINT '    @Columna = ''Nombre'',';
PRINT '    @Direccion = ''ASC'';';
PRINT '';
PRINT '=========================================================';
GO

