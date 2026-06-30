using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria.Net8.DTOs;

namespace Inmobiliaria.Net8.Services
{
    public class APIConfiguracionService
    {
        private readonly string _connectionString;
        private readonly ILogger<APIConfiguracionService> _logger;

        public APIConfiguracionService(IConfiguration configuration, ILogger<APIConfiguracionService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        /// <summary>
        /// Obtiene configuraciones de API según filtro
        /// </summary>
        public async Task<List<APIConfiguracion>> ObtenerConfiguracionesAsync(FiltroAPIConfiguracion? filtro = null)
        {
            var configuraciones = new List<APIConfiguracion>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Intentar primero con el SP
                try 
                {
                    using var command = new SqlCommand("SP_OBTENER_API_CONFIG", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    if (filtro?.TipoAPI != null)
                        command.Parameters.AddWithValue("@TipoAPI", filtro.TipoAPI);

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        configuraciones.Add(MapFromReader(reader));
                    }
                }
                catch (SqlException)
                {
                    // Si falla el SP, intentar consulta directa (fallback)
                    _logger.LogWarning("SP_OBTENER_API_CONFIG no encontrado, usando consulta directa.");
                    
                    using var commandFallback = new SqlCommand(@"SELECT 
                        Id, Nombre, TipoAPI, Url, EndpointUrl, ApiKey, ApiSecret,
                        Descripcion, Activo, FechaCreacion, FechaUltimaSincronizacion,
                        ConfiguracionAdicional
                    FROM dbo.APIConfiguracion
                    WHERE (@TipoAPI IS NULL OR TipoAPI = @TipoAPI)", connection);

                    commandFallback.Parameters.AddWithValue("@TipoAPI", (object?)filtro?.TipoAPI ?? DBNull.Value);

                    using var readerFallback = await commandFallback.ExecuteReaderAsync();
                    while (await readerFallback.ReadAsync())
                    {
                        configuraciones.Add(MapFromReader(readerFallback));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuraciones de API");
                throw;
            }

            return configuraciones;
        }

        /// <summary>
        /// Obtiene una configuración por ID
        /// </summary>
        public async Task<APIConfiguracion?> ObtenerPorIdAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_OBTENER_API_POR_ID", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);
                
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapFromReader(reader);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración por ID: {Id}", id);
                throw;
            }

            return null;
        }

        /// <summary>
        /// Crea o actualiza una configuración de API
        /// </summary>
        public async Task<int> GuardarAsync(APIConfiguracion configuracion)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_GUARDAR_API_CONFIG", connection);
                command.CommandType = CommandType.StoredProcedure;
                
                command.Parameters.AddWithValue("@Id", configuracion.Id);
                command.Parameters.AddWithValue("@Nombre", configuracion.Nombre);
                command.Parameters.AddWithValue("@TipoAPI", configuracion.TipoAPI);
                command.Parameters.AddWithValue("@Url", configuracion.Url);
                command.Parameters.AddWithValue("@EndpointUrl", configuracion.EndpointUrl);
                command.Parameters.AddWithValue("@ApiKey", (object?)configuracion.ApiKey ?? DBNull.Value);
                command.Parameters.AddWithValue("@ApiSecret", (object?)configuracion.ApiSecret ?? DBNull.Value);
                command.Parameters.AddWithValue("@Descripcion", (object?)configuracion.Descripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("@Activo", configuracion.Activo);
                command.Parameters.AddWithValue("@ConfiguracionAdicional", (object?)configuracion.ConfiguracionAdicional ?? DBNull.Value);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar configuración de API");
                throw;
            }
        }

        /// <summary>
        /// Elimina una configuración de API
        /// </summary>
        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_ELIMINAR_API_CONFIG", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", id);
                
                var result = await command.ExecuteScalarAsync();
                return Convert.ToBoolean(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar configuración de API: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Guarda el resultado de una prueba de API
        /// </summary>
        public async Task<int> GuardarTestResultAsync(APITestResult testResult)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_GUARDAR_API_TEST", connection);
                command.CommandType = CommandType.StoredProcedure;
                
                command.Parameters.AddWithValue("@APIConfiguracionId", testResult.APIConfiguracionId);
                command.Parameters.AddWithValue("@Exitoso", testResult.Exitoso);
                command.Parameters.AddWithValue("@StatusCode", (object?)testResult.StatusCode ?? DBNull.Value);
                command.Parameters.AddWithValue("@Mensaje", (object?)testResult.Mensaje ?? DBNull.Value);
                command.Parameters.AddWithValue("@Url", (object?)testResult.Url ?? DBNull.Value);
                command.Parameters.AddWithValue("@TiempoRespuesta", (object?)testResult.TiempoRespuesta ?? DBNull.Value);
                command.Parameters.AddWithValue("@DatosRecibidos", (object?)testResult.DatosRecibidos ?? DBNull.Value);
                command.Parameters.AddWithValue("@ErrorDetalle", (object?)testResult.ErrorDetalle ?? DBNull.Value);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar resultado de test API");
                throw;
            }
        }

        /// <summary>
        /// Guarda el resultado de una sincronización de API
        /// </summary>
        public async Task<int> GuardarSincronizacionAsync(APISincronizacion sincronizacion)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_GUARDAR_API_SYNC", connection);
                command.CommandType = CommandType.StoredProcedure;
                
                command.Parameters.AddWithValue("@APIConfiguracionId", sincronizacion.APIConfiguracionId);
                command.Parameters.AddWithValue("@FechaInicio", sincronizacion.FechaInicio);
                command.Parameters.AddWithValue("@FechaFin", (object?)sincronizacion.FechaFin ?? DBNull.Value);
                command.Parameters.AddWithValue("@Exitoso", sincronizacion.Exitoso);
                command.Parameters.AddWithValue("@Mensaje", (object?)sincronizacion.Mensaje ?? DBNull.Value);
                command.Parameters.AddWithValue("@RegistrosProcesados", sincronizacion.RegistrosProcesados);
                command.Parameters.AddWithValue("@DuracionSegundos", (object?)sincronizacion.DuracionSegundos ?? DBNull.Value);
                command.Parameters.AddWithValue("@ErrorDetalle", (object?)sincronizacion.ErrorDetalle ?? DBNull.Value);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar sincronización de API");
                throw;
            }
        }

        /// <summary>
        /// Obtiene estadísticas de APIs
        /// </summary>
        public async Task<List<APIEstadisticas>> ObtenerEstadisticasAsync(string? tipoAPI = null)
        {
            var estadisticas = new List<APIEstadisticas>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_OBTENER_API_STATS", connection);
                command.CommandType = CommandType.StoredProcedure;
                
                if (!string.IsNullOrEmpty(tipoAPI))
                    command.Parameters.AddWithValue("@TipoAPI", tipoAPI);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    estadisticas.Add(MapEstadisticasFromReader(reader));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de API");
                throw;
            }

            return estadisticas;
        }

        /// <summary>
        /// Inserta un clic o dato de portal (usado en sincronización)
        /// </summary>
        public async Task InsertarClicPortalAsync(int portalId, string propiedadId, string propiedadCodigo, 
            string propiedadTitulo, DateTime fechaClic, string ipUsuario, string userAgent, 
            string referrer, string urlDestino, string tipoClic, int visitas, 
            string ubicacionGeografica, bool sincronizado, DateTime fechaSincronizacion, string datosAdicionales)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("sp_InsertarClicPortal", connection);
                command.CommandType = CommandType.StoredProcedure;
                
                command.Parameters.AddWithValue("@PortalId", portalId);
                command.Parameters.AddWithValue("@PropiedadId", propiedadId);
                command.Parameters.AddWithValue("@PropiedadCodigo", propiedadCodigo);
                command.Parameters.AddWithValue("@PropiedadTitulo", propiedadTitulo);
                command.Parameters.AddWithValue("@FechaClic", fechaClic);
                command.Parameters.AddWithValue("@IpUsuario", ipUsuario);
                command.Parameters.AddWithValue("@UserAgent", userAgent);
                command.Parameters.AddWithValue("@Referrer", referrer);
                command.Parameters.AddWithValue("@UrlDestino", urlDestino);
                command.Parameters.AddWithValue("@TipoClic", tipoClic);
                command.Parameters.AddWithValue("@Visitas", visitas);
                command.Parameters.AddWithValue("@UbicacionGeografica", ubicacionGeografica);
                command.Parameters.AddWithValue("@Sincronizado", sincronizado);
                command.Parameters.AddWithValue("@FechaSincronizacion", fechaSincronizacion);
                command.Parameters.AddWithValue("@DatosAdicionales", datosAdicionales);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al insertar clic de portal");
                throw;
            }
        }

        private APIConfiguracion MapFromReader(SqlDataReader reader)
        {
            return new APIConfiguracion
            {
                Id = Convert.ToInt32(reader["Id"]),
                Nombre = reader["Nombre"].ToString() ?? string.Empty,
                TipoAPI = reader["TipoAPI"].ToString() ?? string.Empty,
                Url = reader["Url"].ToString() ?? string.Empty,
                EndpointUrl = reader["EndpointUrl"].ToString() ?? string.Empty,
                ApiKey = reader["ApiKey"] != DBNull.Value ? reader["ApiKey"].ToString() : null,
                ApiSecret = reader["ApiSecret"] != DBNull.Value ? reader["ApiSecret"].ToString() : null,
                Descripcion = reader["Descripcion"] != DBNull.Value ? reader["Descripcion"].ToString() : null,
                Activo = Convert.ToBoolean(reader["Activo"]),
                FechaCreacion = Convert.ToDateTime(reader["FechaCreacion"]),
                FechaUltimaSincronizacion = reader["FechaUltimaSincronizacion"] != DBNull.Value 
                    ? Convert.ToDateTime(reader["FechaUltimaSincronizacion"]) 
                    : null,
                ConfiguracionAdicional = reader["ConfiguracionAdicional"] != DBNull.Value ? reader["ConfiguracionAdicional"].ToString() : null
            };
        }

        private APIEstadisticas MapEstadisticasFromReader(SqlDataReader reader)
        {
            return new APIEstadisticas
            {
                Id = Convert.ToInt32(reader["Id"]),
                Nombre = reader["Nombre"].ToString() ?? string.Empty,
                TipoAPI = reader["TipoAPI"].ToString() ?? string.Empty,
                Activo = Convert.ToBoolean(reader["Activo"]),
                FechaUltimaSincronizacion = reader["FechaUltimaSincronizacion"] != DBNull.Value 
                    ? Convert.ToDateTime(reader["FechaUltimaSincronizacion"]) 
                    : null,
                UltimaPrueba = reader["UltimaPrueba"] != DBNull.Value 
                    ? Convert.ToDateTime(reader["UltimaPrueba"]) 
                    : null,
                UltimaPruebaExitoso = reader["UltimaPruebaExitoso"] != DBNull.Value 
                    ? Convert.ToBoolean(reader["UltimaPruebaExitoso"]) 
                    : null,
                UltimaSincronizacion = reader["UltimaSincronizacion"] != DBNull.Value 
                    ? Convert.ToDateTime(reader["UltimaSincronizacion"]) 
                    : null,
                UltimaSincronizacionExitoso = reader["UltimaSincronizacionExitoso"] != DBNull.Value 
                    ? Convert.ToBoolean(reader["UltimaSincronizacionExitoso"]) 
                    : null,
                TotalPruebas = Convert.ToInt32(reader["TotalPruebas"]),
                PruebasExitosas = Convert.ToInt32(reader["PruebasExitosas"]),
                TotalSincronizaciones = Convert.ToInt32(reader["TotalSincronizaciones"]),
                SincronizacionesExitosas = Convert.ToInt32(reader["SincronizacionesExitosas"]),
                TotalRegistrosProcesados = reader["TotalRegistrosProcesados"] != DBNull.Value 
                    ? Convert.ToInt32(reader["TotalRegistrosProcesados"]) 
                    : null
            };
        }
    }
}
