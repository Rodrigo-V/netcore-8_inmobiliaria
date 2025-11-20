using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.Services;
using Inmobiliaria.Net8.DTOs;

namespace Inmobiliaria.Net8.Controllers
{
    [Authorize] // Requiere autenticación
    public class PropiedadesController : Controller
    {
        private readonly PropiedadesService _propiedadesService;
        private readonly ILogger<PropiedadesController> _logger;

        public PropiedadesController(PropiedadesService propiedadesService, ILogger<PropiedadesController> logger)
        {
            _propiedadesService = propiedadesService;
            _logger = logger;
        }

        // GET: Propiedades/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: Propiedades/GetData - DataTables server-side
        [HttpPost]
        public async Task<IActionResult> GetData()
        {
            try
            {
                var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault() ?? "1");
                var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
                var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
                var orderColumnIndex = Request.Form["order[0][column]"].FirstOrDefault() ?? "0";
                var orderDir = Request.Form["order[0][dir]"].FirstOrDefault() ?? "desc";
                var searchValue = Request.Form["search[value]"].FirstOrDefault();

                // Mapear columna para ordenamiento (nombres que espera el SP)
                string columna = orderColumnIndex switch
                {
                    "0" => "ID_Propiedad",
                    "1" => "Codigo_Referencia",
                    "2" => "Titulo", // SP espera "Titulo"
                    "3" => "Tipo_Propiedad", // SP espera "Tipo_Propiedad"
                    "4" => "Precio", // SP espera "Precio"
                    "5" => "Comuna",
                    "9" => "Agente_Responsable",
                    _ => "ID_Propiedad" // Default a ID_Propiedad
                };

                var (propiedades, totalRecords) = await _propiedadesService.ObtenerPropiedadesAsync(
                    searchValue,
                    columna,
                    orderDir,
                    start,
                    length
                );

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = propiedades.Select(p => new
                    {
                        id_Propiedad = p.ID_Propiedad,
                        codigo_Referencia = p.Codigo_Referencia,
                        title = p.Title,
                        tipo_elemento = p.Tipo_elemento,
                        valor = p.Valor,
                        comuna = p.Comuna,
                        dormitorios_Banos = $"{p.Dormitorios ?? "0"}/{p.Banos ?? "0"}",
                        metros = $"{p.M2_Construidos ?? "0"} / {p.M2_Terreno ?? "0"}",
                        estado = p.Estado,
                        agente_Responsable = p.Agente_Responsable
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos de propiedades");
                return Json(new
                {
                    draw = 0,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }

        // POST: Propiedades/ObtenerPorId
        [HttpPost]
        public async Task<IActionResult> ObtenerPorId(string idPropiedad)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idPropiedad))
                {
                    return BadRequest(new { success = false, message = "ID de propiedad requerido" });
                }

                var propiedad = await _propiedadesService.ObtenerPorIdAsync(idPropiedad);

                if (propiedad == null)
                {
                    return NotFound(new { success = false, message = $"Propiedad con ID '{idPropiedad}' no encontrada" });
                }

                return PartialView("ModificarModal", propiedad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedad por ID: {IdPropiedad}", idPropiedad);
                return StatusCode(500, new { success = false, message = $"Error interno: {ex.Message}" });
            }
        }

        // POST: Propiedades/Actualizar
        [HttpPost]
        public async Task<IActionResult> Actualizar([FromBody] Propiedad propiedad)
        {
            try
            {
                if (propiedad == null || string.IsNullOrWhiteSpace(propiedad.ID_Propiedad))
                {
                    return BadRequest(new { success = false, message = "Datos de propiedad inválidos" });
                }

                _logger.LogInformation("Actualizando propiedad: {IdPropiedad}", propiedad.ID_Propiedad);

                await _propiedadesService.ActualizarAsync(propiedad);

                return Json(new { success = true, message = "Propiedad actualizada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar propiedad: {IdPropiedad}", propiedad?.ID_Propiedad);
                return Json(new { success = false, message = $"Error al actualizar la propiedad: {ex.Message}" });
            }
        }

        // POST: Propiedades/Agregar
        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] Propiedad propiedad)
        {
            try
            {
                if (propiedad == null)
                {
                    return BadRequest(new { success = false, message = "Datos de propiedad inválidos" });
                }

                _logger.LogInformation("Agregando nueva propiedad");

                await _propiedadesService.AgregarAsync(propiedad);

                return Json(new { success = true, message = "Propiedad agregada correctamente", idPropiedad = propiedad.ID_Propiedad });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar propiedad");
                return Json(new { success = false, message = $"Error al agregar la propiedad: {ex.Message}" });
            }
        }

        // POST: Propiedades/Eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(string idPropiedad)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idPropiedad))
                {
                    return BadRequest(new { success = false, message = "ID de propiedad requerido" });
                }

                await _propiedadesService.EliminarAsync(idPropiedad);

                return Json(new { success = true, message = "Propiedad eliminada correctamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar propiedad: {IdPropiedad}", idPropiedad);
                return Json(new { success = false, message = $"Error al eliminar la propiedad: {ex.Message}" });
            }
        }

        // GET: Propiedades/Estadisticas
        [HttpGet]
        public async Task<IActionResult> Estadisticas()
        {
            try
            {
                _logger.LogInformation("Obteniendo estadísticas de propiedades");

                var estadisticas = await _propiedadesService.ObtenerEstadisticasAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        total = estadisticas.Total,
                        estados = new
                        {
                            disponible = estadisticas.Disponible,
                            vendida = estadisticas.Vendida,
                            reservada = estadisticas.Reservada,
                            arrendada = estadisticas.Arrendada,
                            suspendida = estadisticas.Suspendida,
                            otros = estadisticas.Otros
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return Json(new { success = false, message = $"Error al obtener estadísticas: {ex.Message}" });
            }
        }

        // GET: Propiedades/AgregarModal
        [HttpGet]
        public IActionResult AgregarModal()
        {
            return PartialView("AgregarModal", new Propiedad());
        }

        // GET: Propiedades/ObtenerTiposPropiedad
        [HttpGet]
        public async Task<IActionResult> ObtenerTiposPropiedad()
        {
            try
            {
                var tipos = await _propiedadesService.ObtenerTiposPropiedadAsync();
                return Json(new { success = true, data = tipos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de propiedad");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Propiedades/ObtenerEstados
        [HttpGet]
        public async Task<IActionResult> ObtenerEstados()
        {
            try
            {
                var estados = await _propiedadesService.ObtenerEstadosAsync();
                return Json(new { success = true, data = estados });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estados");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Propiedades/ObtenerAgentes
        [HttpGet]
        public async Task<IActionResult> ObtenerAgentes()
        {
            try
            {
                var agentes = await _propiedadesService.ObtenerAgentesAsync();
                return Json(new { success = true, data = agentes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agentes");
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}

