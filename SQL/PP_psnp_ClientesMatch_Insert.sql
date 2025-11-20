-- ============================================================
-- Stored Procedure: PP_psnp_ClientesMatch_Insert
-- Descripción: Inserta un nuevo Cliente Match
-- ============================================================
-- Autor: Sistema Inmobiliaria .NET 8
-- Fecha: 2025-01-07
-- Basado en la estructura real de Clientes_Match
-- ============================================================

-- Eliminar el procedimiento si ya existe
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_ClientesMatch_Insert')
BEGIN
    DROP PROCEDURE PP_psnp_ClientesMatch_Insert;
    PRINT '✓ Procedimiento anterior eliminado correctamente';
END
GO

-- Crear el procedimiento
CREATE PROCEDURE [dbo].[PP_psnp_ClientesMatch_Insert]
    -- Parámetros de entrada
    @ID_Interno NVARCHAR(50),
    @Tipo_Match NVARCHAR(100),
    @Nombre NVARCHAR(200),
    @Rut NVARCHAR(20) = NULL,
    @Datos_adjuntos NVARCHAR(MAX) = NULL,
    @Direccion NVARCHAR(200) = NULL,
    @Comuna NVARCHAR(100) = NULL,
    @Estado_Civil NVARCHAR(50) = NULL,
    @Profesion NVARCHAR(100) = NULL,
    @Telefono NVARCHAR(20) = NULL,
    @Correo NVARCHAR(100) = NULL,
    @Giro_Razon_Social NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificar si ya existe un Cliente Match con ese ID_Interno
        IF EXISTS (SELECT 1 FROM Clientes_Match WHERE ID_Interno = @ID_Interno)
        BEGIN
            RAISERROR('Ya existe un Cliente Match con ese ID_Interno', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Insertar el nuevo Cliente Match
        INSERT INTO Clientes_Match (
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
        )
        VALUES (
            @ID_Interno,
            @Tipo_Match,
            @Nombre,
            @Rut,
            @Datos_adjuntos,
            @Direccion,
            @Comuna,
            @Estado_Civil,
            @Profesion,
            @Telefono,
            @Correo,
            @Giro_Razon_Social
        );
        
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
GRANT EXECUTE ON PP_psnp_ClientesMatch_Insert TO PUBLIC;
GO

PRINT '';
PRINT '=========================================================';
PRINT '✓ Stored Procedure PP_psnp_ClientesMatch_Insert creado exitosamente';
PRINT '=========================================================';
PRINT '';
PRINT 'Parámetros de entrada:';
PRINT '  @ID_Interno: ID único del Cliente Match (Requerido)';
PRINT '  @Tipo_Match: Tipo de match (Requerido)';
PRINT '  @Nombre: Nombre completo (Requerido)';
PRINT '  @Rut: RUT (Opcional)';
PRINT '  @Datos_adjuntos: Datos adicionales (Opcional)';
PRINT '  @Direccion: Dirección (Opcional)';
PRINT '  @Comuna: Comuna (Opcional)';
PRINT '  @Estado_Civil: Estado civil (Opcional)';
PRINT '  @Profesion: Profesión (Opcional)';
PRINT '  @Telefono: Teléfono (Opcional)';
PRINT '  @Correo: Correo electrónico (Opcional)';
PRINT '  @Giro_Razon_Social: Giro o razón social (Opcional)';
PRINT '';
PRINT '=========================================================';
GO

