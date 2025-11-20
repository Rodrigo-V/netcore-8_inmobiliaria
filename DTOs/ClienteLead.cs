namespace Inmobiliaria.Net8.DTOs
{
    public class ClienteLead
    {
        // Campos principales
        public DateTime? Creado { get; set; }
        public DateTime? Fecha_Contacto { get; set; }
        public string Asistente { get; set; } = string.Empty;
        public string ID_Cliente { get; set; } = string.Empty;
        public string Seguimiento { get; set; } = string.Empty;
        public string Portal { get; set; } = string.Empty;
        
        // Campos opcionales
        public string? Respuesta { get; set; }
        public string? ID_Unidad_Consultada { get; set; }
        public string? Unidad_Consultada { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string? Sexo { get; set; }
        public string? Telefono { get; set; }
        public string? Correo_Electronico { get; set; }
        public bool? Visita_Realizada { get; set; }
        public string? Imagen_Propiedad { get; set; }
        
        // Campos auxiliares
        public int TotalRowCount { get; set; }
        
        // Propiedades calculadas
        public string Creado_Str => Creado?.ToString("dd/MM/yyyy HH:mm") ?? "";
        public string Fecha_Contacto_Str => Fecha_Contacto?.ToString("dd/MM/yyyy") ?? "";
        public string Telefono_Str => Telefono ?? "";
        public string Nombre_Completo => $"{Nombres} {Apellidos}".Trim();
        public string Visita_Realizada_Str => Visita_Realizada == true ? "Sí" : "No";
        public bool EsNuevo => string.IsNullOrEmpty(ID_Cliente);
        public bool TieneImagen => !string.IsNullOrEmpty(Imagen_Propiedad);
    }
}

