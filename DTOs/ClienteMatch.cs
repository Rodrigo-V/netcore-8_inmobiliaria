namespace Inmobiliaria.Net8.DTOs
{
    /// <summary>
    /// DTO para Cliente Match
    /// Representa la estructura de la tabla Clientes_Match
    /// </summary>
    public class ClienteMatch
    {
        // Campos principales
        public string ID_Interno { get; set; } = string.Empty;
        public string? Tipo_Match { get; set; }
        public string? Nombre { get; set; }
        public string? Rut { get; set; }
        public string? Datos_adjuntos { get; set; }
        public string? Direccion { get; set; }
        public string? Comuna { get; set; }
        public string? Estado_Civil { get; set; }
        public string? Profesion { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Giro_Razon_Social { get; set; }
        public int TotalRowCount { get; set; }
    }

    /// <summary>
    /// DTO para filtros de búsqueda de Clientes Match
    /// </summary>
    public class FiltroClientesMatch
    {
        public string? ID_Interno { get; set; }
        public string? Nombre { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Tipo_Match { get; set; }
        public string? Busqueda { get; set; }
        public int PaginaActual { get; set; } = 1;
        public int TamañoPagina { get; set; } = 25;
        public string ColumnaOrden { get; set; } = "ID_Interno";
        public string DireccionOrden { get; set; } = "ASC";
    }
}

