using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.Services;
using System.Linq;

namespace Inmobiliaria.Net8.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SincronizacionPortalesController : Controller
    {
        private readonly SincronizacionPortalesService _sincronizacionService;
        private readonly ILogger<SincronizacionPortalesController> _logger;

        public SincronizacionPortalesController(
            SincronizacionPortalesService sincronizacionService,
            ILogger<SincronizacionPortalesController> logger)
        {
            _sincronizacionService = sincronizacionService;
            _logger = logger;
        }

        // GET: SincronizacionPortales/VistaMatriz
        [HttpGet]
        public async Task<IActionResult> VistaMatriz(string? propiedad, string? comuna, string? region, string? tipoPropiedad)
        {
            try
            {
                _logger.LogInformation("Accediendo a Vista Matriz de Sincronización de Portales");

                var matrizData = await _sincronizacionService.ObtenerMatrizSincronizacionAsync(propiedad, comuna, region, tipoPropiedad);
                var resumenPortales = _sincronizacionService.CalcularResumenDesdeMatriz(matrizData);

                ViewBag.MatrizData = matrizData;
                ViewBag.ResumenPortales = resumenPortales;
                ViewBag.PropiedadSeleccionada = propiedad;
                ViewBag.ComunaSeleccionada = comuna;
                ViewBag.RegionSeleccionada = region;
                ViewBag.TipoPropiedadSeleccionada = tipoPropiedad;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar vista matriz");
                TempData["Error"] = $"Error al cargar datos: {ex.Message}";
                return View();
            }
        }

        // GET: SincronizacionPortales/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Accediendo a Sincronización de Portales");
                
                var resumenPortales = await _sincronizacionService.ObtenerResumenPortalesAsync();
                ViewBag.ResumenPortales = resumenPortales;
                
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar sincronización de portales");
                TempData["Error"] = $"Error al cargar datos: {ex.Message}";
                return View();
            }
        }

        // GET: SincronizacionPortales/DescargarExcel
        [HttpGet]
        public async Task<IActionResult> DescargarExcel(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            try
            {
                _logger.LogInformation("===== INICIO DESCARGA EXCEL =====");
                _logger.LogInformation("Descargando Excel. FechaDesde={FechaDesde}, FechaHasta={FechaHasta}", 
                    fechaDesde?.ToString("yyyy-MM-dd") ?? "null", fechaHasta?.ToString("yyyy-MM-dd") ?? "null");

                // Obtener datos del SP
                _logger.LogInformation("Obteniendo datos del servicio...");
                var datos = await _sincronizacionService.ObtenerDatosExcelMatrizAsync(fechaDesde, fechaHasta);
                _logger.LogInformation("Datos obtenidos: {Count} registros", datos.Count);

                if (!datos.Any())
                {
                    _logger.LogWarning("No hay datos para exportar");
                    TempData["Error"] = "No hay datos para exportar en el rango de fechas seleccionado.";
                    return RedirectToAction("VistaMatriz");
                }

                // Generar Excel
                _logger.LogInformation("Generando archivo Excel...");
                var excelBytes = GenerarExcelMatriz(datos);
                _logger.LogInformation("Excel generado: {Size} bytes", excelBytes.Length);

                var fileName = $"MatrizSincronizacion_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                _logger.LogInformation("Descargando archivo: {FileName}", fileName);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar Excel");
                TempData["Error"] = $"Error al generar Excel: {ex.Message}";
                return RedirectToAction("VistaMatriz");
            }
        }

        private byte[] GenerarExcelMatriz(List<DTOs.DatosExcelMatriz> datos)
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Matriz Sincronización");

            // Agrupar datos por fecha y propiedad
            var fechas = datos.Select(d => d.FechaSincronizacion).Distinct().OrderByDescending(f => f).ToList();
            var propiedades = datos.Select(d => d.ID_Propiedad).Distinct().OrderBy(p => p).ToList();

            // FILA 1: Encabezados de propiedades (fusionados)
            worksheet.Cell(1, 1).Value = "Fecha";
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#4472C4");
            worksheet.Cell(1, 1).Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
            worksheet.Cell(1, 1).Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;

            int colInicio = 2;
            foreach (var propiedad in propiedades)
            {
                // Fusionar 4 columnas para cada propiedad
                var rangoPropiedad = worksheet.Range(1, colInicio, 1, colInicio + 3);
                rangoPropiedad.Merge();
                rangoPropiedad.Value = propiedad;
                rangoPropiedad.Style.Font.Bold = true;
                rangoPropiedad.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#4472C4");
                rangoPropiedad.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                rangoPropiedad.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                rangoPropiedad.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;
                rangoPropiedad.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Medium;

                colInicio += 4;
            }

            // FILA 2: Sub-encabezados de portales
            colInicio = 2;
            foreach (var propiedad in propiedades)
            {
                var portales = new[] { "Portal Inmobiliario", "Proppit", "Chile Propiedades", "TocToc" };
                for (int i = 0; i < 4; i++)
                {
                    var cell = worksheet.Cell(2, colInicio + i);
                    cell.Value = portales[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#5B9BD5");
                    cell.Style.Font.FontColor = ClosedXML.Excel.XLColor.White;
                    cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                    cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                }
                colInicio += 4;
            }

            // Fusionar celdas de "Fecha" en fila 1 y 2
            var fechaRange = worksheet.Range(1, 1, 2, 1);
            fechaRange.Merge();
            fechaRange.Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;

            // FILAS 3+: Datos
            int row = 3;
            foreach (var fecha in fechas)
            {
                // Columna A: Fecha
                var cellFecha = worksheet.Cell(row, 1);
                cellFecha.Value = fecha.ToString("yyyy-MM-dd");
                cellFecha.Style.Font.Bold = true;
                cellFecha.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#E7E6E6");
                cellFecha.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // Columnas B+: Clics por propiedad y portal
                int col = 2;
                foreach (var propiedad in propiedades)
                {
                    var datoProp = datos.FirstOrDefault(d => 
                        d.FechaSincronizacion.Date == fecha.Date && 
                        d.ID_Propiedad == propiedad);

                    if (datoProp != null)
                    {
                        worksheet.Cell(row, col).Value = datoProp.ClicsPortalInmobiliario;
                        worksheet.Cell(row, col + 1).Value = datoProp.ClicsProppit;
                        worksheet.Cell(row, col + 2).Value = datoProp.ClicsChilePropiedades;
                        worksheet.Cell(row, col + 3).Value = datoProp.ClicsTocToc;
                    }
                    else
                    {
                        // Si no hay datos, poner 0
                        worksheet.Cell(row, col).Value = 0;
                        worksheet.Cell(row, col + 1).Value = 0;
                        worksheet.Cell(row, col + 2).Value = 0;
                        worksheet.Cell(row, col + 3).Value = 0;
                    }

                    // Estilo para las celdas de datos
                    for (int i = 0; i < 4; i++)
                    {
                        var cell = worksheet.Cell(row, col + i);
                        cell.Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                        cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        
                        // Resaltar valores > 0 con color verde claro
                        if (!cell.IsEmpty() && cell.GetValue<int>() > 0)
                        {
                            cell.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.FromHtml("#D5F5D5");
                        }
                    }

                    col += 4;
                }

                row++;
            }

            // Ajustar ancho de columnas
            worksheet.Column(1).Width = 15; // Columna Fecha
            for (int i = 2; i <= propiedades.Count * 4 + 1; i++)
            {
                worksheet.Column(i).Width = 12; // Columnas de portales
            }

            // Congelar paneles (primera columna y primeras 2 filas)
            worksheet.SheetView.FreezeRows(2);
            worksheet.SheetView.FreezeColumns(1);

            // Convertir a bytes
            using var stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
    }
}

