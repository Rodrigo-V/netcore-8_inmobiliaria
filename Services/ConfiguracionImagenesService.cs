using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria.Net8.DTOs;

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

        public async Task<List<PropiedadImagen>> ObtenerPropiedadesConImagenesAsync(string? termino = null)
        {
            var propiedades = new List<PropiedadImagen>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                    SELECT TOP 100 
                        ID_Propiedad, 
                        Title, 
                        Direccion, 
                        Comuna, 
                        Valor, 
                        Tipo_elemento,
                        Url_Imagen
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

                sql += " ORDER BY ID_Propiedad";

                using var command = new SqlCommand(sql, connection);
                if (!string.IsNullOrEmpty(termino))
                {
                    command.Parameters.AddWithValue("@termino", "%" + termino + "%");
                }

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
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

            return propiedades;
        }

        public async Task<bool> GuardarUrlImagenAsync(string idPropiedad, string urlImagen)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                    UPDATE Propiedades 
                    SET Url_Imagen = @Url_Imagen 
                    WHERE ID_Propiedad = @ID_Propiedad";

                using var command = new SqlCommand(sql, connection);
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

                var sql = @"
                    UPDATE Propiedades 
                    SET Url_Imagen = NULL 
                    WHERE ID_Propiedad = @ID_Propiedad";

                using var command = new SqlCommand(sql, connection);
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
                var propiedades = await ObtenerPropiedadesConImagenesAsync();

                return new EstadisticasImagenes
                {
                    TotalPropiedades = propiedades.Count,
                    ConImagen = propiedades.Count(p => !string.IsNullOrEmpty(p.Url_Imagen)),
                    SinImagen = propiedades.Count(p => string.IsNullOrEmpty(p.Url_Imagen)),
                    PorcentajeCobertura = propiedades.Count > 0
                        ? Math.Round((double)propiedades.Count(p => !string.IsNullOrEmpty(p.Url_Imagen)) / propiedades.Count * 100, 2)
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
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            // Si ya es una URL de thumbnail, devolverla tal como está
            if (url.Contains("drive.google.com/thumbnail"))
                return url;

            // Si es una URL de compartir de Google Drive
            if (url.Contains("drive.google.com/file/d/") && url.Contains("/view"))
            {
                var startIndex = url.IndexOf("file/d/") + 7;
                var endIndex = url.IndexOf("/view");

                if (startIndex >= 7 && endIndex > startIndex)
                {
                    var fileId = url.Substring(startIndex, endIndex - startIndex);
                    return $"https://drive.google.com/thumbnail?id={fileId}&sz=w800";
                }
            }

            // Si es solo el ID del archivo
            if (!url.Contains("drive.google.com") && !string.IsNullOrEmpty(url))
            {
                return $"https://drive.google.com/thumbnail?id={url}&sz=w800";
            }

            return url;
        }
    }
}

