using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.Services;
using Inmobiliaria.Net8.DTOs;
using Inmobiliaria.Net8.Helpers;

namespace Inmobiliaria.Net8.Controllers
{
    [Authorize(Roles = "Administrador")]
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
            var request = DataTablesHelper.Parse(Request.Form);
            try
            {
                var filtro = new FiltroPropiedadDTO
                {
                    IDPropiedad = request.SearchValue,
                    Min = request.Start + 1,
                    Max = request.Start + request.Length,
                    PageSize = request.Length,
                    Columna = GetOrderColumn(request.OrderColumnIndex),
                    Direccion = request.OrderDirection
                };

                var propiedades = await _service.ObtenerPropiedadesAsync(filtro);
                var totalRecords = propiedades.FirstOrDefault()?.TotalRowCount ?? 0;

                return Json(DataTablesHelper.Success(request.Draw, totalRecords, MapearPortalesParaDataTable(propiedades)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos para tabla");
                return Json(DataTablesHelper.Error(request.Draw, ex.Message));
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

        private static string GetOrderColumn(string? columnIndex)
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
                _ => "ID_Propiedad"
            };
        }

        private static IEnumerable<object> MapearPortalesParaDataTable(IEnumerable<PropiedadPortalDTO> propiedades)
        {
            return propiedades.Select(p => new
            {
                iD_Propiedad = p.ID_Propiedad,
                codigo_Referencia = p.Codigo_Referencia,
                titulo = p.Titulo,
                direccion = p.Direccion,
                comuna = p.Comuna,
                url_Imagen = p.Url_Imagen,
                imagen_Propiedad = p.Imagen_Propiedad,
                id_TocToc = p.id_TocToc,
                id_ChilePropiedades = p.id_ChilePropiedades,
                id_PortalInmobiliario = p.id_PortalInmobiliario,
                id_Proppit = p.id_Proppit,
                id_PortalRosch = p.id_PortalRosch
            });
        }
    }
}
