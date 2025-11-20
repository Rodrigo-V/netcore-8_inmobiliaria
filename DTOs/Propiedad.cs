namespace Inmobiliaria.Net8.DTOs
{
    public class Propiedad
    {
        public string? ID_Propiedad { get; set; }
        public string? Codigo_Referencia { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Tipo_elemento { get; set; }
        public string? Direccion { get; set; }
        public string? Comuna { get; set; }
        public string? Ciudad { get; set; }
        public string? Region { get; set; }
        public string? Valor { get; set; }
        public string? Precio_UF { get; set; }
        public string? Dormitorios { get; set; }
        public string? Banos { get; set; }
        public string? M2_Construidos { get; set; }
        public string? M2_Terreno { get; set; }
        public string? Estado { get; set; }
        public string? Fecha_Publicacion { get; set; }
        public string? Agente_Responsable { get; set; }
        public string? Telefono_Contacto { get; set; }
        public string? Email_Contacto { get; set; }
        public string? Visitas_Totales { get; set; }
        public string? Url_Imagen { get; set; }
        
        // IDs en portales externos
        public string? id_TocToc { get; set; }
        public string? id_ChilePropiedades { get; set; }
        public string? id_PortalInmobiliario { get; set; }
        public string? id_Proppit { get; set; }
        public string? id_PortalRosch { get; set; }
        
        // Para paginación
        public int TotalRowCount { get; set; }
    }

    public class FiltroPropiedad
    {
        public string? IDPropiedad { get; set; }
        public string? Columna { get; set; }
        public string? Direccion { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
    }

    public class EstadisticasPropiedad
    {
        public int Total { get; set; }
        public int Disponible { get; set; }
        public int Vendida { get; set; }
        public int Reservada { get; set; }
        public int Arrendada { get; set; }
        public int Suspendida { get; set; }
        public int Otros { get; set; }
    }
}

