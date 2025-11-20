using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.Services;

namespace Inmobiliaria.Net8.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SincronizacionPortales2Controller : Controller
    {
        private readonly SincronizacionPortalesService _sincronizacionService;
        private readonly ILogger<SincronizacionPortales2Controller> _logger;

        public SincronizacionPortales2Controller(
            SincronizacionPortalesService sincronizacionService,
            ILogger<SincronizacionPortales2Controller> logger)
        {
            _sincronizacionService = sincronizacionService;
            _logger = logger;
        }

        // GET: SincronizacionPortales2/Index
        [HttpGet]
        public async Task<IActionResult> Index(string? buscarPropiedad, string ordenarPor = "clicks")
        {
            try
            {
                _logger.LogInformation("Accediendo a Sincronización de Portales 2");

                var propiedades = await _sincronizacionService.ObtenerPropiedadesConClicsAsync(buscarPropiedad, ordenarPor);
                var estadisticas = await _sincronizacionService.ObtenerEstadisticasGeneralesAsync();

                ViewBag.Propiedades = propiedades;
                ViewBag.Estadisticas = estadisticas;
                ViewBag.BuscarPropiedad = buscarPropiedad;
                ViewBag.OrdenarPor = ordenarPor;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar sincronización de portales 2");
                ViewBag.Error = $"Error al cargar propiedades: {ex.Message}";
                return View();
            }
        }
    }
}

