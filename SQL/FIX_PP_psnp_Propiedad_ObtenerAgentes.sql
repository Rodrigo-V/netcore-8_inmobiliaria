USE [JCF_DEV]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'PP_psnp_Propiedad_ObtenerAgentes')
    DROP PROCEDURE PP_psnp_Propiedad_ObtenerAgentes
GO

CREATE PROCEDURE [dbo].[PP_psnp_Propiedad_ObtenerAgentes]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT Nombres + ' ' + Apellidos AS Agente
    FROM Usuarios
    WHERE Activo = 1
    ORDER BY Agente;
END
GO

PRINT '✓ SP PP_psnp_Propiedad_ObtenerAgentes creado exitosamente';
GO

