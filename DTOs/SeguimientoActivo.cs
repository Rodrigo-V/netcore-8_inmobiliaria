namespace Inmobiliaria.Net8.DTOs
{
    /// <summary>
    /// DTO para Seguimiento Activo
    /// Representa acciones de seguimiento realizadas sobre clientes leads
    /// </summary>
    public class SeguimientoActivo
    {
        public string ID_Cliente { get; set; } = string.Empty;
        public string? ID_Propiedad { get; set; }
        public string Agente { get; set; } = string.Empty;
        public string? Codigo_Agente { get; set; }
        public DateTime Fecha_Accion { get; set; }
        public string Tipo_Accion { get; set; } = string.Empty;
        public string Descripcion_Accion { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime? Fecha_Proximo_Contacto { get; set; }
        
        // Para DataTables
        public int? TotalRowCount { get; set; }
    }

    /// <summary>
    /// DTO para filtros de búsqueda de Seguimiento Activo
    /// </summary>
    public class FiltroSeguimientoActivo
    {
        public string? Agente { get; set; }
        public string? ID_Cliente { get; set; }
        public string? Tipo_Accion { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? ID_Propiedad { get; set; }
        public string? Estado { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string ColumnaOrden { get; set; } = "Fecha_Accion";
        public string DireccionOrden { get; set; } = "DESC";
    }
}

