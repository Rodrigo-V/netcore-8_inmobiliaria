using System;
using System.Collections.Generic;

namespace Inmobiliaria.Net8.DTOs
{
    /// <summary>
    /// DTO para el token de acceso de Mercado Libre
    /// </summary>
    public class MercadoLibreToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public long user_id { get; set; }
        public string refresh_token { get; set; }
    }

    /// <summary>
    /// DTO para almacenar tokens en base de datos
    /// </summary>
    public class MercadoLibreTokenDB
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime FechaExpiracion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public bool Activo { get; set; }
    }

    /// <summary>
    /// DTO para visitas de un item
    /// </summary>
    public class ItemVisits
    {
        public string item_id { get; set; }
        public string date_from { get; set; }
        public string date_to { get; set; }
        public int total_visits { get; set; }
        public List<VisitDetail> visits_detail { get; set; }
    }

    /// <summary>
    /// DTO para visitas de usuario
    /// </summary>
    public class UserVisits
    {
        public long user_id { get; set; }
        public string date_from { get; set; }
        public string date_to { get; set; }
        public int total_visits { get; set; }
        public List<VisitDetail> visits_detail { get; set; }
    }

    /// <summary>
    /// Detalle de visitas por compañía
    /// </summary>
    public class VisitDetail
    {
        public string company { get; set; }
        public int quantity { get; set; }
    }

    /// <summary>
    /// DTO para visitas detalladas por ventana de tiempo
    /// </summary>
    public class ItemVisitsTimeWindow
    {
        public string item_id { get; set; }
        public string date_from { get; set; }
        public string date_to { get; set; }
        public int total_visits { get; set; }
        public int last { get; set; }
        public string unit { get; set; }
        public List<VisitResult> results { get; set; }
    }

    /// <summary>
    /// Resultado de visitas por fecha
    /// </summary>
    public class VisitResult
    {
        public string date { get; set; }
        public int total { get; set; }
        public List<VisitDetail> visits_detail { get; set; }
    }

    /// <summary>
    /// DTO para mostrar estadísticas de visitas en la vista
    /// </summary>
    public class EstadisticasVisitasML
    {
        public string ItemId { get; set; }
        public string TituloPropiedad { get; set; }
        public int VisitasTotales { get; set; }
        public int VisitasUltimos7Dias { get; set; }
        public int VisitasUltimos30Dias { get; set; }
        public List<VisitasPorDia> VisitasPorDia { get; set; }
        public DateTime FechaConsulta { get; set; }
    }

    /// <summary>
    /// DTO para visitas agrupadas por día
    /// </summary>
    public class VisitasPorDia
    {
        public DateTime Fecha { get; set; }
        public int Visitas { get; set; }
    }

    /// <summary>
    /// DTO para respuesta de error de la API
    /// </summary>
    public class MercadoLibreError
    {
        public string error { get; set; }
        public string error_description { get; set; }
        public int status { get; set; }
        public List<object> cause { get; set; }
    }

    /// <summary>
    /// DTO para el estado de conexión con Mercado Libre
    /// </summary>
    public class EstadoConexionML
    {
        public bool Conectado { get; set; }
        public long? UserId { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public int? DiasRestantes { get; set; }
        public string Mensaje { get; set; }
    }
}

