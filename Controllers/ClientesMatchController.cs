using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.DTOs;
using Inmobiliaria.Net8.Services;
using Inmobiliaria.Net8.Helpers;

namespace Inmobiliaria.Net8.Controllers
{
    [Authorize] // Requiere autenticación
    public class ClientesMatchController : Controller
    {
        private readonly ClientesMatchService _clientesMatchService;
        private readonly ILogger<ClientesMatchController> _logger;

        public ClientesMatchController(
            ClientesMatchService clientesMatchService,
            ILogger<ClientesMatchController> logger)
        {
            _clientesMatchService = clientesMatchService;
            _logger = logger;
        }

        // GET: ClientesMatch
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: ClientesMatch/GetData - DataTables server-side
        [HttpPost]
        public async Task<IActionResult> GetData()
        {
            var request = DataTablesHelper.Parse(Request.Form);
            try
            {
                string columna = request.OrderColumnIndex switch
                {
                    "0" => "ID_Interno",
                    "1" => "Tipo_Match",
                    "2" => "Nombre",
                    "3" => "Rut",
                    "4" => "Telefono",
                    "5" => "Correo",
                    "6" => "Comuna",
                    "7" => "Profesion",
                    _ => "ID_Interno"
                };

                var filtro = new FiltroClientesMatch
                {
                    ID_Interno = DataTablesHelper.GetFormValue(Request.Form, "filtroID") ?? string.Empty,
                    Nombre = DataTablesHelper.GetFormValue(Request.Form, "filtroNombre") ?? string.Empty,
                    Telefono = DataTablesHelper.GetFormValue(Request.Form, "filtroTelefono") ?? string.Empty,
                    Correo = DataTablesHelper.GetFormValue(Request.Form, "filtroCorreo") ?? string.Empty,
                    Busqueda = request.SearchValue ?? string.Empty,
                    ColumnaOrden = columna,
                    DireccionOrden = request.OrderDirection.ToUpperInvariant(),
                    PaginaActual = request.PageNumber,
                    TamañoPagina = request.Length
                };

                var (lista, total) = await _clientesMatchService.ObtenerPaginadosAsync(filtro);

                return Json(DataTablesHelper.Success(request.Draw, total, MapearMatchParaDataTable(lista)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetData ClientesMatch");
                return Json(DataTablesHelper.Error(request.Draw, ex.Message));
            }
        }

        // GET: ClientesMatch/Listar
        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] FiltroClientesMatch filtro)
        {
            try
            {
                var lista = await _clientesMatchService.ObtenerTodosAsync(filtro);
                return Json(new { success = true, data = lista });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar clientes match");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ClientesMatch/ObtenerPorId/{id}
        [HttpGet]
        public async Task<IActionResult> ObtenerPorId(string id)
        {
            try
            {
                var cliente = await _clientesMatchService.ObtenerPorIdAsync(id);
                if (cliente == null)
                {
                    return NotFound(new { success = false, message = "Cliente Match no encontrado" });
                }
                return Json(new { success = true, data = cliente });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente match: {Id}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // NOTA: No se implementa CREATE porque los Clientes Match se crean 
        // automáticamente al convertir un Lead desde ClientesLeads

        // PUT: ClientesMatch/Actualizar
        [HttpPut]
        public async Task<IActionResult> Actualizar([FromBody] ClienteMatch cliente)
        {
            try
            {
                if (string.IsNullOrEmpty(cliente.ID_Interno))
                {
                    return BadRequest(new { success = false, message = "El ID es obligatorio" });
                }

                var resultado = await _clientesMatchService.ActualizarAsync(cliente);
                
                if (resultado)
                {
                    return Json(new { success = true, message = "Cliente Match actualizado exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo actualizar el Cliente Match" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente match");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // DELETE: ClientesMatch/Eliminar/{id}
        [HttpDelete]
        public async Task<IActionResult> Eliminar(string id)
        {
            try
            {
                var resultado = await _clientesMatchService.EliminarAsync(id);
                
                if (resultado)
                {
                    return Json(new { success = true, message = "Cliente Match eliminado exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo eliminar el Cliente Match" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente match: {Id}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ClientesMatch/ContarTotal
        [HttpGet]
        public async Task<IActionResult> ContarTotal()
        {
            try
            {
                var total = await _clientesMatchService.ContarTotalAsync();
                return Json(new { success = true, total = total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar clientes match");
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static IEnumerable<object> MapearMatchParaDataTable(IEnumerable<ClienteMatch> lista)
        {
            return lista.Select(m => new
            {
                iD_Interno = m.ID_Interno,
                tipo_Match = m.Tipo_Match,
                nombre = m.Nombre,
                rut = m.Rut,
                telefono = m.Telefono,
                correo = m.Correo,
                comuna = m.Comuna,
                profesion = m.Profesion
            });
        }
    }
}

