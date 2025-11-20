-- ============================================================
-- Stored Procedure: PP_psnp_ClientesMatch_Delete
-- Descripción: Elimina un Cliente Match
-- ============================================================
-- Autor: Sistema Inmobiliaria .NET 8
-- Fecha: 2025-01-07
-- Basado en la estructura real de Clientes_Match
-- ============================================================

-- Eliminar el procedimiento si ya existe
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_ClientesMatch_Delete')
BEGIN
    DROP PROCEDURE PP_psnp_ClientesMatch_Delete;
    PRINT '✓ Procedimiento anterior eliminado correctamente';
END
GO

-- Crear el procedimiento
CREATE PROCEDURE [dbo].[PP_psnp_ClientesMatch_Delete]
    -- Parámetro de entrada
    @ID_Interno NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificar si existe el Cliente Match
        IF NOT EXISTS (SELECT 1 FROM Clientes_Match WHERE ID_Interno = @ID_Interno)
        BEGIN
            RAISERROR('No existe un Cliente Match con ese ID_Interno', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Eliminar el Cliente Match
        DELETE FROM Clientes_Match
        WHERE ID_Interno = @ID_Interno;
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        DECLARE @Error NVARCHAR(MAX);
        SET @Error = ERROR_MESSAGE();
        RAISERROR(@Error, 16, 1);
    END CATCH
END
GO

-- Otorgar permisos de ejecución
GRANT EXECUTE ON PP_psnp_ClientesMatch_Delete TO PUBLIC;
GO

PRINT '';
PRINT '=========================================================';
PRINT '✓ Stored Procedure PP_psnp_ClientesMatch_Delete creado exitosamente';
PRINT '=========================================================';
PRINT '';
PRINT 'Parámetros de entrada:';
PRINT '  @ID_Interno: ID del Cliente Match a eliminar (Requerido)';
PRINT '';
PRINT 'Ejemplo de uso:';
PRINT 'EXEC PP_psnp_ClientesMatch_Delete @ID_Interno = ''MATCH001'';';
PRINT '';
PRINT '=========================================================';
GO

