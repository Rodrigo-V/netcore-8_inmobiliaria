using System.Collections.Generic;

namespace Inmobiliaria.Net8.DTOs
{
    /// <summary>
    /// Result of processing an Excel file for Propit.
    /// </summary>
    public class CargadorExcelPropitResultDTO
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
        public int ClicsProcesados { get; set; }
        public int PropiedadesEncontradas { get; set; }
        public int PropiedadesNoEncontradas { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
        public string Resumen { get; set; }
    }
}
