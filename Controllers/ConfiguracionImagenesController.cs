using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.Services;

namespace Inmobiliaria.Net8.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ConfiguracionImagenesController : Controller
    {
        private readonly ConfiguracionImagenesService _configuracionImagenesService;
        private readonly ILogger<ConfiguracionImagenesController> _logger;

        public ConfiguracionImagenesController(
            ConfiguracionImagenesService configuracionImagenesService,
            ILogger<ConfiguracionImagenesController> logger)
        {
            _configuracionImagenesService = configuracionImagenesService;
            _logger = logger;
        }

        // GET: ConfiguracionImagenes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var propiedades = await _configuracionImagenesService.ObtenerPropiedadesConImagenesAsync();
                var estadisticas = await _configuracionImagenesService.ObtenerEstadisticasAsync();

                ViewBag.Propiedades = propiedades;
                ViewBag.Estadisticas = estadisticas;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar configuración de imágenes");
                TempData["Error"] = $"Error al cargar las propiedades: {ex.Message}";
                return View();
            }
        }

        // POST: ConfiguracionImagenes/ActualizarUrlImagen
        [HttpPost]
        public async Task<IActionResult> ActualizarUrlImagen(string idPropiedad, string urlImagen)
        {
            try
            {
                if (string.IsNullOrEmpty(idPropiedad))
                {
                    return Json(new { success = false, message = "ID de propiedad requerido" });
                }

                // Convertir URL a formato thumbnail si es necesario
                var urlConvertida = _configuracionImagenesService.ConvertirUrlGoogleDrive(urlImagen);

                // Guardar en base de datos
                var resultado = await _configuracionImagenesService.GuardarUrlImagenAsync(idPropiedad, urlConvertida);

                if (resultado)
                {
                    return Json(new
                    {
                        success = true,
                        message = "URL de imagen actualizada exitosamente",
                        urlImagen = urlConvertida
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Error al actualizar la URL de imagen" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar URL de imagen");
                return Json(new { success = false, message = $"Error interno: {ex.Message}" });
            }
        }

        // POST: ConfiguracionImagenes/EliminarImagen
        [HttpPost]
        public async Task<IActionResult> EliminarImagen(string idPropiedad)
        {
            try
            {
                if (string.IsNullOrEmpty(idPropiedad))
                {
                    return Json(new { success = false, message = "ID de propiedad requerido" });
                }

                var resultado = await _configuracionImagenesService.EliminarUrlImagenAsync(idPropiedad);

                if (resultado)
                {
                    return Json(new { success = true, message = "Imagen eliminada exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "Error al eliminar la imagen" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar imagen");
                return Json(new { success = false, message = $"Error interno: {ex.Message}" });
            }
        }

        // POST: ConfiguracionImagenes/BuscarPropiedades
        [HttpPost]
        public async Task<IActionResult> BuscarPropiedades(string termino)
        {
            try
            {
                var propiedades = await _configuracionImagenesService.ObtenerPropiedadesConImagenesAsync(termino);
                return Json(new { success = true, propiedades = propiedades });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar propiedades");
                return Json(new { success = false, message = $"Error al buscar propiedades: {ex.Message}" });
            }
        }

        // GET: ConfiguracionImagenes/ObtenerEstadisticas
        [HttpGet]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            try
            {
                var estadisticas = await _configuracionImagenesService.ObtenerEstadisticasAsync();
                return Json(new { success = true, estadisticas = estadisticas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return Json(new { success = false, message = $"Error al obtener estadísticas: {ex.Message}" });
            }
        }
    }
}

