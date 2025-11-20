-- =============================================
-- Stored Procedure: SP_ObtenerPropiedadesConClics
-- Descripción: Obtiene propiedades con información de clics para la vista de tarjetas
-- Parámetros:
--   @BuscarPropiedad: Término de búsqueda (opcional)
--   @OrdenarPor: Criterio de ordenamiento (clicks, precio, nombre)
--   @TopRegistros: Cantidad máxima de registros a retornar
-- =============================================

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'SP_ObtenerPropiedadesConClics')
    DROP PROCEDURE SP_ObtenerPropiedadesConClics
GO

CREATE PROCEDURE SP_ObtenerPropiedadesConClics
    @BuscarPropiedad NVARCHAR(255) = NULL,
    @OrdenarPor NVARCHAR(50) = 'clicks',
    @TopRegistros INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @SQL NVARCHAR(MAX);

    SET @SQL = '
    SELECT TOP ' + CAST(@TopRegistros AS NVARCHAR(10)) + '
        p.ID_Propiedad,
        p.Title AS Titulo,
        p.Direccion,
        p.Comuna,
        p.Region,
        p.Valor AS Precio,
        p.Tipo_elemento AS TipoPropiedad,
        p.Dormitorios,
        p.Banos,
        p.M2_Construidos AS MetrosConstruidos,
        p.M2_Terreno AS MetrosTerreno,
        p.Agente_Responsable,
        p.Url_Imagen,
        ISNULL((SELECT COUNT(*) FROM Clics_Portales cp WHERE cp.ID_Propiedad = p.ID_Propiedad AND cp.NombrePortal = ''PortalInmobiliario.com''), 0) AS ClicsPortalInmobiliario,
        ISNULL((SELECT COUNT(*) FROM Clics_Portales cp WHERE cp.ID_Propiedad = p.ID_Propiedad AND cp.NombrePortal = ''Proppit''), 0) AS ClicsProppit,
        ISNULL((SELECT COUNT(*) FROM Clics_Portales cp WHERE cp.ID_Propiedad = p.ID_Propiedad AND cp.NombrePortal = ''ChilePropiedades''), 0) AS ClicsChilePropiedades,
        ISNULL((SELECT COUNT(*) FROM Clics_Portales cp WHERE cp.ID_Propiedad = p.ID_Propiedad AND cp.NombrePortal = ''TocToc''), 0) AS ClicsTocToc,
        ISNULL((SELECT COUNT(*) FROM Clics_Portales cp WHERE cp.ID_Propiedad = p.ID_Propiedad), 0) AS TotalClicsTodosPortales,
        (SELECT MAX(Fecha_Clic) FROM Clics_Portales cp WHERE cp.ID_Propiedad = p.ID_Propiedad) AS UltimoClick
    FROM Propiedades p
    WHERE 1=1';

    -- Agregar filtro de búsqueda si existe
    IF @BuscarPropiedad IS NOT NULL AND @BuscarPropiedad != ''
    BEGIN
        SET @SQL = @SQL + ' 
        AND (p.ID_Propiedad LIKE ''%' + @BuscarPropiedad + '%'' 
             OR p.Title LIKE ''%' + @BuscarPropiedad + '%'' 
             OR p.Direccion LIKE ''%' + @BuscarPropiedad + '%''
             OR p.Comuna LIKE ''%' + @BuscarPropiedad + '%'')';
    END

    -- Agregar ordenamiento
    IF @OrdenarPor = 'precio'
    BEGIN
        SET @SQL = @SQL + ' ORDER BY p.Valor DESC';
    END
    ELSE IF @OrdenarPor = 'nombre'
    BEGIN
        SET @SQL = @SQL + ' ORDER BY p.Title ASC';
    END
    ELSE -- clicks (default)
    BEGIN
        SET @SQL = @SQL + ' ORDER BY TotalClicsTodosPortales DESC, p.Title ASC';
    END

    -- Ejecutar la consulta dinámica
    EXEC sp_executesql @SQL;
END
GO

-- Script de prueba
-- EXEC SP_ObtenerPropiedadesConClics NULL, 'clicks', 20
-- EXEC SP_ObtenerPropiedadesConClics 'Santiago', 'precio', 10
-- EXEC SP_ObtenerPropiedadesConClics NULL, 'nombre', 50

