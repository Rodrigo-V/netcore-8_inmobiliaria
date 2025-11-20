namespace Inmobiliaria.Net8.DTOs
{
    public class FiltroClientesLeads
    {
        // Filtros de búsqueda
        public string? ID_Cliente { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string? Correo_Electronico { get; set; }
        public string? Portal { get; set; }
        public string? Asistente { get; set; }
        public string? Seguimiento { get; set; }
        public string? ID_Unidad_Consultada { get; set; }
        public DateTime? Fecha_Contacto_Desde { get; set; }
        public DateTime? Fecha_Contacto_Hasta { get; set; }
        public DateTime? Creado_Desde { get; set; }
        public DateTime? Creado_Hasta { get; set; }
        
        // Paginación
        public int PaginaActual { get; set; } = 1;
        public int TamañoPagina { get; set; } = 10;
        public string ColumnaOrden { get; set; } = "Creado";
        public string DireccionOrden { get; set; } = "DESC";
    }
}

