using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.Services;
using Inmobiliaria.Net8.DTOs;
using System;
using System.Threading.Tasks;

namespace Inmobiliaria.Net8.Controllers
{
    public class MercadoLibreController : Controller
    {
        private readonly MercadoLibreService _mlService;
        private readonly ILogger<MercadoLibreController> _logger;

        public MercadoLibreController(
            MercadoLibreService mlService,
            ILogger<MercadoLibreController> logger)
        {
            _mlService = mlService;
            _logger = logger;
        }

        /// <summary>
        /// Vista principal de configuración de Mercado Libre
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var estadoConexion = await _mlService.ObtenerEstadoConexionAsync();
                return View(estadoConexion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado de conexión de Mercado Libre");
                ViewBag.Error = "Error al obtener el estado de conexión: " + ex.Message;
                return View(new EstadoConexionML { Conectado = false, Mensaje = "Error al verificar conexión" });
            }
        }

        /// <summary>
        /// Redirige al usuario a Mercado Libre para autorizar la aplicación
        /// </summary>
        [HttpGet]
        public IActionResult Autorizar()
        {
            try
            {
                var authUrl = _mlService.GetAuthorizationUrl();
                _logger.LogInformation("Redirigiendo a URL de autorización de Mercado Libre");
                return Redirect(authUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar URL de autorización");
                TempData["Error"] = "Error al iniciar autorización: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Callback que recibe Mercado Libre después de autorizar
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Callback(string code, string error, string error_description)
        {
            // Si hubo error en la autorización
            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogWarning($"Error en autorización de Mercado Libre: {error} - {error_description}");
                TempData["Error"] = $"Error al autorizar: {error_description ?? error}";
                return RedirectToAction(nameof(Index));
            }

            // Si no se recibió el código
            if (string.IsNullOrEmpty(code))
            {
                _logger.LogWarning("No se recibió código de autorización de Mercado Libre");
                TempData["Error"] = "No se recibió el código de autorización. Intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Intercambiar código por token
                var token = await _mlService.GetAccessTokenAsync(code);
                
                _logger.LogInformation($"Token de Mercado Libre obtenido exitosamente para usuario {token.user_id}");
                TempData["Success"] = "¡Conexión con Mercado Libre establecida correctamente!";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener token de Mercado Libre");
                TempData["Error"] = "Error al completar autorización: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Vista para ver visitas de una publicación específica
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> VerVisitas(string itemId, string titulo = "Propiedad")
        {
            if (string.IsNullOrEmpty(itemId))
            {
                TempData["Error"] = "Debe proporcionar un ID de publicación";
                return RedirectToAction("Index", "Propiedades");
            }

            try
            {
                var estadisticas = await _mlService.GetEstadisticasVisitasAsync(itemId, titulo);
                return View(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener visitas del item {itemId}");
                TempData["Error"] = "Error al obtener estadísticas: " + ex.Message;
                return RedirectToAction("Index", "Propiedades");
            }
        }

        /// <summary>
        /// API para obtener visitas de un item (formato JSON)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerVisitasJson(string itemId)
        {
            try
            {
                if (string.IsNullOrEmpty(itemId))
                {
                    return BadRequest(new { error = "itemId es requerido" });
                }

                var visitasTotales = await _mlService.GetItemTotalVisitsAsync(itemId);
                
                var dateFrom = DateTime.Now.AddDays(-30);
                var dateTo = DateTime.Now;
                var visitas30Dias = await _mlService.GetItemVisitsAsync(itemId, dateFrom, dateTo);
                
                var dateFrom7 = DateTime.Now.AddDays(-7);
                var visitas7Dias = await _mlService.GetItemVisitsAsync(itemId, dateFrom7, dateTo);

                return Json(new
                {
                    success = true,
                    itemId = itemId,
                    visitasTotales = visitasTotales,
                    visitasUltimos30Dias = visitas30Dias?.total_visits ?? 0,
                    visitasUltimos7Dias = visitas7Dias?.total_visits ?? 0,
                    fechaConsulta = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ObtenerVisitasJson para item {itemId}");
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// API para obtener visitas detalladas por día
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerVisitasPorDia(string itemId, int dias = 30)
        {
            try
            {
                if (string.IsNullOrEmpty(itemId))
                {
                    return BadRequest(new { error = "itemId es requerido" });
                }

                if (dias < 1 || dias > 150)
                {
                    return BadRequest(new { error = "dias debe estar entre 1 y 150" });
                }

                var visitasTimeWindow = await _mlService.GetItemVisitsTimeWindowAsync(itemId, dias);

                return Json(new
                {
                    success = true,
                    data = visitasTimeWindow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en ObtenerVisitasPorDia para item {itemId}");
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Vista para mostrar el estado de la conexión
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EstadoConexion()
        {
            try
            {
                var estado = await _mlService.ObtenerEstadoConexionAsync();
                return PartialView("_EstadoConexion", estado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estado de conexión");
                return PartialView("_EstadoConexion", new EstadoConexionML 
                { 
                    Conectado = false, 
                    Mensaje = "Error: " + ex.Message 
                });
            }
        }

        /// <summary>
        /// Renovar token manualmente (útil para testing)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RenovarToken()
        {
            try
            {
                var accessToken = await _mlService.GetValidAccessTokenAsync();
                TempData["Success"] = "Token renovado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al renovar token");
                TempData["Error"] = "Error al renovar token: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Modal para ver visitas rápidas desde cualquier vista
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ModalVisitas(string itemId, string titulo)
        {
            try
            {
                if (string.IsNullOrEmpty(itemId))
                {
                    return BadRequest("itemId es requerido");
                }

                var estadisticas = await _mlService.GetEstadisticasVisitasAsync(itemId, titulo ?? "Propiedad");
                return PartialView("_ModalVisitas", estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al cargar modal de visitas para {itemId}");
                ViewBag.Error = ex.Message;
                return PartialView("_ModalVisitas", null);
            }
        }
    }
}

