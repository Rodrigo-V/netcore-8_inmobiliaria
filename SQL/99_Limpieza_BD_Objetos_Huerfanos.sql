-- ================================================================
-- LIMPIEZA BD JCF_DEV - Objetos huérfanos / no usados por .NET 8
-- ================================================================
-- Base: JCF_DEV
-- Fecha: 2026-06-29
--
-- IMPORTANTE:
--   1. Ejecutar primero la sección BACKUP (recomendado).
--   2. Revisar sección OPCIONAL antes de dropear tablas con datos.
--   3. NO toca tablas/SPs del núcleo operativo ni Mercado Libre.
--
-- Uso:
--   sqlcmd -S "SERVIDOR\INSTANCIA" -E -d JCF_DEV -i 99_Limpieza_BD_Objetos_Huerfanos.sql
-- ================================================================

SET NOCOUNT ON;
PRINT '=== INICIO LIMPIEZA BD ===';
PRINT 'Base de datos: ' + DB_NAME();
PRINT 'Fecha: ' + CONVERT(VARCHAR(20), GETDATE(), 120);
GO

-- ================================================================
-- SECCIÓN 0: BACKUP DE TABLAS CON DATOS (opcional pero recomendado)
-- Descomentar si quieres respaldar antes de eliminar tablas legacy
-- ================================================================
/*
IF OBJECT_ID('dbo._BAK_Clientes', 'U') IS NULL
    SELECT * INTO dbo._BAK_Clientes FROM dbo.Clientes;

IF OBJECT_ID('dbo._BAK_Clientes_Matriz', 'U') IS NULL
    SELECT * INTO dbo._BAK_Clientes_Matriz FROM dbo.Clientes_Matriz;

IF OBJECT_ID('dbo._BAK_Requerimientos', 'U') IS NULL
    SELECT * INTO dbo._BAK_Requerimientos FROM dbo.Requerimientos;

IF OBJECT_ID('dbo._BAK_Visitas', 'U') IS NULL AND OBJECT_ID('dbo.Visitas', 'U') IS NOT NULL
    SELECT * INTO dbo._BAK_Visitas FROM dbo.Visitas;

PRINT 'Backups _BAK_* creados.';
GO
*/

-- ================================================================
-- SECCIÓN 1: SPs PRODUCTO (tabla Producto no existe en BD)
-- ================================================================
PRINT '';
PRINT '--- Sección 1: SPs Producto ---';

IF OBJECT_ID('dbo.PP_psnp_Producto_AgregaxId', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_Producto_AgregaxId; PRINT '  DROP PP_psnp_Producto_AgregaxId'; END

IF OBJECT_ID('dbo.PP_psnp_Producto_EliminarxId', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_Producto_EliminarxId; PRINT '  DROP PP_psnp_Producto_EliminarxId'; END

IF OBJECT_ID('dbo.PP_psnp_Producto_ModificaxId', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_Producto_ModificaxId; PRINT '  DROP PP_psnp_Producto_ModificaxId'; END

IF OBJECT_ID('dbo.PP_psnp_Producto_SelectxPK', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_Producto_SelectxPK; PRINT '  DROP PP_psnp_Producto_SelectxPK'; END

IF OBJECT_ID('dbo.PP_psnp_Producto_SelectxTipoEmpresa', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_Producto_SelectxTipoEmpresa; PRINT '  DROP PP_psnp_Producto_SelectxTipoEmpresa'; END
GO

-- ================================================================
-- SECCIÓN 2: SPs CLIENTES MATRIZ (módulo no implementado en .NET)
-- ================================================================
PRINT '';
PRINT '--- Sección 2: SPs Clientes Matriz ---';

IF OBJECT_ID('dbo.PP_psnp_ClientesMatriz_AgregaxId', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_ClientesMatriz_AgregaxId; PRINT '  DROP PP_psnp_ClientesMatriz_AgregaxId'; END

IF OBJECT_ID('dbo.PP_psnp_ClientesMatriz_EliminarxId', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_ClientesMatriz_EliminarxId; PRINT '  DROP PP_psnp_ClientesMatriz_EliminarxId'; END

IF OBJECT_ID('dbo.PP_psnp_ClientesMatriz_ModificaxId', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_ClientesMatriz_ModificaxId; PRINT '  DROP PP_psnp_ClientesMatriz_ModificaxId'; END

IF OBJECT_ID('dbo.PP_psnp_ClientesMatriz_SelectxPK', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_ClientesMatriz_SelectxPK; PRINT '  DROP PP_psnp_ClientesMatriz_SelectxPK'; END

IF OBJECT_ID('dbo.PP_psnp_ClientesMatriz_SelectxTipoEmpresa', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_ClientesMatriz_SelectxTipoEmpresa; PRINT '  DROP PP_psnp_ClientesMatriz_SelectxTipoEmpresa'; END
GO

-- ================================================================
-- SECCIÓN 3: SPs REQUERIMIENTOS (módulo no implementado en .NET)
-- ================================================================
PRINT '';
PRINT '--- Sección 3: SPs Requerimientos ---';

IF OBJECT_ID('dbo.PP_psnp_Requerimiento_AgregaxId', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_Requerimiento_AgregaxId; PRINT '  DROP PP_psnp_Requerimiento_AgregaxId'; END

IF OBJECT_ID('dbo.PP_psnp_Requerimiento_EliminarxId', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_Requerimiento_EliminarxId; PRINT '  DROP PP_psnp_Requerimiento_EliminarxId'; END

IF OBJECT_ID('dbo.PP_psnp_Requerimiento_ModificarxId', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_Requerimiento_ModificarxId; PRINT '  DROP PP_psnp_Requerimiento_ModificarxId'; END

IF OBJECT_ID('dbo.PP_psnp_Requerimiento_SelectxPK', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_Requerimiento_SelectxPK; PRINT '  DROP PP_psnp_Requerimiento_SelectxPK'; END

IF OBJECT_ID('dbo.PP_psnp_Requerimiento_SelectxTipoEmpresa', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_Requerimiento_SelectxTipoEmpresa; PRINT '  DROP PP_psnp_Requerimiento_SelectxTipoEmpresa'; END
GO

-- ================================================================
-- SECCIÓN 4: SPs REDUNDANTES (la app usa otra vía)
-- ================================================================
PRINT '';
PRINT '--- Sección 4: SPs redundantes ---';

IF OBJECT_ID('dbo.PP_psnp_ClientesMatch_Insert', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_ClientesMatch_Insert; PRINT '  DROP PP_psnp_ClientesMatch_Insert (usa SP_ConvertirLeadAClienteMatch)'; END

IF OBJECT_ID('dbo.PP_psnp_Propiedad_ActualizarUrlImagen', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_Propiedad_ActualizarUrlImagen; PRINT '  DROP PP_psnp_Propiedad_ActualizarUrlImagen (usa UPDATE directo)'; END

IF OBJECT_ID('dbo.PP_psnp_ClientesLeads_Estadisticas', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.PP_psnp_ClientesLeads_Estadisticas; PRINT '  DROP PP_psnp_ClientesLeads_Estadisticas (usa SQL inline en servicio)'; END
GO

-- ================================================================
-- SECCIÓN 5: SPs DIAGNÓSTICO / UTILIDAD (no referenciados en .NET)
-- ================================================================
PRINT '';
PRINT '--- Sección 5: SPs diagnóstico y utilidad ---';

IF OBJECT_ID('dbo.SP_DiagnosticoDetallado', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_DiagnosticoDetallado; PRINT '  DROP SP_DiagnosticoDetallado'; END

IF OBJECT_ID('dbo.SP_DiagnosticoExcel', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_DiagnosticoExcel; PRINT '  DROP SP_DiagnosticoExcel'; END

IF OBJECT_ID('dbo.SP_BuscarPropiedadesAutocomplete', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_BuscarPropiedadesAutocomplete; PRINT '  DROP SP_BuscarPropiedadesAutocomplete'; END

IF OBJECT_ID('dbo.SP_ObtenerUrlImagenPropiedad', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_ObtenerUrlImagenPropiedad; PRINT '  DROP SP_ObtenerUrlImagenPropiedad'; END

IF OBJECT_ID('dbo.SP_ValidarRolPermitido', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_ValidarRolPermitido; PRINT '  DROP SP_ValidarRolPermitido'; END
GO

-- ================================================================
-- SECCIÓN 6: SPs CLICS/ACCIONES no usados por la app actual
-- (Mantiene: SP_ObtenerMatrizClics, SP_ObtenerPropiedadesConClics,
--  SP_ObtenerEstadisticasGenerales, SP_ObtenerDatosExcelMatriz, sp_InsertarClicPortal)
-- ================================================================
PRINT '';
PRINT '--- Sección 6: SPs clics/acciones no usados ---';

IF OBJECT_ID('dbo.SP_ObtenerResumenClicsPortales', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_ObtenerResumenClicsPortales; PRINT '  DROP SP_ObtenerResumenClicsPortales'; END

IF OBJECT_ID('dbo.SP_ObtenerResumenClicsPorPortal', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_ObtenerResumenClicsPorPortal; PRINT '  DROP SP_ObtenerResumenClicsPorPortal'; END

IF OBJECT_ID('dbo.SP_ObtenerClicsPorPropiedad', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_ObtenerClicsPorPropiedad; PRINT '  DROP SP_ObtenerClicsPorPropiedad'; END

IF OBJECT_ID('dbo.SP_ObtenerClicsDetalladosPorPropiedad', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_ObtenerClicsDetalladosPorPropiedad; PRINT '  DROP SP_ObtenerClicsDetalladosPorPropiedad'; END

IF OBJECT_ID('dbo.SP_ObtenerEstadisticasClicsPorPropiedad', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_ObtenerEstadisticasClicsPorPropiedad; PRINT '  DROP SP_ObtenerEstadisticasClicsPorPropiedad'; END

IF OBJECT_ID('dbo.SP_ObtenerDatosExcelSincronizacion', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_ObtenerDatosExcelSincronizacion; PRINT '  DROP SP_ObtenerDatosExcelSincronizacion'; END

IF OBJECT_ID('dbo.SP_ObtenerEstadisticasAcciones', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_ObtenerEstadisticasAcciones; PRINT '  DROP SP_ObtenerEstadisticasAcciones'; END

IF OBJECT_ID('dbo.SP_RegistrarAccionAgente', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_RegistrarAccionAgente; PRINT '  DROP SP_RegistrarAccionAgente'; END

IF OBJECT_ID('dbo.SP_EliminarAccion', 'P') IS NOT NULL
BEGIN DROP PROCEDURE dbo.SP_EliminarAccion; PRINT '  DROP SP_EliminarAccion'; END
GO

-- ================================================================
-- SECCIÓN 7: TABLA VISITAS (vacía, sin uso en .NET)
-- ================================================================
PRINT '';
PRINT '--- Sección 7: Tabla Visitas ---';

IF OBJECT_ID('dbo.Visitas', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Visitas;
    PRINT '  DROP TABLE Visitas';
END
ELSE
    PRINT '  Visitas: no existe (omitido)';
GO

-- ================================================================
-- SECCIÓN 8: TABLAS LEGACY CON DATOS (COMENTADO - descomentar tras backup)
-- Clientes (5), Clientes_Matriz (95), Requerimientos (97)
-- ================================================================
PRINT '';
PRINT '--- Sección 8: Tablas legacy (OMITIDA por defecto) ---';
PRINT '  Para eliminar Clientes, Clientes_Matriz, Requerimientos:';
PRINT '  1) Descomentar sección 0 (backup)';
PRINT '  2) Descomentar bloque siguiente';

/*
-- Orden: Requerimientos -> Clientes_Matriz -> Clientes (por posibles FKs lógicas)
IF OBJECT_ID('dbo.Requerimientos', 'U') IS NOT NULL
BEGIN DROP TABLE dbo.Requerimientos; PRINT '  DROP TABLE Requerimientos'; END

IF OBJECT_ID('dbo.Clientes_Matriz', 'U') IS NOT NULL
BEGIN DROP TABLE dbo.Clientes_Matriz; PRINT '  DROP TABLE Clientes_Matriz'; END

IF OBJECT_ID('dbo.Clientes', 'U') IS NOT NULL
BEGIN DROP TABLE dbo.Clientes; PRINT '  DROP TABLE Clientes'; END
GO
*/

-- ================================================================
-- SECCIÓN 9: VERIFICACIÓN POST-LIMPIEZA
-- ================================================================
PRINT '';
PRINT '=== VERIFICACIÓN POST-LIMPIEZA ===';

SELECT 'Tablas restantes' AS Tipo, COUNT(*) AS Cantidad
FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

SELECT ROUTINE_NAME AS Procedimiento
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
  AND ROUTINE_NAME IN (
    'PP_psnp_Producto_SelectxPK',
    'PP_psnp_ClientesMatriz_SelectxPK',
    'PP_psnp_Requerimiento_SelectxPK',
    'PP_psnp_ClientesMatch_Insert',
    'SP_DiagnosticoExcel',
    'SP_ObtenerResumenClicsPortales'
  );
-- Si no devuelve filas, la limpieza de esos objetos fue exitosa

PRINT '';
PRINT '=== FIN LIMPIEZA BD ===';
PRINT 'Objetos NO eliminados (nucleo activo):';
PRINT '  Propiedades, Clientes_Leads, Clientes_Match, Seguimiento_Activo,';
PRINT '  Usuarios, ClicsPortales, PortalesInmobiliarios, APIConfiguracion,';
PRINT '  APISincronizacion, APITestResult';
PRINT '';
PRINT 'SPs Seguimiento conservados (CRUD futuro):';
PRINT '  PP_psnp_Seguimiento_Activo_ModificaxId';
PRINT '  PP_psnp_Seguimiento_Activo_EliminarxId';
PRINT '  PP_psnp_Seguimiento_Activo_SelectxPK';
GO
