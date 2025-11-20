using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria.Net8.DTOs;

namespace Inmobiliaria.Net8.Services
{
    public class PropiedadesService
    {
        private readonly string _connectionString;
        private readonly ILogger<PropiedadesService> _logger;

        public PropiedadesService(IConfiguration configuration, ILogger<PropiedadesService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        /// <summary>
        /// Obtener propiedades con paginación y filtros
        /// </summary>
        public async Task<(List<Propiedad> propiedades, int totalRecords)> ObtenerPropiedadesAsync(
            string? idPropiedad = null,
            string columna = "Fecha_Publicacion",
            string direccion = "desc",
            int start = 0,
            int length = 10)
        {
            var propiedades = new List<Propiedad>();
            int totalRecords = 0;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_Propiedad_SelectxTipoEmpresa", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 120;

                // El SP solo acepta búsqueda exacta por ID, para búsqueda global enviar NULL
                command.Parameters.AddWithValue("@IDPropiedad", DBNull.Value); // Siempre NULL para traer todos
                command.Parameters.AddWithValue("@Columna", columna);
                command.Parameters.AddWithValue("@Direccion", direccion);
                command.Parameters.AddWithValue("@Min", start + 1);
                command.Parameters.AddWithValue("@Max", start + length);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var propiedad = new Propiedad
                    {
                        ID_Propiedad = reader["ID_Propiedad"]?.ToString(),
                        Codigo_Referencia = reader["Codigo_Referencia"]?.ToString(),
                        Title = reader["Titulo"]?.ToString(), // SP devuelve "Titulo"
                        Description = reader["Descripcion"]?.ToString(),
                        Tipo_elemento = reader["Tipo_Propiedad"]?.ToString(), // SP devuelve "Tipo_Propiedad"
                        Direccion = reader["Direccion"]?.ToString(),
                        Comuna = reader["Comuna"]?.ToString(),
                        Ciudad = reader["Ciudad"]?.ToString(),
                        Region = reader["Region"]?.ToString(),
                        Valor = reader["Precio"]?.ToString(), // SP devuelve "Precio"
                        Precio_UF = reader["Precio_UF"]?.ToString(),
                        Dormitorios = reader["Dormitorios"]?.ToString(),
                        Banos = reader["Banos"]?.ToString(),
                        M2_Construidos = reader["Metros_Construidos"]?.ToString(), // SP devuelve "Metros_Construidos"
                        M2_Terreno = reader["Metros_Terreno"]?.ToString(), // SP devuelve "Metros_Terreno"
                        Estado = reader["Estado_Propiedad"]?.ToString(), // SP devuelve "Estado_Propiedad"
                        Fecha_Publicacion = reader["Fecha_Publicacion"]?.ToString(),
                        Agente_Responsable = reader["Agente_Responsable"]?.ToString(),
                        Telefono_Contacto = reader["Telefono_Contacto"]?.ToString(),
                        Email_Contacto = reader["Email_Contacto"]?.ToString(),
                        Visitas_Totales = reader["Visitas_Totales"]?.ToString(),
                        Url_Imagen = reader["Url_Imagen"]?.ToString(),
                        id_TocToc = reader["id_TocToc"]?.ToString(),
                        id_ChilePropiedades = reader["id_ChilePropiedades"]?.ToString(),
                        id_PortalInmobiliario = reader["id_PortalInmobiliario"]?.ToString(),
                        id_Proppit = reader["id_Proppit"]?.ToString(),
                        id_PortalRosch = reader["id_PortalRosch"]?.ToString(),
                        TotalRowCount = reader["TotalRowCount"] != DBNull.Value ? Convert.ToInt32(reader["TotalRowCount"]) : 0
                    };
                    propiedades.Add(propiedad);
                }

                if (propiedades.Any())
                {
                    totalRecords = propiedades.First().TotalRowCount;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedades");
                throw;
            }

            return (propiedades, totalRecords);
        }

        /// <summary>
        /// Obtener propiedad por ID
        /// </summary>
        public async Task<Propiedad?> ObtenerPorIdAsync(string idPropiedad)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_Propiedad_SelectxPK", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@IDPropiedad", idPropiedad);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Propiedad
                    {
                        ID_Propiedad = reader["ID_Propiedad"]?.ToString(),
                        Codigo_Referencia = reader["Codigo_Referencia"]?.ToString(),
                        Title = reader["Titulo"]?.ToString(), // SelectxPK devuelve "Titulo"
                        Description = reader["Descripcion"]?.ToString(),
                        Tipo_elemento = reader["Tipo_Propiedad"]?.ToString(), // SelectxPK devuelve "Tipo_Propiedad"
                        Direccion = reader["Direccion"]?.ToString(),
                        Comuna = reader["Comuna"]?.ToString(),
                        Ciudad = reader["Ciudad"]?.ToString(),
                        Region = reader["Region"]?.ToString(),
                        Valor = reader["Precio"]?.ToString(), // SelectxPK devuelve "Precio"
                        Precio_UF = reader["Precio_UF"]?.ToString(),
                        Dormitorios = reader["Dormitorios"]?.ToString(),
                        Banos = reader["Banos"]?.ToString(),
                        M2_Construidos = reader["Metros_Construidos"]?.ToString(), // SelectxPK devuelve "Metros_Construidos"
                        M2_Terreno = reader["Metros_Terreno"]?.ToString(), // SelectxPK devuelve "Metros_Terreno"
                        Estado = reader["Estado_Propiedad"]?.ToString(), // SelectxPK devuelve "Estado_Propiedad"
                        Fecha_Publicacion = reader["Fecha_Publicacion"]?.ToString(),
                        Agente_Responsable = reader["Agente_Responsable"]?.ToString(),
                        Telefono_Contacto = reader["Telefono_Contacto"]?.ToString(),
                        Email_Contacto = reader["Email_Contacto"]?.ToString(),
                        Visitas_Totales = reader["Visitas_Totales"]?.ToString(),
                        Url_Imagen = reader["Url_Imagen"]?.ToString(),
                        id_TocToc = reader["id_TocToc"]?.ToString(),
                        id_ChilePropiedades = reader["id_ChilePropiedades"]?.ToString(),
                        id_PortalInmobiliario = reader["id_PortalInmobiliario"]?.ToString(),
                        id_Proppit = reader["id_Proppit"]?.ToString(),
                        id_PortalRosch = reader["id_PortalRosch"]?.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedad por ID: {IdPropiedad}", idPropiedad);
                throw;
            }

            return null;
        }

        /// <summary>
        /// Actualizar propiedad
        /// </summary>
        public async Task ActualizarAsync(Propiedad propiedad)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_Propiedad_ModificaxId", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 60;

                command.Parameters.AddWithValue("@ID_Propiedad", propiedad.ID_Propiedad ?? "");
                command.Parameters.AddWithValue("@Title", propiedad.Title ?? "");
                command.Parameters.AddWithValue("@Direccion", propiedad.Direccion ?? "");
                command.Parameters.AddWithValue("@Comuna", propiedad.Comuna ?? "");
                command.Parameters.AddWithValue("@Region", propiedad.Region ?? "");
                command.Parameters.AddWithValue("@Valor", propiedad.Valor ?? "");
                command.Parameters.AddWithValue("@Estado", propiedad.Estado ?? "");
                command.Parameters.AddWithValue("@Dormitorios", propiedad.Dormitorios ?? "");
                command.Parameters.AddWithValue("@Banos", propiedad.Banos ?? "");
                command.Parameters.AddWithValue("@M2_Construidos", propiedad.M2_Construidos ?? "");
                command.Parameters.AddWithValue("@M2_Terreno", propiedad.M2_Terreno ?? "");
                command.Parameters.AddWithValue("@Tipo_elemento", propiedad.Tipo_elemento ?? "");
                command.Parameters.AddWithValue("@Agente_Responsable", propiedad.Agente_Responsable ?? "");

                await command.ExecuteNonQueryAsync();
                _logger.LogInformation("Propiedad actualizada: {IdPropiedad}", propiedad.ID_Propiedad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar propiedad: {IdPropiedad}", propiedad.ID_Propiedad);
                throw;
            }
        }

        /// <summary>
        /// Agregar nueva propiedad
        /// </summary>
        public async Task AgregarAsync(Propiedad propiedad)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_Propiedad_AgregaxId", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = 60;

                // Generar ID si no existe
                if (string.IsNullOrWhiteSpace(propiedad.ID_Propiedad))
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var random = new Random().Next(100, 999);
                    propiedad.ID_Propiedad = $"PROP-{timestamp}-{random}";
                }

                command.Parameters.AddWithValue("@ID_Propiedad", propiedad.ID_Propiedad);
                command.Parameters.AddWithValue("@Title", propiedad.Title ?? "");
                command.Parameters.AddWithValue("@Direccion", propiedad.Direccion ?? "");
                command.Parameters.AddWithValue("@Comuna", propiedad.Comuna ?? "");
                command.Parameters.AddWithValue("@Region", propiedad.Region ?? "");
                command.Parameters.AddWithValue("@Valor", propiedad.Valor ?? "");
                command.Parameters.AddWithValue("@Estado", propiedad.Estado ?? "");
                command.Parameters.AddWithValue("@Dormitorios", propiedad.Dormitorios ?? "");
                command.Parameters.AddWithValue("@Banos", propiedad.Banos ?? "");
                command.Parameters.AddWithValue("@M2_Construidos", propiedad.M2_Construidos ?? "");
                command.Parameters.AddWithValue("@M2_Terreno", propiedad.M2_Terreno ?? "");
                command.Parameters.AddWithValue("@Tipo_elemento", propiedad.Tipo_elemento ?? "");
                command.Parameters.AddWithValue("@Agente_Responsable", propiedad.Agente_Responsable ?? "");

                await command.ExecuteNonQueryAsync();
                _logger.LogInformation("Propiedad agregada: {IdPropiedad}", propiedad.ID_Propiedad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar propiedad");
                throw;
            }
        }

        /// <summary>
        /// Eliminar propiedad
        /// </summary>
        public async Task EliminarAsync(string idPropiedad)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_Propiedad_EliminarxId", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID_Propiedad", idPropiedad);

                await command.ExecuteNonQueryAsync();
                _logger.LogInformation("Propiedad eliminada: {IdPropiedad}", idPropiedad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar propiedad: {IdPropiedad}", idPropiedad);
                throw;
            }
        }

        /// <summary>
        /// Obtener estadísticas de propiedades
        /// </summary>
        public async Task<EstadisticasPropiedad> ObtenerEstadisticasAsync()
        {
            var estadisticas = new EstadisticasPropiedad();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Consulta directa para obtener estadísticas
                var query = @"
                    SELECT 
                        COUNT(*) AS Total,
                        SUM(CASE WHEN LOWER(Estado) LIKE '%disponible%' THEN 1 ELSE 0 END) AS Disponible,
                        SUM(CASE WHEN LOWER(Estado) LIKE '%vendida%' THEN 1 ELSE 0 END) AS Vendida,
                        SUM(CASE WHEN LOWER(Estado) LIKE '%reservada%' THEN 1 ELSE 0 END) AS Reservada,
                        SUM(CASE WHEN LOWER(Estado) LIKE '%arrendada%' THEN 1 ELSE 0 END) AS Arrendada,
                        SUM(CASE WHEN LOWER(Estado) LIKE '%suspendida%' THEN 1 ELSE 0 END) AS Suspendida
                    FROM Propiedades WITH (NOLOCK)";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    estadisticas.Total = reader["Total"] != DBNull.Value ? Convert.ToInt32(reader["Total"]) : 0;
                    estadisticas.Disponible = reader["Disponible"] != DBNull.Value ? Convert.ToInt32(reader["Disponible"]) : 0;
                    estadisticas.Vendida = reader["Vendida"] != DBNull.Value ? Convert.ToInt32(reader["Vendida"]) : 0;
                    estadisticas.Reservada = reader["Reservada"] != DBNull.Value ? Convert.ToInt32(reader["Reservada"]) : 0;
                    estadisticas.Arrendada = reader["Arrendada"] != DBNull.Value ? Convert.ToInt32(reader["Arrendada"]) : 0;
                    estadisticas.Suspendida = reader["Suspendida"] != DBNull.Value ? Convert.ToInt32(reader["Suspendida"]) : 0;
                    estadisticas.Otros = estadisticas.Total - (estadisticas.Disponible + estadisticas.Vendida + 
                                                              estadisticas.Reservada + estadisticas.Arrendada + 
                                                              estadisticas.Suspendida);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
            }

            return estadisticas;
        }

        /// <summary>
        /// Obtener tipos de propiedad
        /// </summary>
        public async Task<List<string>> ObtenerTiposPropiedadAsync()
        {
            var tipos = new List<string>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_Propiedad_ObtenerTipos", connection);
                command.CommandType = CommandType.StoredProcedure;

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tipos.Add(reader["Tipo"]?.ToString() ?? "");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipos de propiedad");
                throw;
            }

            return tipos;
        }

        /// <summary>
        /// Obtener estados de propiedad
        /// </summary>
        public async Task<List<string>> ObtenerEstadosAsync()
        {
            var estados = new List<string>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_Propiedad_ObtenerEstados", connection);
                command.CommandType = CommandType.StoredProcedure;

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    estados.Add(reader["Estado"]?.ToString() ?? "");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estados");
                throw;
            }

            return estados;
        }

        /// <summary>
        /// Obtener agentes responsables
        /// </summary>
        public async Task<List<string>> ObtenerAgentesAsync()
        {
            var agentes = new List<string>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_Propiedad_ObtenerAgentes", connection);
                command.CommandType = CommandType.StoredProcedure;

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    agentes.Add(reader["Agente"]?.ToString() ?? "");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agentes");
                throw;
            }

            return agentes;
        }
    }
}

