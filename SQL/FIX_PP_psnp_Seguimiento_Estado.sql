IF OBJECT_ID('dbo.PP_psnp_Seguimiento_Activo_Select', 'P') IS NOT NULL
    DROP PROCEDURE dbo.PP_psnp_Seguimiento_Activo_Select;
GO

CREATE PROCEDURE [dbo].[PP_psnp_Seguimiento_Activo_Select]
  @Agente           nvarchar(50) = NULL,
  @ID_Cliente       nvarchar(50) = NULL,
  @Tipo_Accion      nvarchar(50) = NULL,
  @Estado           nvarchar(50) = NULL,
  @FechaDesde       date         = NULL,
  @FechaHasta       date         = NULL,
  @ID_Propiedad     nvarchar(50) = NULL,
  @Min              int,
  @Max              int,
  @Columna          sysname      = NULL,
  @Direccion        varchar(4)   = 'DESC'
AS
BEGIN
  SET NOCOUNT ON;

  IF @Columna IS NULL OR @Columna NOT IN ('ID_Cliente','Agente','Fecha_Accion','Tipo_Accion','Estado','Resultado')
    SET @Columna = 'Fecha_Accion';

  IF UPPER(@Direccion) NOT IN ('ASC','DESC')
    SET @Direccion = 'DESC';

  DECLARE @sql nvarchar(max) = N'
  SELECT *
  FROM (
    SELECT
      ID_Cliente, ID_Propiedad, Agente, Codigo_Agente, Fecha_Accion, Tipo_Accion,
      Descripcion_Accion, Resultado, Estado, Fecha_Proximo_Contacto,
      COUNT(*) OVER() AS TotalRowCount,
      ROW_NUMBER() OVER (ORDER BY ' + QUOTENAME(@Columna) + N' ' + CASE WHEN UPPER(@Direccion)='ASC' THEN 'ASC' ELSE 'DESC' END + N') AS rn
    FROM Seguimiento_Activo
    WHERE 1=1'
    + CASE WHEN @Agente IS NULL OR LTRIM(RTRIM(@Agente)) = '' THEN '' ELSE N' AND Agente LIKE ''%'' + @pAgente + ''%''' END
    + CASE WHEN @ID_Cliente IS NULL OR LTRIM(RTRIM(@ID_Cliente)) = '' THEN '' ELSE N' AND ID_Cliente LIKE ''%'' + @pIdCliente + ''%''' END
    + CASE WHEN @Tipo_Accion IS NULL OR LTRIM(RTRIM(@Tipo_Accion)) = '' THEN '' ELSE N' AND Tipo_Accion = @pTipoAccion' END
    + CASE WHEN @Estado IS NULL OR LTRIM(RTRIM(@Estado)) = '' THEN '' ELSE N' AND Estado = @pEstado' END
    + CASE WHEN @FechaDesde IS NULL THEN '' ELSE N' AND Fecha_Accion >= @pDesde' END
    + CASE WHEN @FechaHasta IS NULL THEN '' ELSE N' AND Fecha_Accion <= @pHasta' END
    + CASE WHEN @ID_Propiedad IS NULL OR LTRIM(RTRIM(@ID_Propiedad)) = '' THEN '' ELSE N' AND ID_Propiedad = @pPropiedad' END
    + N'
  ) x
  WHERE x.rn BETWEEN @pMin AND @pMax
  ORDER BY x.rn';

  EXEC sp_executesql @sql,
    N'@pAgente nvarchar(50), @pIdCliente nvarchar(50), @pTipoAccion nvarchar(50), @pEstado nvarchar(50),
      @pDesde date, @pHasta date, @pPropiedad nvarchar(50), @pMin int, @pMax int',
    @pAgente = @Agente, @pIdCliente = @ID_Cliente, @pTipoAccion = @Tipo_Accion, @pEstado = @Estado,
    @pDesde = @FechaDesde, @pHasta = @FechaHasta, @pPropiedad = @ID_Propiedad,
    @pMin = @Min, @pMax = @Max;
END
GO

GRANT EXECUTE ON PP_psnp_Seguimiento_Activo_Select TO PUBLIC;
GO
