-- ============================================================
-- Stored Procedure: SP_ConvertirLeadAClienteMatch
-- Descripción: Convierte un Lead en un Cliente Match
-- ============================================================
-- Autor: Sistema Inmobiliaria .NET 8
-- Fecha: 2025-01-04
-- Adaptado a la estructura real de Clientes_Match
-- ============================================================

-- Eliminar el procedimiento si ya existe
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_ConvertirLeadAClienteMatch')
BEGIN
    DROP PROCEDURE SP_ConvertirLeadAClienteMatch;
    PRINT '✓ Procedimiento anterior eliminado correctamente';
END
GO

-- Crear el procedimiento
CREATE PROCEDURE [dbo].[SP_ConvertirLeadAClienteMatch]
    -- Parámetros de entrada
    @ID_Cliente NVARCHAR(50),
    @Nombres NVARCHAR(100),
    @Apellidos NVARCHAR(100),
    @Correo_Electronico NVARCHAR(100),
    @Telefono NVARCHAR(20),
    @Sexo NVARCHAR(1),
    @Portal NVARCHAR(100),
    @Asistente NVARCHAR(100),
    @ID_Unidad_Consultada NVARCHAR(50),
    @Unidad_Consultada NVARCHAR(500),
    @Respuesta NVARCHAR(MAX),
    @Fecha_Contacto DATETIME,
    
    -- Parámetro de salida para indicar el resultado
    @Resultado INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Variables locales
    DECLARE @Error NVARCHAR(MAX);
    DECLARE @ExisteCliente INT;
    DECLARE @NombreCompleto NVARCHAR(200);
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Verificar si ya existe un Cliente Match con ese ID_Interno
        SELECT @ExisteCliente = COUNT(*)
        FROM Clientes_Match
        WHERE ID_Interno = @ID_Cliente;
        
        IF @ExisteCliente > 0
        BEGIN
            -- El cliente ya existe en Clientes_Match
            SET @Resultado = 0;
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Concatenar nombre completo
        SET @NombreCompleto = LTRIM(RTRIM(ISNULL(@Nombres, '') + ' ' + ISNULL(@Apellidos, '')));
        
        -- Insertar el nuevo Cliente Match adaptado a la estructura real
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
            @ID_Cliente,                           -- ID_Interno
            'Lead Convertido',                     -- Tipo_Match
            @NombreCompleto,                       -- Nombre (nombre completo)
            NULL,                                   -- Rut (no disponible desde Lead)
            @Respuesta,                            -- Datos_adjuntos (usamos la respuesta)
            NULL,                                   -- Direccion (no disponible)
            NULL,                                   -- Comuna (no disponible)
            NULL,                                   -- Estado_Civil (no disponible)
            NULL,                                   -- Profesion (no disponible)
            @Telefono,                             -- Telefono
            @Correo_Electronico,                   -- Correo
            @Portal + ' - ' + ISNULL(@Unidad_Consultada, 'Sin propiedad') -- Giro_Razon_Social (info del portal y propiedad)
        );
        
        -- Actualizar el estado del Lead a RESERVA
        UPDATE Clientes_Leads
        SET Seguimiento = 'RESERVA'
        WHERE ID_Cliente = @ID_Cliente;
        
        -- Confirmación exitosa
        SET @Resultado = 1;
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        -- Si ocurre algún error, revertir la transacción
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        -- Capturar el error
        SET @Error = ERROR_MESSAGE();
        SET @Resultado = -1;
        
        -- Registrar el error (opcional, puedes descomentar si tienes una tabla de logs)
        -- INSERT INTO ErrorLog (Fecha, Procedimiento, Mensaje)
        -- VALUES (GETDATE(), 'SP_ConvertirLeadAClienteMatch', @Error);
        
        -- Re-lanzar el error para que sea capturado por la aplicación
        RAISERROR(@Error, 16, 1);
    END CATCH
END
GO

-- Otorgar permisos de ejecución (ajusta según tu esquema de seguridad)
GRANT EXECUTE ON SP_ConvertirLeadAClienteMatch TO PUBLIC;
GO

PRINT '';
PRINT '=========================================================';
PRINT '✓ Stored Procedure SP_ConvertirLeadAClienteMatch creado exitosamente';
PRINT '=========================================================';
PRINT '';
PRINT 'ADAPTADO A LA ESTRUCTURA REAL DE Clientes_Match:';
PRINT '  - ID_Interno: ID del cliente';
PRINT '  - Tipo_Match: "Lead Convertido"';
PRINT '  - Nombre: Nombre completo (Nombres + Apellidos)';
PRINT '  - Telefono: Teléfono del lead';
PRINT '  - Correo: Email del lead';
PRINT '  - Datos_adjuntos: Respuesta del lead';
PRINT '  - Giro_Razon_Social: Portal + Propiedad consultada';
PRINT '';
PRINT 'Códigos de resultado (@Resultado):';
PRINT '  1  = Conversión exitosa';
PRINT '  0  = Cliente ya existe en Clientes_Match';
PRINT '  -1 = Error durante la ejecución';
PRINT '';
PRINT 'Ejemplo de uso:';
PRINT 'DECLARE @Res INT;';
PRINT 'EXEC SP_ConvertirLeadAClienteMatch';
PRINT '    @ID_Cliente = ''LEAD001'',';
PRINT '    @Nombres = ''Juan'',';
PRINT '    @Apellidos = ''Pérez'',';
PRINT '    @Correo_Electronico = ''juan@test.com'',';
PRINT '    @Telefono = ''+56912345678'',';
PRINT '    @Sexo = ''M'',';
PRINT '    @Portal = ''Portal Inmobiliario'',';
PRINT '    @Asistente = ''María González'',';
PRINT '    @ID_Unidad_Consultada = ''PROP001'',';
PRINT '    @Unidad_Consultada = ''Depto 101'',';
PRINT '    @Respuesta = ''Cliente interesado'',';
PRINT '    @Fecha_Contacto = ''2025-01-04'',';
PRINT '    @Resultado = @Res OUTPUT;';
PRINT 'SELECT @Res;';
PRINT '';
PRINT '=========================================================';
GO
