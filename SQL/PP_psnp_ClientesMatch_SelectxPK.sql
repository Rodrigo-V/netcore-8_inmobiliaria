-- ============================================================
-- Stored Procedure: PP_psnp_ClientesMatch_SelectxPK
-- Descripción: Obtiene un Cliente Match por su ID_Interno (Primary Key)
-- ============================================================
-- Autor: Sistema Inmobiliaria .NET 8
-- Fecha: 2025-01-07
-- Basado en la estructura real de Clientes_Match
-- ============================================================

-- Eliminar el procedimiento si ya existe
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_ClientesMatch_SelectxPK')
BEGIN
    DROP PROCEDURE PP_psnp_ClientesMatch_SelectxPK;
    PRINT '✓ Procedimiento anterior eliminado correctamente';
END
GO

-- Crear el procedimiento
CREATE PROCEDURE [dbo].[PP_psnp_ClientesMatch_SelectxPK]
    -- Parámetro de entrada
    @ID_Interno NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
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
        WHERE ID_Interno = @ID_Interno;
        
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
GRANT EXECUTE ON PP_psnp_ClientesMatch_SelectxPK TO PUBLIC;
GO

PRINT '';
PRINT '=========================================================';
PRINT '✓ Stored Procedure PP_psnp_ClientesMatch_SelectxPK creado exitosamente';
PRINT '=========================================================';
PRINT '';
PRINT 'Parámetros de entrada:';
PRINT '  @ID_Interno: ID del Cliente Match (PK)';
PRINT '';
PRINT 'Ejemplo de uso:';
PRINT 'EXEC PP_psnp_ClientesMatch_SelectxPK @ID_Interno = ''MATCH001'';';
PRINT '';
PRINT '=========================================================';
GO

