using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Inmobiliaria.Net8.Services;
using Inmobiliaria.Net8.DTOs;

namespace Inmobiliaria.Net8.Controllers
{
    [Authorize(Roles = "Administrador")]
    [Route("[controller]")]
    public class CargadorExcelPropitController : Controller
    {
        private readonly CargadorExcelPropitService _excelService;
        private readonly ILogger<CargadorExcelPropitController> _logger;

        public CargadorExcelPropitController(CargadorExcelPropitService excelService, ILogger<CargadorExcelPropitController> logger)
        {
            _excelService = excelService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarExcel(IFormFile archivo)
        {
            var result = await _excelService.ProcessExcelAsync(archivo);
            return Json(new
            {
                success = result.Exitoso,
                message = result.Mensaje,
                errores = result.Errores,
                resumen = result.Resumen,
                clicsProcesados = result.ClicsProcesados,
                propiedadesEncontradas = result.PropiedadesEncontradas,
                propiedadesNoEncontradas = result.PropiedadesNoEncontradas
            });
        }
    }
}
