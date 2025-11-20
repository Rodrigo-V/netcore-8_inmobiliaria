namespace Inmobiliaria.Net8.DTOs
{
    /// <summary>
    /// DTO para datos de la matriz de sincronización
    /// </summary>
    public class MatrizSincronizacion
    {
        public string ID_Propiedad { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Comuna { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string TipoPropiedad { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string? Url_Imagen { get; set; }
        public Dictionary<string, int> ClicsPorPortal { get; set; } = new Dictionary<string, int>();
        public int TotalClics { get; set; }
    }

    /// <summary>
    /// DTO para resumen de portales
    /// </summary>
    public class ResumenPortal
    {
        public string NombrePortal { get; set; } = string.Empty;
        public int TotalClics { get; set; }
        public int PropiedadesActivas { get; set; }
        public double PromedioClics { get; set; }
    }

    /// <summary>
    /// DTO para propiedad con clics
    /// </summary>
    public class PropiedadConClics
    {
        public string ID_Propiedad { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Comuna { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string TipoPropiedad { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string? ImagenUrl { get; set; }
        public int TotalClics { get; set; }
        public DateTime? UltimoClick { get; set; }
        public List<ClicPorPortal> ClicsPorPortal { get; set; } = new List<ClicPorPortal>();
    }

    /// <summary>
    /// DTO para clics por portal
    /// </summary>
    public class ClicPorPortal
    {
        public string NombrePortal { get; set; } = string.Empty;
        public int CantidadClics { get; set; }
        public DateTime? UltimoClick { get; set; }
    }

    /// <summary>
    /// DTO para estadísticas generales
    /// </summary>
    public class EstadisticasGenerales
    {
        public int TotalPropiedades { get; set; }
        public int TotalClics { get; set; }
        public int PropiedadesConClics { get; set; }
        public double PromedioClicsPorPropiedad { get; set; }
    }

    /// <summary>
    /// DTO para Portal Inmobiliario
    /// </summary>
    public class PortalInmobiliario
    {
        public int ID_Portal { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? URL { get; set; }
        public bool Activo { get; set; }
        public DateTime? Fecha_Creacion { get; set; }
    }

    /// <summary>
    /// DTO para datos de Excel de matriz
    /// </summary>
    public class DatosExcelMatriz
    {
        public DateTime FechaSincronizacion { get; set; }
        public string ID_Propiedad { get; set; } = string.Empty;
        public string TituloPropiedad { get; set; } = string.Empty;
        public string Comuna { get; set; } = string.Empty;
        public int TotalClics { get; set; }
        public int ClicsPortalInmobiliario { get; set; }
        public int ClicsProppit { get; set; }
        public int ClicsChilePropiedades { get; set; }
        public int ClicsTocToc { get; set; }
    }
}

