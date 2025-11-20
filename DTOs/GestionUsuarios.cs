namespace Inmobiliaria.Net8.DTOs
{
    /// <summary>
    /// DTO para entidad Usuario
    /// </summary>
    public class Usuario
    {
        public int ID_Usuario { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo_Electronico { get; set; } = string.Empty;
        public string? Clave { get; set; }
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime Fecha_Creacion { get; set; }
        public DateTime? Ultimo_Acceso { get; set; }
        
        // Propiedades de solo lectura para la presentación
        public string NombreCompleto => $"{Nombres} {Apellidos}";
        public string InicialAvatar => !string.IsNullOrEmpty(Nombres) && !string.IsNullOrEmpty(Apellidos) 
            ? $"{Nombres[0]}{Apellidos[0]}" 
            : "US";
    }

    /// <summary>
    /// DTO para estadísticas de usuarios
    /// </summary>
    public class EstadisticasUsuarios
    {
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int UsuariosInactivos { get; set; }
        public int TotalRoles { get; set; }
        public int UsuariosActivosUltimaSemana { get; set; }
        public int UsuariosActivosUltimoMes { get; set; }
        public List<EstadisticaRol> EstadisticasPorRol { get; set; } = new List<EstadisticaRol>();
    }

    /// <summary>
    /// DTO para estadísticas por rol
    /// </summary>
    public class EstadisticaRol
    {
        public string Rol { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public int Activos { get; set; }
        public int Inactivos { get; set; }
    }
}

