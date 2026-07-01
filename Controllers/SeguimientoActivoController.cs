using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.DTOs;
using Inmobiliaria.Net8.Services;
using Inmobiliaria.Net8.Helpers;

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

        // POST: SeguimientoActivo/GetData - DataTables server-side
        [HttpPost]
        public async Task<IActionResult> GetData()
        {
            var request = DataTablesHelper.Parse(Request.Form);
            try
            {
                string columna = request.OrderColumnIndex switch
                {
                    "0" => "ID_Cliente",
                    "1" => "Agente",
                    "2" => "Fecha_Accion",
                    "3" => "Tipo_Accion",
                    _ => "Fecha_Accion"
                };

                var filtro = new FiltroSeguimientoActivo
                {
                    ID_Cliente = DataTablesHelper.GetFormValue(Request.Form, "filtroCliente"),
                    Agente = DataTablesHelper.GetFormValue(Request.Form, "filtroAgente"),
                    Tipo_Accion = DataTablesHelper.GetFormValue(Request.Form, "filtroTipoAccion"),
                    Estado = DataTablesHelper.GetFormValue(Request.Form, "filtroEstado"),
                    PageNumber = request.PageNumber,
                    PageSize = request.Length,
                    ColumnaOrden = columna,
                    DireccionOrden = request.OrderDirection.ToUpperInvariant()
                };

                var (lista, total) = await _seguimientoService.ObtenerTodosAsync(filtro);

                return Json(DataTablesHelper.Success(request.Draw, total, MapearSeguimientoParaDataTable(lista)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetData SeguimientoActivo");
                return Json(DataTablesHelper.Error(request.Draw, ex.Message));
            }
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

        // GET: SeguimientoActivo/Clientes
        [HttpGet]
        public async Task<IActionResult> Clientes()
        {
            try
            {
                var lista = await _seguimientoService.ObtenerClientesDistintosAsync();
                return Json(new { success = true, data = lista });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes para filtros");
                return Json(new { success = false, data = new List<string>() });
            }
        }

        // GET: SeguimientoActivo/Agentes
        [HttpGet]
        public async Task<IActionResult> Agentes()
        {
            try
            {
                var lista = await _seguimientoService.ObtenerAgentesDistintosAsync();
                return Json(new { success = true, data = lista });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agentes para filtros");
                return Json(new { success = false, data = new List<string>() });
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

        private static IEnumerable<object> MapearSeguimientoParaDataTable(IEnumerable<SeguimientoActivo> lista)
        {
            return lista.Select(s => new
            {
                iD_Cliente = s.ID_Cliente,
                agente = s.Agente,
                fecha_Accion = s.Fecha_Accion,
                tipo_Accion = s.Tipo_Accion,
                descripcion_Accion = s.Descripcion_Accion,
                resultado = s.Resultado,
                estado = s.Estado,
                fecha_Proximo_Contacto = s.Fecha_Proximo_Contacto
            });
        }
    }
}

