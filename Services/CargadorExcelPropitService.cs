using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Inmobiliaria.Net8.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Inmobiliaria.Net8.Services
{
    /// <summary>
    /// Service to handle Excel file processing for Propit.
    /// </summary>
    public class CargadorExcelPropitService
    {
        private readonly ILogger<CargadorExcelPropitService> _logger;
        private readonly string _connectionString;

        public CargadorExcelPropitService(ILogger<CargadorExcelPropitService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        /// <summary>
        /// Validates the uploaded file and returns a result DTO.
        /// </summary>
        public async Task<CargadorExcelPropitResultDTO> ProcessExcelAsync(IFormFile file)
        {
            var result = new CargadorExcelPropitResultDTO();
            try
            {
                if (file == null || file.Length == 0)
                {
                    result.Exitoso = false;
                    result.Mensaje = "No se ha enviado ningún archivo o está vacío.";
                    return result;
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension != ".xlsx" && extension != ".xls")
                {
                    result.Exitoso = false;
                    result.Mensaje = "Solo se permiten archivos Excel (.xlsx, .xls).";
                    return result;
                }

                if (file.Length > 10 * 1024 * 1024)
                {
                    result.Exitoso = false;
                    result.Mensaje = "El archivo supera el tamaño máximo permitido de 10 MB.";
                    return result;
                }

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            result.Exitoso = false;
                            result.Mensaje = "El archivo Excel no contiene ninguna hoja.";
                            return result;
                        }

                        // Process rows
                        await ProcessRowsAsync(worksheet, result);
                    }
                }

                result.Exitoso = true;
                result.Mensaje = "Archivo procesado correctamente.";
                result.Resumen = GenerarResumen(result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Excel file for Propit.");
                result.Exitoso = false;
                result.Mensaje = $"Error al procesar el archivo: {ex.Message}";
                result.Errores.Add(ex.Message);
                return result;
            }
        }

        private async Task ProcessRowsAsync(IXLWorksheet worksheet, CargadorExcelPropitResultDTO result)
        {
            var rows = worksheet.RowsUsed().Skip(1); // Skip header
            int rowIndex = 2; // 1-based index, starting from 2 since we skipped header

            foreach (var row in rows)
            {
                try
                {
                    // Column A: ID Propiedad Proppit (1)
                    // Column F: Título (6)
                    // Column AC: Visitas (29)

                    var idProppit = row.Cell(1).GetValue<string>();
                    var titulo = row.Cell(6).GetValue<string>();
                    var visitasStr = row.Cell(29).GetValue<string>();

                    if (string.IsNullOrWhiteSpace(idProppit))
                    {
                        // Skip empty rows
                        rowIndex++;
                        continue;
                    }

                    if (!int.TryParse(visitasStr, out int visitas))
                    {
                        visitas = 0;
                    }

                    // 1. Find and Link Property
                    string idPropiedad = await BuscarYVincularPropiedadAsync(idProppit, rowIndex, result.Errores);

                    if (!string.IsNullOrEmpty(idPropiedad))
                    {
                        result.PropiedadesEncontradas++;

                        // 2. Register Clics (Visits)
                        await CrearClicConSPAsync(idPropiedad, idProppit, titulo, visitas, rowIndex);
                        result.ClicsProcesados += visitas;
                    }
                    else
                    {
                        result.PropiedadesNoEncontradas++;
                        result.Errores.Add($"Fila {rowIndex}: No se encontró propiedad para ID Proppit '{idProppit}'");
                    }
                }
                catch (Exception ex)
                {
                    result.Errores.Add($"Error en fila {rowIndex}: {ex.Message}");
                }
                rowIndex++;
            }
        }

        private async Task<string> BuscarYVincularPropiedadAsync(string idProppit, int fila, List<string> errores)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // 1. Search by id_Proppit
                    var queryByProppit = "SELECT ID_Propiedad FROM Propiedades WHERE id_Proppit = @idProppit";
                    using (var command = new SqlCommand(queryByProppit, connection))
                    {
                        command.Parameters.AddWithValue("@idProppit", idProppit);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            return result.ToString();
                        }
                    }

                    // 2. Search by ID_Propiedad
                    var queryById = "SELECT ID_Propiedad, id_Proppit FROM Propiedades WHERE ID_Propiedad = @ID_Propiedad";
                    using (var command = new SqlCommand(queryById, connection))
                    {
                        command.Parameters.AddWithValue("@ID_Propiedad", idProppit);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var idPropiedad = reader["ID_Propiedad"].ToString();
                                reader.Close(); // Close before update

                                // Update id_Proppit
                                await ActualizarIDProppitAsync(idPropiedad, idProppit, connection);
                                return idPropiedad;
                            }
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                errores.Add($"Error buscando propiedad para ID Proppit {idProppit} (fila {fila}): {ex.Message}");
                return string.Empty;
            }
        }

        private async Task ActualizarIDProppitAsync(string idPropiedad, string idProppit, SqlConnection connection)
        {
            try
            {
                using (var command = new SqlCommand("PP_psnp_Propiedad_ActualizarIDsPortales", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ID_Propiedad", idPropiedad);
                    command.Parameters.AddWithValue("@id_Proppit", idProppit);
                    command.Parameters.AddWithValue("@id_TocToc", DBNull.Value);
                    command.Parameters.AddWithValue("@id_ChilePropiedades", DBNull.Value);
                    command.Parameters.AddWithValue("@id_PortalInmobiliario", DBNull.Value);
                    command.Parameters.AddWithValue("@id_PortalRosch", DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex) when (ex.Number == 2812) // SP not found
            {
                // Fallback update
                var query = "UPDATE Propiedades SET id_Proppit = @id_Proppit WHERE ID_Propiedad = @ID_Propiedad";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ID_Propiedad", idPropiedad);
                    command.Parameters.AddWithValue("@id_Proppit", idProppit);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task CrearClicConSPAsync(string propiedadId, string idProppit, string tituloPropiedad, int visitas, int fila)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                int portalId = await ObtenerIdPortalProppitAsync(connection);

                if (portalId == 0) return; // Could not find portal

                using (var command = new SqlCommand("sp_InsertarClicPortal", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@PortalId", portalId);
                    command.Parameters.AddWithValue("@PropiedadId", propiedadId);
                    command.Parameters.AddWithValue("@PropiedadCodigo", idProppit);
                    command.Parameters.AddWithValue("@PropiedadTitulo", tituloPropiedad ?? "");
                    command.Parameters.AddWithValue("@FechaClic", DateTime.Now);
                    command.Parameters.AddWithValue("@IpUsuario", "PROPPIT_EXCEL");
                    command.Parameters.AddWithValue("@UserAgent", "ProppitExcel/1.0");
                    command.Parameters.AddWithValue("@Referrer", "https://proppit.com");
                    command.Parameters.AddWithValue("@UrlDestino", "");
                    command.Parameters.AddWithValue("@TipoClic", "VISITA");
                    command.Parameters.AddWithValue("@Visitas", visitas);
                    command.Parameters.AddWithValue("@UbicacionGeografica", "Chile");
                    command.Parameters.AddWithValue("@Sincronizado", true);
                    command.Parameters.AddWithValue("@FechaSincronizacion", DateTime.Now);
                    command.Parameters.AddWithValue("@DatosAdicionales", $"{{\"id_Proppit\":\"{idProppit}\",\"titulo_proppit\":\"{tituloPropiedad}\",\"visitas_totales\":{visitas},\"fila_excel\":{fila}}}");

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task<int> ObtenerIdPortalProppitAsync(SqlConnection connection)
        {
            var query = "SELECT TOP 1 Id FROM PortalesInmobiliarios WHERE Nombre LIKE '%Proppit%' OR Nombre LIKE '%Propit%'";
            using (var command = new SqlCommand(query, connection))
            {
                var result = await command.ExecuteScalarAsync();
                if (result != null && int.TryParse(result.ToString(), out int id))
                {
                    return id;
                }
            }
            return 0;
        }

        private string GenerarResumen(CargadorExcelPropitResultDTO result)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== RESUMEN DE PROCESAMIENTO ===");
            sb.AppendLine($"✅ Visitas procesadas: {result.ClicsProcesados}");
            sb.AppendLine($"✅ Propiedades encontradas: {result.PropiedadesEncontradas}");
            sb.AppendLine($"❌ Propiedades no encontradas: {result.PropiedadesNoEncontradas}");

            if (result.Errores.Any())
            {
                sb.AppendLine("\n=== ERRORES DETECTADOS ===");
                foreach (var error in result.Errores.Take(10))
                {
                    sb.AppendLine($"• {error}");
                }
                if (result.Errores.Count > 10)
                {
                    sb.AppendLine($"... y {result.Errores.Count - 10} errores más");
                }
            }
            return sb.ToString();
        }
    }
}
