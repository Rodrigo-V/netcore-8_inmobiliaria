using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria.Net8.DTOs;

namespace Inmobiliaria.Net8.Services
{
    public class ConfiguracionPortalesService
    {
        private readonly string _connectionString;
        private readonly ILogger<ConfiguracionPortalesService> _logger;

        public ConfiguracionPortalesService(IConfiguration configuration, ILogger<ConfiguracionPortalesService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        public async Task<List<PropiedadPortalDTO>> ObtenerPropiedadesAsync(FiltroPropiedadDTO filtro)
        {
            var propiedades = new List<PropiedadPortalDTO>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Construir query base
                    var query = @"
                        SELECT 
                            p.ID_Propiedad,
                            -- p.Codigo_Referencia,
                            p.Title as Titulo,
                            p.Direccion,
                            p.Comuna,
                            p.Estado as Estado_Propiedad,
                            p.Url_Imagen,
                            -- p.Imagen_Propiedad,
                            p.id_TocToc,
                            p.id_ChilePropiedades,
                            p.id_PortalInmobiliario,
                            p.id_Proppit,
                            p.id_PortalRosch,
                            COUNT(*) OVER() as TotalRowCount
                        FROM Propiedades p
                        WHERE 1=1";

                    // Filtros
                    if (!string.IsNullOrEmpty(filtro.IDPropiedad))
                    {
                        query += " AND (p.ID_Propiedad LIKE @Filtro OR p.Title LIKE @Filtro OR p.Comuna LIKE @Filtro OR p.id_TocToc LIKE @Filtro OR p.id_ChilePropiedades LIKE @Filtro OR p.id_PortalInmobiliario LIKE @Filtro)";
                    }

                    // Ordenamiento
                    query += $" ORDER BY {filtro.Columna} {filtro.Direccion}";

                    // Paginación
                    if (filtro.PageSize > 0)
                    {
                        query += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                    }

                    using (var command = new SqlCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(filtro.IDPropiedad))
                        {
                            command.Parameters.AddWithValue("@Filtro", $"%{filtro.IDPropiedad}%");
                        }

                        if (filtro.PageSize > 0)
                        {
                            command.Parameters.AddWithValue("@Offset", filtro.Min - 1);
                            command.Parameters.AddWithValue("@PageSize", filtro.PageSize);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                propiedades.Add(new PropiedadPortalDTO
                                {
                                    ID_Propiedad = reader["ID_Propiedad"]?.ToString() ?? "",
                                    Codigo_Referencia = "", // reader["Codigo_Referencia"]?.ToString() ?? "",
                                    Titulo = reader["Titulo"]?.ToString() ?? "",
                                    Direccion = reader["Direccion"]?.ToString() ?? "",
                                    Comuna = reader["Comuna"]?.ToString() ?? "",
                                    Estado_Propiedad = reader["Estado_Propiedad"]?.ToString() ?? "",
                                    Url_Imagen = reader["Url_Imagen"]?.ToString(),
                                    Imagen_Propiedad = null, // reader["Imagen_Propiedad"]?.ToString(),
                                    id_TocToc = reader["id_TocToc"]?.ToString(),
                                    id_ChilePropiedades = reader["id_ChilePropiedades"]?.ToString(),
                                    id_PortalInmobiliario = reader["id_PortalInmobiliario"]?.ToString(),
                                    id_Proppit = reader["id_Proppit"]?.ToString(),
                                    id_PortalRosch = reader["id_PortalRosch"]?.ToString(),
                                    TotalRowCount = Convert.ToInt32(reader["TotalRowCount"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedades para configuración de portales");
                throw;
            }

            return propiedades;
        }

        public async Task<PortalIDsDTO?> ObtenerIDsPortalesAsync(string idPropiedad)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        SELECT 
                            ID_Propiedad,
                            id_TocToc,
                            id_ChilePropiedades,
                            id_PortalInmobiliario,
                            id_Proppit,
                            id_PortalRosch
                        FROM Propiedades 
                        WHERE ID_Propiedad = @ID_Propiedad";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID_Propiedad", idPropiedad);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new PortalIDsDTO
                                {
                                    ID_Propiedad = reader["ID_Propiedad"]?.ToString() ?? "",
                                    id_TocToc = reader["id_TocToc"]?.ToString(),
                                    id_ChilePropiedades = reader["id_ChilePropiedades"]?.ToString(),
                                    id_PortalInmobiliario = reader["id_PortalInmobiliario"]?.ToString(),
                                    id_Proppit = reader["id_Proppit"]?.ToString(),
                                    id_PortalRosch = reader["id_PortalRosch"]?.ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener IDs de portales para propiedad {IdPropiedad}", idPropiedad);
                throw;
            }

            return null;
        }

        public async Task<(bool Success, string Message)> GuardarIDsPortalesAsync(PortalIDsDTO ids)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("PP_psnp_Propiedad_ActualizarIDsPortales", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@ID_Propiedad", ids.ID_Propiedad);
                        command.Parameters.AddWithValue("@id_TocToc", string.IsNullOrEmpty(ids.id_TocToc) ? (object)DBNull.Value : ids.id_TocToc);
                        command.Parameters.AddWithValue("@id_ChilePropiedades", string.IsNullOrEmpty(ids.id_ChilePropiedades) ? (object)DBNull.Value : ids.id_ChilePropiedades);
                        command.Parameters.AddWithValue("@id_PortalInmobiliario", string.IsNullOrEmpty(ids.id_PortalInmobiliario) ? (object)DBNull.Value : ids.id_PortalInmobiliario);
                        command.Parameters.AddWithValue("@id_Proppit", string.IsNullOrEmpty(ids.id_Proppit) ? (object)DBNull.Value : ids.id_Proppit);
                        command.Parameters.AddWithValue("@id_PortalRosch", string.IsNullOrEmpty(ids.id_PortalRosch) ? (object)DBNull.Value : ids.id_PortalRosch);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var resultado = reader["Resultado"]?.ToString();
                                var mensaje = reader["Mensaje"]?.ToString() ?? "Operación completada";

                                return (resultado == "SUCCESS", mensaje);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2812) // SP not found
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
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var query = @"
                        UPDATE Propiedades 
                        SET 
                            id_TocToc = @id_TocToc,
                            id_ChilePropiedades = @id_ChilePropiedades,
                            id_PortalInmobiliario = @id_PortalInmobiliario,
                            id_Proppit = @id_Proppit,
                            id_PortalRosch = @id_PortalRosch
                        WHERE ID_Propiedad = @ID_Propiedad";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ID_Propiedad", ids.ID_Propiedad);
                        command.Parameters.AddWithValue("@id_TocToc", string.IsNullOrEmpty(ids.id_TocToc) ? (object)DBNull.Value : ids.id_TocToc);
                        command.Parameters.AddWithValue("@id_ChilePropiedades", string.IsNullOrEmpty(ids.id_ChilePropiedades) ? (object)DBNull.Value : ids.id_ChilePropiedades);
                        command.Parameters.AddWithValue("@id_PortalInmobiliario", string.IsNullOrEmpty(ids.id_PortalInmobiliario) ? (object)DBNull.Value : ids.id_PortalInmobiliario);
                        command.Parameters.AddWithValue("@id_Proppit", string.IsNullOrEmpty(ids.id_Proppit) ? (object)DBNull.Value : ids.id_Proppit);
                        command.Parameters.AddWithValue("@id_PortalRosch", string.IsNullOrEmpty(ids.id_PortalRosch) ? (object)DBNull.Value : ids.id_PortalRosch);

                        var rowsAffected = await command.ExecuteNonQueryAsync();
                        
                        if (rowsAffected > 0)
                        {
                            return (true, "IDs actualizados correctamente (Fallback)");
                        }
                        else
                        {
                            return (false, "No se encontró la propiedad para actualizar");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en fallback de guardar IDs");
                return (false, $"Error en fallback: {ex.Message}");
            }
        }
    }
}
