using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.DTOs;
using Inmobiliaria.Net8.Services;

namespace Inmobiliaria.Net8.Controllers
{
    [Authorize]
    public class SeguimientoActivoController : Controller
    {
        private readonly SeguimientoActivoService _seguimientoService;
        private readonly ILogger<SeguimientoActivoController> _logger;

        public SeguimientoActivoController(
            SeguimientoActivoService seguimientoService,
            ILogger<SeguimientoActivoController> logger)
        {
            _seguimientoService = seguimientoService;
            _logger = logger;
        }

        // GET: SeguimientoActivo
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // GET: SeguimientoActivo/Listar
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] FiltroSeguimientoActivo filtro)
        {
            try
            {
                var (lista, total) = await _seguimientoService.ObtenerTodosAsync(filtro);
                return Json(new { success = true, data = lista, total = total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar seguimientos");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: SeguimientoActivo/ObtenerPorCliente/{idCliente}
        [HttpGet]
        public async Task<IActionResult> ObtenerPorCliente(string idCliente)
        {
            try
            {
                var lista = await _seguimientoService.ObtenerPorClienteAsync(idCliente);
                return Json(new { success = true, data = lista });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener seguimientos del cliente");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: SeguimientoActivo/Agregar
        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] SeguimientoActivo seguimiento)
        {
            try
            {
                var resultado = await _seguimientoService.AgregarAsync(seguimiento);
                return Json(new { success = resultado, message = "Seguimiento agregado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar seguimiento");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

