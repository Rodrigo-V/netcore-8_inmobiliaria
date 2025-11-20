namespace Inmobiliaria.Net8.DTOs
{
    public class PropiedadImagen
    {
        public string ID_Propiedad { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Comuna { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Tipo_elemento { get; set; } = string.Empty;
        public string? Url_Imagen { get; set; }
    }

    public class EstadisticasImagenes
    {
        public int TotalPropiedades { get; set; }
        public int ConImagen { get; set; }
        public int SinImagen { get; set; }
        public double PorcentajeCobertura { get; set; }
    }
}

