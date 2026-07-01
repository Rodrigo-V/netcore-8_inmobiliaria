using Inmobiliaria.Net8.DTOs;

namespace Inmobiliaria.Net8.Services
{
    public class ConfiguracionPortalesService
    {
        private readonly PropiedadesService _propiedadesService;
        private readonly string _connectionString;
        private readonly ILogger<ConfiguracionPortalesService> _logger;

        public ConfiguracionPortalesService(
            PropiedadesService propiedadesService,
            IConfiguration configuration,
            ILogger<ConfiguracionPortalesService> logger)
        {
            _propiedadesService = propiedadesService;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        public async Task<List<PropiedadPortalDTO>> ObtenerPropiedadesAsync(FiltroPropiedadDTO filtro)
        {
            var pageSize = filtro.PageSize > 0 ? filtro.PageSize : 10;
            var start = Math.Max(0, filtro.Min - 1);

            var (propiedades, totalRecords) = await _propiedadesService.ObtenerPropiedadesAsync(
                filtro.IDPropiedad,
                filtro.Columna,
                filtro.Direccion,
                start,
                pageSize);

            return propiedades.Select(p => new PropiedadPortalDTO
            {
                ID_Propiedad = p.ID_Propiedad ?? string.Empty,
                Codigo_Referencia = p.Codigo_Referencia ?? string.Empty,
                Titulo = p.Title ?? string.Empty,
                Direccion = p.Direccion ?? string.Empty,
                Comuna = p.Comuna ?? string.Empty,
                Estado_Propiedad = p.Estado ?? string.Empty,
                Url_Imagen = p.Url_Imagen,
                id_TocToc = p.id_TocToc,
                id_ChilePropiedades = p.id_ChilePropiedades,
                id_PortalInmobiliario = p.id_PortalInmobiliario,
                id_Proppit = p.id_Proppit,
                id_PortalRosch = p.id_PortalRosch,
                TotalRowCount = p.TotalRowCount > 0 ? p.TotalRowCount : totalRecords
            }).ToList();
        }

        public async Task<PortalIDsDTO?> ObtenerIDsPortalesAsync(string idPropiedad)
        {
            try
            {
                var propiedad = await _propiedadesService.ObtenerPorIdAsync(idPropiedad);
                if (propiedad == null)
                    return null;

                return new PortalIDsDTO
                {
                    ID_Propiedad = propiedad.ID_Propiedad ?? string.Empty,
                    id_TocToc = propiedad.id_TocToc,
                    id_ChilePropiedades = propiedad.id_ChilePropiedades,
                    id_PortalInmobiliario = propiedad.id_PortalInmobiliario,
                    id_Proppit = propiedad.id_Proppit,
                    id_PortalRosch = propiedad.id_PortalRosch
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener IDs de portales para propiedad {IdPropiedad}", idPropiedad);
                throw;
            }
        }

        public async Task<(bool Success, string Message)> GuardarIDsPortalesAsync(PortalIDsDTO ids)
        {
            try
            {
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new Microsoft.Data.SqlClient.SqlCommand("PP_psnp_Propiedad_ActualizarIDsPortales", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.CommandTimeout = Helpers.SqlDefaults.CommandTimeoutSeconds;

                command.Parameters.AddWithValue("@ID_Propiedad", ids.ID_Propiedad);
                command.Parameters.AddWithValue("@id_TocToc", string.IsNullOrEmpty(ids.id_TocToc) ? (object)DBNull.Value : ids.id_TocToc);
                command.Parameters.AddWithValue("@id_ChilePropiedades", string.IsNullOrEmpty(ids.id_ChilePropiedades) ? (object)DBNull.Value : ids.id_ChilePropiedades);
                command.Parameters.AddWithValue("@id_PortalInmobiliario", string.IsNullOrEmpty(ids.id_PortalInmobiliario) ? (object)DBNull.Value : ids.id_PortalInmobiliario);
                command.Parameters.AddWithValue("@id_Proppit", string.IsNullOrEmpty(ids.id_Proppit) ? (object)DBNull.Value : ids.id_Proppit);
                command.Parameters.AddWithValue("@id_PortalRosch", string.IsNullOrEmpty(ids.id_PortalRosch) ? (object)DBNull.Value : ids.id_PortalRosch);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var resultado = reader["Resultado"]?.ToString();
                    var mensaje = reader["Mensaje"]?.ToString() ?? "Operación completada";
                    return (resultado == "SUCCESS", mensaje);
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2812)
            {
                _logger.LogWarning("Stored Procedure PP_psnp_Propiedad_ActualizarIDsPortales no encontrado. Usando fallback query.");
                return await GuardarIDsPortalesFallbackAsync(ids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar IDs de portales para propiedad {IdPropiedad}", ids.ID_Propiedad);
                return (false, $"Error interno: {ex.Message}");
            }

            return (false, "No se recibió respuesta del servidor");
        }

        private async Task<(bool Success, string Message)> GuardarIDsPortalesFallbackAsync(PortalIDsDTO ids)
        {
            try
            {
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                    UPDATE Propiedades 
                    SET 
                        id_TocToc = @id_TocToc,
                        id_ChilePropiedades = @id_ChilePropiedades,
                        id_PortalInmobiliario = @id_PortalInmobiliario,
                        id_Proppit = @id_Proppit,
                        id_PortalRosch = @id_PortalRosch
                    WHERE ID_Propiedad = @ID_Propiedad";

                using var command = new Microsoft.Data.SqlClient.SqlCommand(query, connection);
                command.CommandTimeout = Helpers.SqlDefaults.CommandTimeoutSeconds;
                command.Parameters.AddWithValue("@ID_Propiedad", ids.ID_Propiedad);
                command.Parameters.AddWithValue("@id_TocToc", string.IsNullOrEmpty(ids.id_TocToc) ? (object)DBNull.Value : ids.id_TocToc);
                command.Parameters.AddWithValue("@id_ChilePropiedades", string.IsNullOrEmpty(ids.id_ChilePropiedades) ? (object)DBNull.Value : ids.id_ChilePropiedades);
                command.Parameters.AddWithValue("@id_PortalInmobiliario", string.IsNullOrEmpty(ids.id_PortalInmobiliario) ? (object)DBNull.Value : ids.id_PortalInmobiliario);
                command.Parameters.AddWithValue("@id_Proppit", string.IsNullOrEmpty(ids.id_Proppit) ? (object)DBNull.Value : ids.id_Proppit);
                command.Parameters.AddWithValue("@id_PortalRosch", string.IsNullOrEmpty(ids.id_PortalRosch) ? (object)DBNull.Value : ids.id_PortalRosch);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0
                    ? (true, "IDs actualizados correctamente (Fallback)")
                    : (false, "No se encontró la propiedad para actualizar");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en fallback de guardar IDs");
                return (false, $"Error en fallback: {ex.Message}");
            }
        }
    }
}
