using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria.Net8.DTOs;
using Inmobiliaria.Net8.Helpers;

namespace Inmobiliaria.Net8.Services
{
    public class ClientesLeadsService
    {
        private readonly string _connectionString;
        private readonly ILogger<ClientesLeadsService> _logger;

        public ClientesLeadsService(IConfiguration configuration, ILogger<ClientesLeadsService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        // LISTAR con paginación y filtros
        public async Task<List<ClienteLead>> ObtenerTodosAsync(FiltroClientesLeads filtro)
        {
            var lista = new List<ClienteLead>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_ClientesLeads_SelectxTipoEmpresa", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlDefaults.CommandTimeoutSeconds;

                // Parámetros del SP
                command.Parameters.AddWithValue("@ID_Cliente", filtro.ID_Cliente ?? string.Empty);
                command.Parameters.AddWithValue("@Nombres", filtro.Nombres ?? string.Empty);
                command.Parameters.AddWithValue("@Apellidos", filtro.Apellidos ?? string.Empty);
                command.Parameters.AddWithValue("@Portal", filtro.Portal ?? string.Empty);
                command.Parameters.AddWithValue("@Asistente", filtro.Asistente ?? string.Empty);
                command.Parameters.AddWithValue("@Seguimiento", filtro.Seguimiento ?? string.Empty);
                command.Parameters.AddWithValue("@Busqueda", filtro.Busqueda ?? string.Empty);
                command.Parameters.AddWithValue("@Columna", filtro.ColumnaOrden);
                command.Parameters.AddWithValue("@Direccion", filtro.DireccionOrden);

                // Calcular Min y Max para paginación
                int min = (filtro.PaginaActual - 1) * filtro.TamañoPagina + 1;
                int max = filtro.PaginaActual * filtro.TamañoPagina;

                command.Parameters.AddWithValue("@Min", min);
                command.Parameters.AddWithValue("@Max", max);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var lead = new ClienteLead
                    {
                        Creado = reader["Creado"] != DBNull.Value ? Convert.ToDateTime(reader["Creado"]) : null,
                        Fecha_Contacto = reader["Fecha_Contacto"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Contacto"]) : null,
                        Asistente = reader["Asistente"]?.ToString() ?? string.Empty,
                        ID_Cliente = reader["ID_Cliente"]?.ToString() ?? string.Empty,
                        Seguimiento = reader["Seguimiento"]?.ToString() ?? string.Empty,
                        Portal = reader["Portal"]?.ToString() ?? string.Empty,
                        Respuesta = reader["Respuesta"]?.ToString(),
                        ID_Unidad_Consultada = reader["ID_Unidad_Consultada"]?.ToString(),
                        Unidad_Consultada = reader["Unidad_Consultada"]?.ToString(),
                        Nombres = reader["Nombres"]?.ToString(),
                        Apellidos = reader["Apellidos"]?.ToString(),
                        Sexo = reader["Sexo"]?.ToString(),
                        Telefono = reader["Telefono"]?.ToString(),
                        Correo_Electronico = reader["Correo_Electronico"]?.ToString(),
                        Visita_Realizada = reader["Visita_Realizada"] != DBNull.Value ? Convert.ToBoolean(reader["Visita_Realizada"]) : null,
                        Imagen_Propiedad = LeerUrlImagenPropiedad(reader),
                        TotalRowCount = reader["TotalRowCount"] != DBNull.Value ? Convert.ToInt32(reader["TotalRowCount"]) : 0
                    };

                    lista.Add(lead);
                }

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los leads");
                throw new Exception($"Error al obtener los leads: {ex.Message}", ex);
            }
        }

        // CONTAR TOTAL
        public async Task<int> ContarTotalAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SELECT COUNT(*) FROM Clientes_Leads", connection);
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar total de leads");
                throw new Exception($"Error al contar total de leads: {ex.Message}", ex);
            }
        }

        // CONTAR TOTAL por Asistente
        public async Task<int> ContarTotalPorAsistenteAsync(string asistente)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SELECT COUNT(*) FROM Clientes_Leads WHERE Asistente = @Asistente", connection);
                command.Parameters.AddWithValue("@Asistente", asistente ?? string.Empty);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar leads por asistente");
                throw new Exception($"Error al contar leads por asistente: {ex.Message}", ex);
            }
        }

        // OBTENER por ID
        public async Task<ClienteLead?> ObtenerPorIdAsync(string idCliente)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_ClientesLeads_SelectxPK", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID_Cliente", idCliente);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var lead = new ClienteLead
                    {
                        Creado = reader["Creado"] != DBNull.Value ? Convert.ToDateTime(reader["Creado"]) : null,
                        Fecha_Contacto = reader["Fecha_Contacto"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Contacto"]) : null,
                        Asistente = reader["Asistente"]?.ToString() ?? string.Empty,
                        ID_Cliente = reader["ID_Cliente"]?.ToString() ?? string.Empty,
                        Seguimiento = reader["Seguimiento"]?.ToString() ?? string.Empty,
                        Portal = reader["Portal"]?.ToString() ?? string.Empty,
                        Respuesta = reader["Respuesta"]?.ToString(),
                        ID_Unidad_Consultada = reader["ID_Unidad_Consultada"]?.ToString(),
                        Unidad_Consultada = reader["Unidad_Consultada"]?.ToString(),
                        Nombres = reader["Nombres"]?.ToString(),
                        Apellidos = reader["Apellidos"]?.ToString(),
                        Sexo = reader["Sexo"]?.ToString(),
                        Telefono = reader["Telefono"]?.ToString(),
                        Correo_Electronico = reader["Correo_Electronico"]?.ToString(),
                        Visita_Realizada = reader["Visita_Realizada"] != DBNull.Value ? Convert.ToBoolean(reader["Visita_Realizada"]) : null
                    };
                    
                    // Cargar imagen de la propiedad si existe
                    if (!string.IsNullOrEmpty(lead.ID_Unidad_Consultada))
                    {
                        await CargarImagenesPropiedades(new List<ClienteLead> { lead }, connection);
                    }
                    
                    return lead;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lead por ID: {IdCliente}", idCliente);
                throw new Exception($"Error al obtener lead: {ex.Message}", ex);
            }
        }

        // AGREGAR nuevo
        public async Task<string> AgregarAsync(ClienteLead lead)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_ClientesLeads_AgregaxId", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@Fecha_Contacto", lead.Fecha_Contacto ?? DateTime.Now);
                command.Parameters.AddWithValue("@Asistente", lead.Asistente ?? string.Empty);
                command.Parameters.AddWithValue("@Seguimiento", lead.Seguimiento ?? string.Empty);
                command.Parameters.AddWithValue("@Respuesta", (object?)lead.Respuesta ?? DBNull.Value);
                command.Parameters.AddWithValue("@ID_Unidad_Consultada", (object?)lead.ID_Unidad_Consultada ?? DBNull.Value);
                command.Parameters.AddWithValue("@Unidad_Consultada", (object?)lead.Unidad_Consultada ?? DBNull.Value);
                command.Parameters.AddWithValue("@Nombres", (object?)lead.Nombres ?? DBNull.Value);
                command.Parameters.AddWithValue("@Apellidos", (object?)lead.Apellidos ?? DBNull.Value);
                command.Parameters.AddWithValue("@Portal", lead.Portal ?? string.Empty);
                command.Parameters.AddWithValue("@Sexo", (object?)lead.Sexo ?? DBNull.Value);
                command.Parameters.AddWithValue("@Telefono", (object?)lead.Telefono ?? DBNull.Value);
                command.Parameters.AddWithValue("@Correo_Electronico", (object?)lead.Correo_Electronico ?? DBNull.Value);
                command.Parameters.AddWithValue("@Visita_Realizada", lead.Visita_Realizada ?? false);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return reader["ID_Cliente_Generado"]?.ToString() ?? string.Empty;
                }

                throw new Exception("No se pudo obtener el ID generado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar lead");
                throw new Exception($"Error al agregar lead: {ex.Message}", ex);
            }
        }

        // MODIFICAR existente
        public async Task<bool> ModificarAsync(ClienteLead lead)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_ClientesLeads_ModificaxId", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@ID_Cliente", lead.ID_Cliente);
                command.Parameters.AddWithValue("@Fecha_Contacto", lead.Fecha_Contacto ?? DateTime.Now);
                command.Parameters.AddWithValue("@Asistente", lead.Asistente ?? string.Empty);
                command.Parameters.AddWithValue("@Seguimiento", lead.Seguimiento ?? string.Empty);
                command.Parameters.AddWithValue("@Respuesta", (object?)lead.Respuesta ?? DBNull.Value);
                command.Parameters.AddWithValue("@ID_Unidad_Consultada", (object?)lead.ID_Unidad_Consultada ?? DBNull.Value);
                command.Parameters.AddWithValue("@Unidad_Consultada", (object?)lead.Unidad_Consultada ?? DBNull.Value);
                command.Parameters.AddWithValue("@Nombres", (object?)lead.Nombres ?? DBNull.Value);
                command.Parameters.AddWithValue("@Apellidos", (object?)lead.Apellidos ?? DBNull.Value);
                command.Parameters.AddWithValue("@Portal", lead.Portal ?? string.Empty);
                command.Parameters.AddWithValue("@Sexo", (object?)lead.Sexo ?? DBNull.Value);
                command.Parameters.AddWithValue("@Telefono", (object?)lead.Telefono ?? DBNull.Value);
                command.Parameters.AddWithValue("@Correo_Electronico", (object?)lead.Correo_Electronico ?? DBNull.Value);
                command.Parameters.AddWithValue("@Visita_Realizada", (object?)lead.Visita_Realizada ?? DBNull.Value);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    int filasAfectadas = reader["FilasAfectadas"] != DBNull.Value ? Convert.ToInt32(reader["FilasAfectadas"]) : 0;
                    return filasAfectadas > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al modificar lead: {IdCliente}", lead.ID_Cliente);
                throw new Exception($"Error al modificar lead: {ex.Message}", ex);
            }
        }

        // ELIMINAR
        public async Task<bool> EliminarAsync(string idCliente)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_ClientesLeads_EliminarxId", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID_Cliente", idCliente);

                var filasAfectadas = await command.ExecuteNonQueryAsync();
                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar lead: {IdCliente}", idCliente);
                throw new Exception($"Error al eliminar lead: {ex.Message}", ex);
            }
        }

        // OBTENER PORTALES
        public async Task<List<string>> ObtenerPortalesAsync()
        {
            var lista = new List<string>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SELECT DISTINCT Portal FROM Clientes_Leads WHERE Portal IS NOT NULL ORDER BY Portal", connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var portal = reader["Portal"]?.ToString();
                    if (!string.IsNullOrEmpty(portal))
                    {
                        lista.Add(portal);
                    }
                }

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener portales");
                throw new Exception($"Error al obtener portales: {ex.Message}", ex);
            }
        }

        // OBTENER ASISTENTES
        public async Task<List<string>> ObtenerAsistentesAsync()
        {
            var lista = new List<string>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SELECT DISTINCT Asistente FROM Clientes_Leads WHERE Asistente IS NOT NULL ORDER BY Asistente", connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var asistente = reader["Asistente"]?.ToString();
                    if (!string.IsNullOrEmpty(asistente))
                    {
                        lista.Add(asistente);
                    }
                }

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asistentes");
                throw new Exception($"Error al obtener asistentes: {ex.Message}", ex);
            }
        }

        // OBTENER ESTADÍSTICAS
        public async Task<dynamic> ObtenerEstadisticasAsync(string? asistente = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                    SELECT 
                        COUNT(*) as Total,
                        SUM(CASE WHEN Seguimiento = 'Nuevo' THEN 1 ELSE 0 END) as Nuevo,
                        SUM(CASE WHEN Seguimiento = 'En Seguimiento' THEN 1 ELSE 0 END) as EnSeguimiento,
                        SUM(CASE WHEN Seguimiento = 'Con Visita Programada' THEN 1 ELSE 0 END) as ConVisitaProgramada,
                        SUM(CASE WHEN Seguimiento = 'En Espera' THEN 1 ELSE 0 END) as EnEspera,
                        SUM(CASE WHEN Seguimiento = 'Terminado' THEN 1 ELSE 0 END) as Terminado,
                        SUM(CASE WHEN Seguimiento = 'RESERVA' THEN 1 ELSE 0 END) as Reserva
                    FROM Clientes_Leads
                    WHERE (@Asistente = '' OR Asistente = @Asistente)";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@Asistente", asistente ?? string.Empty);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new
                    {
                        Total = Convert.ToInt32(reader["Total"]),
                        Nuevo = Convert.ToInt32(reader["Nuevo"]),
                        EnSeguimiento = Convert.ToInt32(reader["EnSeguimiento"]),
                        ConVisitaProgramada = Convert.ToInt32(reader["ConVisitaProgramada"]),
                        EnEspera = Convert.ToInt32(reader["EnEspera"]),
                        Terminado = Convert.ToInt32(reader["Terminado"]),
                        Reserva = Convert.ToInt32(reader["Reserva"])
                    };
                }

                return new
                {
                    Total = 0,
                    Nuevo = 0,
                    EnSeguimiento = 0,
                    ConVisitaProgramada = 0,
                    EnEspera = 0,
                    Terminado = 0,
                    Reserva = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                throw new Exception($"Error al obtener estadísticas: {ex.Message}", ex);
            }
        }

        // Convertir Lead a Cliente Match
        // NOTA: Usa el Stored Procedure SP_ConvertirLeadAClienteMatch (regla del proyecto: SOLO SPs)
        public async Task<bool> ConvertirAClienteMatchAsync(ClienteLead lead)
        {
            try
            {
                _logger.LogInformation("Iniciando conversión de Lead a Cliente Match: {IdCliente}", lead.ID_Cliente);
                
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Usar el Stored Procedure creado específicamente para esta conversión
                    using (var command = new SqlCommand("SP_ConvertirLeadAClienteMatch", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        
                        // Parámetros de entrada
                        command.Parameters.AddWithValue("@ID_Cliente", lead.ID_Cliente ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Nombres", lead.Nombres ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Apellidos", lead.Apellidos ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Correo_Electronico", lead.Correo_Electronico ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Telefono", lead.Telefono ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Sexo", lead.Sexo ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Portal", lead.Portal ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Asistente", lead.Asistente ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@ID_Unidad_Consultada", lead.ID_Unidad_Consultada ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Unidad_Consultada", lead.Unidad_Consultada ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Respuesta", lead.Respuesta ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Fecha_Contacto", lead.Fecha_Contacto ?? DateTime.Now);
                        
                        // Parámetro de salida
                        var resultadoParam = new SqlParameter("@Resultado", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(resultadoParam);
                        
                        // Ejecutar el SP
                        await command.ExecuteNonQueryAsync();
                        
                        // Obtener el resultado
                        int resultado = (int)resultadoParam.Value;
                        
                        if (resultado == 1)
                        {
                            _logger.LogInformation("Lead convertido exitosamente a Cliente Match: {IdCliente}", lead.ID_Cliente);
                            return true;
                        }
                        else if (resultado == 0)
                        {
                            _logger.LogWarning("El Cliente Match ya existe: {IdCliente}", lead.ID_Cliente);
                            return false;
                        }
                        else
                        {
                            _logger.LogError("Error al convertir Lead (código {Resultado}): {IdCliente}", resultado, lead.ID_Cliente);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir Lead a Cliente Match: {IdCliente}", lead.ID_Cliente);
                throw new Exception($"Error al convertir Lead a Cliente Match: {ex.Message}", ex);
            }
        }

        // Verificar si ya existe un Cliente Match con ese ID
        public async Task<bool> ExisteClienteMatchAsync(string idCliente)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Usar el nombre correcto de la tabla: Clientes_Match (con guion bajo)
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM Clientes_Match WHERE ID_Interno = @ID_Interno", connection))
                    {
                        command.Parameters.AddWithValue("@ID_Interno", idCliente);
                        
                        var count = (int)await command.ExecuteScalarAsync();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de Cliente Match: {IdCliente}", idCliente);
                return false;
            }
        }

        private static string? LeerUrlImagenPropiedad(SqlDataReader reader)
        {
            try
            {
                var ordinal = reader.GetOrdinal("Url_Imagen_Propiedad");
                if (reader.IsDBNull(ordinal))
                    return null;

                return GoogleDriveHelper.ConvertirUrlThumbnail(reader.GetString(ordinal));
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        // Método helper para cargar imágenes cuando el SP no incluye JOIN (ej. SelectxPK)
        private async Task CargarImagenesPropiedades(List<ClienteLead> leads, SqlConnection connectionOriginal)
        {
            try
            {
                if (leads == null || leads.Count == 0)
                    return;

                // Obtener IDs únicos de propiedades consultadas
                var idsPropiedad = leads
                    .Where(l => !string.IsNullOrEmpty(l.ID_Unidad_Consultada))
                    .Select(l => l.ID_Unidad_Consultada)
                    .Distinct()
                    .ToList();

                if (idsPropiedad.Count == 0)
                    return;

                // Crear un diccionario para mapear ID_Propiedad -> URL_Imagen
                var imagenesPorPropiedad = new Dictionary<string, string>();

                // Crear una NUEVA conexión para evitar conflictos con DataReader abierto
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Consultar las URLs de imágenes en una sola query
                    var query = $@"
                        SELECT ID_Propiedad, Url_Imagen 
                        FROM Propiedades 
                        WHERE ID_Propiedad IN ({string.Join(",", idsPropiedad.Select((_, i) => $"@id{i}"))})
                        AND Url_Imagen IS NOT NULL";

                    using (var command = new SqlCommand(query, connection))
                    {
                        for (int i = 0; i < idsPropiedad.Count; i++)
                        {
                            command.Parameters.AddWithValue($"@id{i}", idsPropiedad[i]);
                        }

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var idPropiedad = reader["ID_Propiedad"]?.ToString();
                                var urlImagen = reader["Url_Imagen"]?.ToString();

                                if (!string.IsNullOrEmpty(idPropiedad) && !string.IsNullOrEmpty(urlImagen))
                                {
                                    // Convertir la URL de Google Drive al formato de visualización
                                    imagenesPorPropiedad[idPropiedad] = GoogleDriveHelper.ConvertirUrlThumbnail(urlImagen);
                                }
                            }
                        }
                    }
                }

                // Asignar las URLs de imágenes a los leads correspondientes
                foreach (var lead in leads)
                {
                    if (!string.IsNullOrEmpty(lead.ID_Unidad_Consultada) &&
                        imagenesPorPropiedad.TryGetValue(lead.ID_Unidad_Consultada, out var urlImagen))
                    {
                        lead.Imagen_Propiedad = urlImagen;
                    }
                }

                _logger.LogInformation("Imágenes cargadas: {Count} propiedades con imagen de {Total} leads", 
                    imagenesPorPropiedad.Count, leads.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar imágenes de propiedades");
                // No lanzar excepción para no interrumpir la carga de leads
            }
        }
    }
}

