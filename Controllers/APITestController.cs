using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.Services;
using Inmobiliaria.Net8.DTOs;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inmobiliaria.Net8.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class APITestController : Controller
    {
        private readonly APIConfiguracionService _apiService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<APITestController> _logger;

        public APITestController(APIConfiguracionService apiService, ILogger<APITestController> logger)
        {
            _apiService = apiService;
            _logger = logger;
            
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            handler.CheckCertificateRevocationList = false;
            handler.UseProxy = false;
            handler.AllowAutoRedirect = true;
            
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromMinutes(5);
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var configuraciones = await _apiService.ObtenerConfiguracionesAsync();
                return View(configuraciones);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar configuraciones: {ex.Message}";
                return View(new List<APIConfiguracion>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProbarConexion(int apiId)
        {
            try
            {
                var configuracion = await _apiService.ObtenerPorIdAsync(apiId);
                if (configuracion == null)
                {
                    return Json(new { Exitoso = false, Mensaje = "Configuración de API no encontrada" });
                }

                var stopwatch = Stopwatch.StartNew();
                var testResult = new APITestResult
                {
                    APIConfiguracionId = apiId,
                    FechaTest = DateTime.Now,
                    Exitoso = false
                };

                try
                {
                    var apiUrl = ConstruirUrl(configuracion);
                    testResult.Url = apiUrl;

                    var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                    ConfigurarHeadersRequest(request, configuracion);
                    
                    var response = await _httpClient.SendAsync(request);
                    var content = await response.Content.ReadAsStringAsync();

                    stopwatch.Stop();
                    testResult.TiempoRespuesta = (int)stopwatch.ElapsedMilliseconds;
                    testResult.StatusCode = (int)response.StatusCode;

                    if (response.IsSuccessStatusCode)
                    {
                        testResult.Exitoso = true;
                        testResult.Mensaje = $"Conexión exitosa con {configuracion.Nombre}";
                        testResult.DatosRecibidos = content.Length > 1000 ? content.Substring(0, 1000) + "..." : content;
                    }
                    else
                    {
                        testResult.Mensaje = $"Error en la API: {response.StatusCode} - {response.ReasonPhrase}";
                        testResult.ErrorDetalle = content;
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    testResult.TiempoRespuesta = (int)stopwatch.ElapsedMilliseconds;
                    testResult.Mensaje = $"Error de conexión: {ex.Message}";
                    testResult.ErrorDetalle = ex.ToString();
                }

                try
                {
                    await _apiService.GuardarTestResultAsync(testResult);
                }
                catch (Exception saveEx)
                {
                    _logger.LogWarning(saveEx, "No se pudo guardar el resultado de la prueba en BD");
                }

                return Json(new {
                    Exitoso = testResult.Exitoso, 
                    Mensaje = testResult.Mensaje,
                    StatusCode = testResult.StatusCode,
                    Url = testResult.Url,
                    TiempoRespuesta = testResult.TiempoRespuesta,
                    DatosRecibidos = testResult.DatosRecibidos,
                    ErrorDetalle = testResult.ErrorDetalle
                });
            }
            catch (Exception ex)
            {
                return Json(new { Exitoso = false, Mensaje = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SincronizarAPI(int apiId)
        {
            try
            {
                var configuracion = await _apiService.ObtenerPorIdAsync(apiId);
                if (configuracion == null)
                {
                    return Json(new { Exitoso = false, Mensaje = "Configuración de API no encontrada" });
                }

                var sincronizacion = new APISincronizacion
                {
                    APIConfiguracionId = apiId,
                    FechaInicio = DateTime.Now,
                    Exitoso = false,
                    RegistrosProcesados = 0
                };

                try
                {
                    await SincronizarAPIGenerica(configuracion, sincronizacion);
                }
                catch (Exception ex)
                {
                    sincronizacion.Mensaje = $"Error en sincronización: {ex.Message}";
                    sincronizacion.ErrorDetalle = ex.ToString();
                }

                sincronizacion.FechaFin = DateTime.Now;
                sincronizacion.DuracionSegundos = (decimal)(sincronizacion.FechaFin.Value - sincronizacion.FechaInicio).TotalSeconds;

                await _apiService.GuardarSincronizacionAsync(sincronizacion);

                return Json(new { 
                    Exitoso = sincronizacion.Exitoso, 
                    Mensaje = sincronizacion.Mensaje,
                    RegistrosProcesados = sincronizacion.RegistrosProcesados,
                    DuracionSegundos = sincronizacion.DuracionSegundos
                });
            }
            catch (Exception ex)
            {
                return Json(new { Exitoso = false, Mensaje = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracion(APIConfiguracion configuracion)
        {
            try
            {
                if (string.IsNullOrEmpty(configuracion.Nombre) || 
                    string.IsNullOrEmpty(configuracion.TipoAPI) || 
                    string.IsNullOrEmpty(configuracion.Url) || 
                    string.IsNullOrEmpty(configuracion.EndpointUrl))
                {
                    return Json(new { success = false, message = "Los campos Nombre, Tipo API, URL y Endpoint URL son obligatorios" });
                }

                var id = await _apiService.GuardarAsync(configuracion);
                
                if (configuracion.Id == 0)
                {
                    return Json(new { success = true, message = $"Configuración de API creada exitosamente con ID: {id}" });
                }
                else
                {
                    return Json(new { success = true, message = "Configuración de API actualizada exitosamente" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error al guardar configuración: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfiguracion(int id)
        {
            try
            {
                var resultado = await _apiService.EliminarAsync(id);
                if (resultado)
                {
                    TempData["Success"] = "Configuración de API eliminada exitosamente";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar la configuración";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al eliminar configuración: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerEstadisticas()
        {
            try
            {
                var estadisticas = await _apiService.ObtenerEstadisticasAsync();
                return Json(estadisticas);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerFechasAPI(int apiId)
        {
            try
            {
                var estadisticas = await _apiService.ObtenerEstadisticasAsync();
                var apiStats = estadisticas.FirstOrDefault(s => s.Id == apiId);
                
                if (apiStats != null)
                {
                    return Json(new { 
                        UltimaPrueba = apiStats.UltimaPrueba?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca",
                        UltimaPruebaExitoso = apiStats.UltimaPruebaExitoso,
                        UltimaSincronizacion = apiStats.UltimaSincronizacion?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca",
                        UltimaSincronizacionExitoso = apiStats.UltimaSincronizacionExitoso
                    });
                }
                
                return Json(new { 
                    UltimaPrueba = "Nunca",
                    UltimaPruebaExitoso = false,
                    UltimaSincronizacion = "Nunca",
                    UltimaSincronizacionExitoso = false
                });
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerPorId(int id)
        {
            try
            {
                var configuracion = await _apiService.ObtenerPorIdAsync(id);
                return Json(configuracion);
            }
            catch (Exception ex)
            {
                return Json(new { Error = ex.Message });
            }
        }

        private string ConstruirUrl(APIConfiguracion configuracion)
        {
            return (configuracion.EndpointUrl ?? string.Empty).Trim();
        }

        private void ConfigurarHeadersRequest(HttpRequestMessage request, APIConfiguracion configuracion)
        {
            request.Headers.Clear();

            switch (configuracion.TipoAPI)
            {
                case "TocToc":
                    // TocToc no necesita Content-Type para GET requests
                    if (!string.IsNullOrEmpty(configuracion.ApiSecret))
                        request.Headers.TryAddWithoutValidation("KeyIntegrador", configuracion.ApiSecret);
                    if (!string.IsNullOrEmpty(configuracion.ApiKey))
                        request.Headers.TryAddWithoutValidation("KeyCliente", configuracion.ApiKey);
                    break;
                case "PortalInmobiliario":
                    if (!string.IsNullOrEmpty(configuracion.ApiSecret))
                        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {configuracion.ApiSecret}");
                    if (!string.IsNullOrEmpty(configuracion.ApiKey))
                        request.Headers.TryAddWithoutValidation("X-API-Key", configuracion.ApiKey);
                    break;
                case "ChilePropiedades":
                    if (string.IsNullOrEmpty(configuracion.ApiSecret))
                    {
                        throw new ArgumentException("El token de Chile Propiedades no puede ser null o vacío");
                    }
                    request.Headers.TryAddWithoutValidation("Authorization", configuracion.ApiSecret);
                    break;
            }
        }

        private async Task SincronizarAPIGenerica(APIConfiguracion configuracion, APISincronizacion sincronizacion)
        {
            try
            {
                _logger.LogInformation("[API SYNC] Iniciando sincronización para {TipoAPI}", configuracion.TipoAPI);
                
                // Obtener datos de la API
                var url = configuracion.EndpointUrl?.Trim() ?? configuracion.Url?.Trim();
                if (string.IsNullOrEmpty(url))
                {
                    sincronizacion.Exitoso = false;
                    sincronizacion.Mensaje = "URL no configurada";
                    return;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                ConfigurarHeadersRequest(request, configuracion);
                
                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("[API SYNC] Status: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var registrosProcesados = await ProcesarDatosAPI(configuracion, content);
                    
                    sincronizacion.Exitoso = true;
                    sincronizacion.Mensaje = $"Sincronización exitosa. Datos obtenidos de API y guardados en BD.";
                    sincronizacion.RegistrosProcesados = registrosProcesados;
                    
                    _logger.LogInformation("[API SYNC] Completado - Registros procesados: {RegistrosProcesados}", registrosProcesados);
                }
                else
                {
                    sincronizacion.Exitoso = false;
                    sincronizacion.Mensaje = $"Error HTTP {response.StatusCode}: {content}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[API SYNC] ERROR: {Message}", ex.Message);
                sincronizacion.Exitoso = false;
                sincronizacion.Mensaje = $"Error en sincronización: {ex.Message}";
                sincronizacion.ErrorDetalle = ex.ToString();
            }
        }

        private int ObtenerPortalIdPorTipoAPI(string tipoAPI)
        {
            // Mapeo de tipos de API a PortalIds específicos
            switch (tipoAPI)
            {
                case "TocToc":
                    return 5; // Portal TocToc
                case "ChilePropiedades":
                    return 4; // Portal ChilePropiedades
                case "PortalInmobiliario":
                    return 2; // Portal Inmobiliario
                default:
                    return 1; // Portal genérico por defecto
            }
        }

        private async Task<int> ProcesarDatosAPI(APIConfiguracion configuracion, string jsonContent)
        {
            try
            {
                _logger.LogInformation("[API SYNC] Procesando datos JSON para {TipoAPI}", configuracion.TipoAPI);
                
                var registrosProcesados = 0;
                var fechaSincronizacion = DateTime.Now;
                var portalId = ObtenerPortalIdPorTipoAPI(configuracion.TipoAPI);
                
                // Procesar JSON según el tipo de API
                switch (configuracion.TipoAPI)
                {
                    case "TocToc":
                        registrosProcesados = await ProcesarDatosTocToc(portalId, jsonContent, fechaSincronizacion);
                        break;
                    case "ChilePropiedades":
                        registrosProcesados = await ProcesarDatosChilePropiedades(portalId, jsonContent, fechaSincronizacion);
                        break;
                    default:
                        // Para otros tipos, guardar JSON tal como viene
                        registrosProcesados = await GuardarDatosGenericos(portalId, jsonContent, fechaSincronizacion);
                        break;
                }

                return registrosProcesados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[API SYNC] Error procesando datos: {Message}", ex.Message);
                return 0;
            }
        }

        private async Task<int> ProcesarDatosTocToc(int portalId, string jsonContent, DateTime fechaSincronizacion)
        {
            try
            {
                var visitasData = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                var registrosProcesados = 0;

                // Verificar estructura del JSON de TocToc
                var visitas = new List<dynamic>();
                
                if (visitasData?.success == true && visitasData?.data?.propertyVisits != null)
                {
                    visitas = visitasData.data.propertyVisits.ToObject<List<dynamic>>();
                }
                else if (visitasData is JArray)
                {
                    visitas = visitasData.ToObject<List<dynamic>>();
                }
                else if (visitasData?.data != null)
                {
                    if (visitasData.data is JArray)
                    {
                        visitas = visitasData.data.ToObject<List<dynamic>>();
                    }
                    else
                    {
                        visitas = new List<dynamic> { visitasData.data };
                    }
                }

                foreach (var visitaData in visitas)
                {
                    try
                    {
                        string propiedadId = visitaData?.integratorPropertyID?.ToString() ?? visitaData?.propiedad_id?.ToString() ?? "";
                        string propiedadCodigo = visitaData?.integratorPropertyID?.ToString() ?? "";
                        string titulo = visitaData?.title?.ToString() ?? "";
                        int visitasTocToc = 0;
                        int.TryParse(visitaData?.totalVisits?.ToString(), out visitasTocToc);

                        var datosVisita = new
                        {
                            Visitas = visitaData?.totalVisits ?? 0,
                            FechaExtraccion = fechaSincronizacion.ToString("yyyy-MM-ddTHH:mm:ss.fffffffK"),
                            Fuente = "TocTocAPI",
                            Referencia = propiedadId,
                            DatosOriginales = visitaData
                        };

                        await _apiService.InsertarClicPortalAsync(
                            portalId, 
                            propiedadId, 
                            propiedadCodigo, 
                            titulo, 
                            fechaSincronizacion, 
                            "TOCTOC_API", 
                            "TocTocSync/1.0", 
                            "https://api.toctoc.com", 
                            "https://api.toctoc.com/statistics", 
                            "Visita", 
                            visitasTocToc, 
                            "Chile", 
                            true, 
                            fechaSincronizacion, 
                            JsonConvert.SerializeObject(datosVisita)
                        );

                        registrosProcesados++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando visita TocToc");
                    }
                }

                return registrosProcesados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general TocToc: {Message}", ex.Message);
                return 0;
            }
        }

        private async Task<int> ProcesarDatosChilePropiedades(int portalId, string jsonContent, DateTime fechaSincronizacion)
        {
            try
            {
                var estadisticasData = JsonConvert.DeserializeObject<dynamic>(jsonContent);
                var registrosProcesados = 0;

                var estadisticasArray = estadisticasData as JArray;
                if (estadisticasArray != null)
                {
                    foreach (var estadisticaToken in estadisticasArray)
                    {
                        try
                        {
                            dynamic estadistica = estadisticaToken;
                            string propiedadId = estadistica?.id?.ToString() ?? "";
                            string propiedadCodigo = estadistica?.externalID?.ToString() ?? estadistica?.id?.ToString() ?? "";
                            int visitasChile = 0;
                            int.TryParse(estadistica?.pageViews?.ToString(), out visitasChile);

                            await _apiService.InsertarClicPortalAsync(
                                portalId,
                                propiedadId,
                                propiedadCodigo,
                                $"Propiedad ChilePropiedades ID: {estadistica?.id}",
                                fechaSincronizacion,
                                "CHILEPROP_API",
                                "ChilePropiedadesSync/1.0",
                                "https://www.chilepropiedades.cl",
                                $"https://www.chilepropiedades.cl/propiedad/{estadistica?.id}",
                                "ESTADISTICAS",
                                visitasChile,
                                "Chile",
                                true,
                                fechaSincronizacion,
                                JsonConvert.SerializeObject(estadisticaToken)
                            );

                            registrosProcesados++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error procesando estadística ChilePropiedades");
                        }
                    }
                }
                else
                {
                    // Si no es array, guardar como un solo registro
                    await _apiService.InsertarClicPortalAsync(
                        portalId,
                        "ESTADISTICAS_GENERALES",
                        "",
                        "Estadísticas Chile Propiedades",
                        fechaSincronizacion,
                        "",
                        "",
                        "",
                        "",
                        "ESTADISTICAS",
                        0,
                        "",
                        true,
                        fechaSincronizacion,
                        jsonContent
                    );
                    registrosProcesados = 1;
                }

                return registrosProcesados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error general ChilePropiedades: {Message}", ex.Message);
                return 0;
            }
        }

        private async Task<int> GuardarDatosGenericos(int portalId, string jsonContent, DateTime fechaSincronizacion)
        {
            try
            {
                await _apiService.InsertarClicPortalAsync(
                    portalId,
                    "GENERICO",
                    "",
                    "Datos API Genérica",
                    fechaSincronizacion,
                    "",
                    "",
                    "",
                    "",
                    "API_DATA",
                    0,
                    "",
                    true,
                    fechaSincronizacion,
                    jsonContent
                );
                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando datos genéricos: {Message}", ex.Message);
                return 0;
            }
        }
    }
}
