using System;
using System.Collections.Generic;

namespace Inmobiliaria.Net8.DTOs
{
    public class PropiedadPortalDTO
    {
        public string ID_Propiedad { get; set; } = string.Empty;
        public string Codigo_Referencia { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Comuna { get; set; } = string.Empty;
        public string Estado_Propiedad { get; set; } = string.Empty;
        public string? Url_Imagen { get; set; }
        public string? Imagen_Propiedad { get; set; }
        public int TotalRowCount { get; set; }

        // IDs de portales
        public string? id_TocToc { get; set; }
        public string? id_ChilePropiedades { get; set; }
        public string? id_PortalInmobiliario { get; set; }
        public string? id_Proppit { get; set; }
        public string? id_PortalRosch { get; set; }
    }

    public class PortalIDsDTO
    {
        public string ID_Propiedad { get; set; } = string.Empty;
        public string? id_TocToc { get; set; }
        public string? id_ChilePropiedades { get; set; }
        public string? id_PortalInmobiliario { get; set; }
        public string? id_Proppit { get; set; }
        public string? id_PortalRosch { get; set; }
    }

    public class FiltroPropiedadDTO
    {
        public string? IDPropiedad { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public string Columna { get; set; } = "Fecha_Publicacion";
        public string Direccion { get; set; } = "DESC";
        public int PageSize { get; set; } = 10;
    }

    public class PortalInmobiliarioDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string TipoPortal { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
