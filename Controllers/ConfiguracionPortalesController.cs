using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.Services;
using Inmobiliaria.Net8.DTOs;

namespace Inmobiliaria.Net8.Controllers
{
    public class ConfiguracionPortalesController : Controller
    {
        private readonly ConfiguracionPortalesService _service;
        private readonly ILogger<ConfiguracionPortalesController> _logger;

        public ConfiguracionPortalesController(ConfiguracionPortalesService service, ILogger<ConfiguracionPortalesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 10;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int pageNumber = (skip / pageSize) + 1;

                var filtro = new FiltroPropiedadDTO
                {
                    IDPropiedad = searchValue,
                    Min = skip + 1,
                    Max = skip + pageSize,
                    PageSize = pageSize,
                    Columna = GetOrderColumn(sortColumnIndex),
                    Direccion = sortDirection ?? "DESC"
                };

                var propiedades = await _service.ObtenerPropiedadesAsync(filtro);
                
                var totalRecords = propiedades.FirstOrDefault()?.TotalRowCount ?? 0;

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = propiedades
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos para tabla");
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerIDsPortales(string idPropiedad)
        {
            try
            {
                if (string.IsNullOrEmpty(idPropiedad))
                {
                    return Json(new { success = false, message = "ID de propiedad es requerido" });
                }

                var data = await _service.ObtenerIDsPortalesAsync(idPropiedad);
                
                if (data != null)
                {
                    return Json(new { success = true, data = data });
                }
                else
                {
                    return Json(new { success = false, message = "Propiedad no encontrada" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarIDsPortales(PortalIDsDTO ids)
        {
            try
            {
                if (string.IsNullOrEmpty(ids.ID_Propiedad))
                {
                    return Json(new { success = false, message = "ID de propiedad es requerido" });
                }

                var result = await _service.GuardarIDsPortalesAsync(ids);
                
                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error interno: {ex.Message}" });
            }
        }

        private string GetOrderColumn(string? columnIndex)
        {
            return columnIndex switch
            {
                "0" => "ID_Propiedad",
                "2" => "Titulo",
                "3" => "Direccion",
                "4" => "id_TocToc",
                "5" => "id_ChilePropiedades",
                "6" => "id_PortalInmobiliario",
                "7" => "id_Proppit",
                "8" => "id_PortalRosch",
                _ => "Fecha_Publicacion"
            };
        }
    }
}
