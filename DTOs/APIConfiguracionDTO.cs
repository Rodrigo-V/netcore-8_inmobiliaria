using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Inmobiliaria.Net8.DTOs
{
    /// <summary>
    /// DTO para configuración de APIs
    /// </summary>
    public class APIConfiguracion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string TipoAPI { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string EndpointUrl { get; set; } = string.Empty;
        public string? ApiKey { get; set; }
        public string? ApiSecret { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaUltimaSincronizacion { get; set; }
        public string? ConfiguracionAdicional { get; set; }

        /// <summary>
        /// Obtiene la configuración adicional como diccionario
        /// </summary>
        public Dictionary<string, object> GetConfiguracionAdicional()
        {
            if (string.IsNullOrEmpty(ConfiguracionAdicional))
                return new Dictionary<string, object>();

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfiguracionAdicional) ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Establece la configuración adicional desde un diccionario
        /// </summary>
        public void SetConfiguracionAdicional(Dictionary<string, object> config)
        {
            ConfiguracionAdicional = JsonConvert.SerializeObject(config);
        }
    }

    /// <summary>
    /// Filtro para búsqueda de configuraciones de API
    /// </summary>
    public class FiltroAPIConfiguracion
    {
        public string? TipoAPI { get; set; }
        public bool? Activo { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
    }

    /// <summary>
    /// DTO para resultado de prueba de API
    /// </summary>
    public class APITestResult
    {
        public int Id { get; set; }
        public int APIConfiguracionId { get; set; }
        public DateTime FechaTest { get; set; }
        public bool Exitoso { get; set; }
        public int? StatusCode { get; set; }
        public string? Mensaje { get; set; }
        public string? Url { get; set; }
        public int? TiempoRespuesta { get; set; }
        public string? DatosRecibidos { get; set; }
        public string? ErrorDetalle { get; set; }
    }

    /// <summary>
    /// DTO para resultado de sincronización de API
    /// </summary>
    public class APISincronizacion
    {
        public int Id { get; set; }
        public int APIConfiguracionId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool Exitoso { get; set; }
        public string? Mensaje { get; set; }
        public int RegistrosProcesados { get; set; }
        public decimal? DuracionSegundos { get; set; }
        public string? ErrorDetalle { get; set; }
    }

    /// <summary>
    /// DTO para estadísticas de API
    /// </summary>
    public class APIEstadisticas
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string TipoAPI { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime? FechaUltimaSincronizacion { get; set; }
        public DateTime? UltimaPrueba { get; set; }
        public bool? UltimaPruebaExitoso { get; set; }
        public DateTime? UltimaSincronizacion { get; set; }
        public bool? UltimaSincronizacionExitoso { get; set; }
        public int TotalPruebas { get; set; }
        public int PruebasExitosas { get; set; }
        public int TotalSincronizaciones { get; set; }
        public int SincronizacionesExitosas { get; set; }
        public int? TotalRegistrosProcesados { get; set; }
    }
}
