using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.DTOs;
using Inmobiliaria.Net8.Services;

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
    }
}

