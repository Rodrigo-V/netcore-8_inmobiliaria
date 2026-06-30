-- =============================================
-- Script: Tabla y Stored Procedures para Mercado Libre
-- Descripción: Gestión de tokens y conexión con API de Mercado Libre
-- =============================================

-- =============================================
-- 1. CREAR TABLA PARA TOKENS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MercadoLibre_Tokens')
BEGIN
    CREATE TABLE MercadoLibre_Tokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId BIGINT NOT NULL,
        AccessToken VARCHAR(500) NOT NULL,
        RefreshToken VARCHAR(500) NOT NULL,
        FechaExpiracion DATETIME NOT NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        FechaActualizacion DATETIME NULL,
        Activo BIT NOT NULL DEFAULT 1,
        CONSTRAINT UC_MercadoLibre_UserId UNIQUE (UserId)
    );
    
    PRINT 'Tabla MercadoLibre_Tokens creada correctamente';
END
ELSE
BEGIN
    PRINT 'La tabla MercadoLibre_Tokens ya existe';
END
GO

-- =============================================
-- 2. SP PARA GUARDAR TOKEN
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_MercadoLibre_GuardarToken')
    DROP PROCEDURE PP_psnp_MercadoLibre_GuardarToken
GO

CREATE PROCEDURE PP_psnp_MercadoLibre_GuardarToken
    @UserId BIGINT,
    @AccessToken VARCHAR(500),
    @RefreshToken VARCHAR(500),
    @ExpiresIn INT -- Segundos hasta que expire (normalmente 21600 = 6 horas)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Desactivar tokens anteriores del mismo usuario
        UPDATE MercadoLibre_Tokens
        SET Activo = 0,
            FechaActualizacion = GETDATE()
        WHERE UserId = @UserId;
        
        -- Calcular fecha de expiración
        DECLARE @FechaExpiracion DATETIME = DATEADD(SECOND, @ExpiresIn, GETDATE());
        
        -- Insertar nuevo token
        INSERT INTO MercadoLibre_Tokens (
            UserId,
            AccessToken,
            RefreshToken,
            FechaExpiracion,
            FechaCreacion,
            Activo
        )
        VALUES (
            @UserId,
            @AccessToken,
            @RefreshToken,
            @FechaExpiracion,
            GETDATE(),
            1
        );
        
        SELECT 
            Id,
            UserId,
            AccessToken,
            RefreshToken,
            FechaExpiracion,
            FechaCreacion,
            Activo
        FROM MercadoLibre_Tokens
        WHERE Id = SCOPE_IDENTITY();
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage VARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- =============================================
-- 3. SP PARA ACTUALIZAR TOKEN
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_MercadoLibre_ActualizarToken')
    DROP PROCEDURE PP_psnp_MercadoLibre_ActualizarToken
GO

CREATE PROCEDURE PP_psnp_MercadoLibre_ActualizarToken
    @UserId BIGINT,
    @AccessToken VARCHAR(500),
    @RefreshToken VARCHAR(500),
    @ExpiresIn INT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Calcular nueva fecha de expiración
        DECLARE @FechaExpiracion DATETIME = DATEADD(SECOND, @ExpiresIn, GETDATE());
        
        -- Actualizar token existente
        UPDATE MercadoLibre_Tokens
        SET AccessToken = @AccessToken,
            RefreshToken = @RefreshToken,
            FechaExpiracion = @FechaExpiracion,
            FechaActualizacion = GETDATE(),
            Activo = 1
        WHERE UserId = @UserId;
        
        -- Si no existe, insertar nuevo
        IF @@ROWCOUNT = 0
        BEGIN
            INSERT INTO MercadoLibre_Tokens (
                UserId,
                AccessToken,
                RefreshToken,
                FechaExpiracion,
                FechaCreacion,
                Activo
            )
            VALUES (
                @UserId,
                @AccessToken,
                @RefreshToken,
                @FechaExpiracion,
                GETDATE(),
                1
            );
        END
        
        SELECT 
            Id,
            UserId,
            AccessToken,
            RefreshToken,
            FechaExpiracion,
            FechaCreacion,
            FechaActualizacion,
            Activo
        FROM MercadoLibre_Tokens
        WHERE UserId = @UserId AND Activo = 1;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage VARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- =============================================
-- 4. SP PARA OBTENER TOKEN ACTIVO
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_MercadoLibre_ObtenerTokenActivo')
    DROP PROCEDURE PP_psnp_MercadoLibre_ObtenerTokenActivo
GO

CREATE PROCEDURE PP_psnp_MercadoLibre_ObtenerTokenActivo
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT TOP 1
            Id,
            UserId,
            AccessToken,
            RefreshToken,
            FechaExpiracion,
            FechaCreacion,
            FechaActualizacion,
            Activo
        FROM MercadoLibre_Tokens
        WHERE Activo = 1
        ORDER BY FechaCreacion DESC;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage VARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- =============================================
-- 5. SP PARA DESACTIVAR TOKEN
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_MercadoLibre_DesactivarToken')
    DROP PROCEDURE PP_psnp_MercadoLibre_DesactivarToken
GO

CREATE PROCEDURE PP_psnp_MercadoLibre_DesactivarToken
    @UserId BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE MercadoLibre_Tokens
        SET Activo = 0,
            FechaActualizacion = GETDATE()
        WHERE UserId = @UserId;
        
        SELECT @@ROWCOUNT AS FilasAfectadas;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage VARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- =============================================
-- 6. SP PARA OBTENER ESTADO DE CONEXIÓN
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_MercadoLibre_ObtenerEstado')
    DROP PROCEDURE PP_psnp_MercadoLibre_ObtenerEstado
GO

CREATE PROCEDURE PP_psnp_MercadoLibre_ObtenerEstado
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        SELECT TOP 1
            Id,
            UserId,
            FechaExpiracion,
            DATEDIFF(DAY, GETDATE(), FechaExpiracion) AS DiasRestantes,
            CASE 
                WHEN FechaExpiracion > GETDATE() THEN 1
                ELSE 0
            END AS Vigente,
            Activo,
            FechaCreacion,
            FechaActualizacion
        FROM MercadoLibre_Tokens
        WHERE Activo = 1
        ORDER BY FechaCreacion DESC;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage VARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

-- =============================================
-- 7. SP PARA LIMPIAR TOKENS ANTIGUOS
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_MercadoLibre_LimpiarTokensAntiguos')
    DROP PROCEDURE PP_psnp_MercadoLibre_LimpiarTokensAntiguos
GO

CREATE PROCEDURE PP_psnp_MercadoLibre_LimpiarTokensAntiguos
    @DiasAntiguedad INT = 180 -- Por defecto elimina tokens de más de 6 meses
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        DELETE FROM MercadoLibre_Tokens
        WHERE Activo = 0
          AND FechaCreacion < DATEADD(DAY, -@DiasAntiguedad, GETDATE());
        
        SELECT @@ROWCOUNT AS TokensEliminados;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage VARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO

PRINT 'Todos los Stored Procedures de Mercado Libre fueron creados exitosamente';
GO

