using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria.Net8.DTOs;
using Inmobiliaria.Net8.Helpers;

namespace Inmobiliaria.Net8.Services
{
    public class ConfiguracionImagenesService
    {
        private readonly string _connectionString;
        private readonly ILogger<ConfiguracionImagenesService> _logger;

        public ConfiguracionImagenesService(IConfiguration configuration, ILogger<ConfiguracionImagenesService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        public Task<(List<PropiedadImagen> propiedades, int total)> ObtenerPropiedadesConImagenesAsync(string? termino = null)
            => ObtenerPropiedadesPaginadasAsync(termino, 1, int.MaxValue / 2);

        public async Task<(List<PropiedadImagen> propiedades, int total)> ObtenerPropiedadesPaginadasAsync(
            string? termino,
            int pagina,
            int tamanoPagina)
        {
            var propiedades = new List<PropiedadImagen>();
            var total = 0;
            pagina = Math.Max(1, pagina);
            tamanoPagina = Math.Clamp(tamanoPagina, 1, 100);
            var offset = (pagina - 1) * tamanoPagina;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                    SELECT 
                        ID_Propiedad, 
                        Title, 
                        Direccion, 
                        Comuna, 
                        Valor, 
                        Tipo_elemento,
                        Url_Imagen,
                        COUNT(*) OVER() AS TotalRowCount
                    FROM Propiedades 
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(termino))
                {
                    sql += @" AND (
                        ID_Propiedad LIKE @termino OR 
                        Title LIKE @termino OR 
                        Direccion LIKE @termino OR 
                        Comuna LIKE @termino
                    )";
                }

                sql += " ORDER BY ID_Propiedad OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using var command = new SqlCommand(sql, connection);
                command.CommandTimeout = SqlDefaults.CommandTimeoutSeconds;
                command.Parameters.AddWithValue("@Offset", offset);
                command.Parameters.AddWithValue("@PageSize", tamanoPagina);

                if (!string.IsNullOrEmpty(termino))
                {
                    command.Parameters.AddWithValue("@termino", "%" + termino + "%");
                }

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (total == 0 && reader["TotalRowCount"] != DBNull.Value)
                    {
                        total = Convert.ToInt32(reader["TotalRowCount"]);
                    }

                    propiedades.Add(new PropiedadImagen
                    {
                        ID_Propiedad = reader["ID_Propiedad"].ToString() ?? string.Empty,
                        Title = reader["Title"].ToString() ?? string.Empty,
                        Direccion = reader["Direccion"].ToString() ?? string.Empty,
                        Comuna = reader["Comuna"].ToString() ?? string.Empty,
                        Valor = reader["Valor"] != DBNull.Value ? Convert.ToDecimal(reader["Valor"]) : 0,
                        Tipo_elemento = reader["Tipo_elemento"].ToString() ?? string.Empty,
                        Url_Imagen = reader["Url_Imagen"]?.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedades con imágenes");
                throw;
            }

            return (propiedades, total);
        }

        public async Task<bool> GuardarUrlImagenAsync(string idPropiedad, string urlImagen)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    UPDATE Propiedades 
                    SET Url_Imagen = @Url_Imagen 
                    WHERE ID_Propiedad = @ID_Propiedad";

                using var command = new SqlCommand(sql, connection);
                command.CommandTimeout = SqlDefaults.CommandTimeoutSeconds;
                command.Parameters.AddWithValue("@ID_Propiedad", idPropiedad);
                command.Parameters.AddWithValue("@Url_Imagen", urlImagen ?? (object)DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar URL de imagen");
                throw;
            }
        }

        public async Task<bool> EliminarUrlImagenAsync(string idPropiedad)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    UPDATE Propiedades 
                    SET Url_Imagen = NULL 
                    WHERE ID_Propiedad = @ID_Propiedad";

                using var command = new SqlCommand(sql, connection);
                command.CommandTimeout = SqlDefaults.CommandTimeoutSeconds;
                command.Parameters.AddWithValue("@ID_Propiedad", idPropiedad);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar URL de imagen");
                throw;
            }
        }

        public async Task<EstadisticasImagenes> ObtenerEstadisticasAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    SELECT
                        COUNT(*) AS TotalPropiedades,
                        SUM(CASE WHEN Url_Imagen IS NOT NULL AND LTRIM(RTRIM(Url_Imagen)) <> '' THEN 1 ELSE 0 END) AS ConImagen,
                        SUM(CASE WHEN Url_Imagen IS NULL OR LTRIM(RTRIM(Url_Imagen)) = '' THEN 1 ELSE 0 END) AS SinImagen
                    FROM Propiedades";

                using var command = new SqlCommand(sql, connection);
                command.CommandTimeout = SqlDefaults.CommandTimeoutSeconds;
                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return new EstadisticasImagenes();
                }

                var total = Convert.ToInt32(reader["TotalPropiedades"]);
                var conImagen = Convert.ToInt32(reader["ConImagen"]);

                return new EstadisticasImagenes
                {
                    TotalPropiedades = total,
                    ConImagen = conImagen,
                    SinImagen = Convert.ToInt32(reader["SinImagen"]),
                    PorcentajeCobertura = total > 0
                        ? Math.Round((double)conImagen / total * 100, 2)
                        : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                throw;
            }
        }

        public string ConvertirUrlGoogleDrive(string url)
            => GoogleDriveHelper.ConvertirUrlThumbnail(url) ?? string.Empty;
    }
}
