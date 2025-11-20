using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria.Net8.DTOs;

namespace Inmobiliaria.Net8.Services
{
    public class ClientesMatchService
    {
        private readonly string _connectionString;
        private readonly ILogger<ClientesMatchService> _logger;

        public ClientesMatchService(IConfiguration configuration, ILogger<ClientesMatchService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        // LISTAR todos los Clientes Match con filtros
        public async Task<List<ClienteMatch>> ObtenerTodosAsync(FiltroClientesMatch filtro)
        {
            var lista = new List<ClienteMatch>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_ClientesMatch_SelectAll", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Parámetros del SP
                command.Parameters.AddWithValue("@ID_Interno", filtro.ID_Interno ?? string.Empty);
                command.Parameters.AddWithValue("@Nombre", filtro.Nombre ?? string.Empty);
                command.Parameters.AddWithValue("@Telefono", filtro.Telefono ?? string.Empty);
                command.Parameters.AddWithValue("@Correo", filtro.Correo ?? string.Empty);
                command.Parameters.AddWithValue("@Tipo_Match", filtro.Tipo_Match ?? string.Empty);
                command.Parameters.AddWithValue("@Columna", filtro.ColumnaOrden);
                command.Parameters.AddWithValue("@Direccion", filtro.DireccionOrden);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    lista.Add(new ClienteMatch
                    {
                        ID_Interno = reader["ID_Interno"]?.ToString() ?? string.Empty,
                        Tipo_Match = reader["Tipo_Match"]?.ToString(),
                        Nombre = reader["Nombre"]?.ToString(),
                        Rut = reader["Rut"]?.ToString(),
                        Datos_adjuntos = reader["Datos_adjuntos"]?.ToString(),
                        Direccion = reader["Direccion"]?.ToString(),
                        Comuna = reader["Comuna"]?.ToString(),
                        Estado_Civil = reader["Estado_Civil"]?.ToString(),
                        Profesion = reader["Profesion"]?.ToString(),
                        Telefono = reader["Telefono"]?.ToString(),
                        Correo = reader["Correo"]?.ToString(),
                        Giro_Razon_Social = reader["Giro_Razon_Social"]?.ToString()
                    });
                }

                _logger.LogInformation("Se obtuvieron {Count} clientes match", lista.Count);
                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes match");
                throw new Exception($"Error al obtener clientes match: {ex.Message}", ex);
            }
        }

        // OBTENER por ID
        public async Task<ClienteMatch?> ObtenerPorIdAsync(string idInterno)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_ClientesMatch_SelectxPK", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID_Interno", idInterno);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new ClienteMatch
                    {
                        ID_Interno = reader["ID_Interno"]?.ToString() ?? string.Empty,
                        Tipo_Match = reader["Tipo_Match"]?.ToString(),
                        Nombre = reader["Nombre"]?.ToString(),
                        Rut = reader["Rut"]?.ToString(),
                        Datos_adjuntos = reader["Datos_adjuntos"]?.ToString(),
                        Direccion = reader["Direccion"]?.ToString(),
                        Comuna = reader["Comuna"]?.ToString(),
                        Estado_Civil = reader["Estado_Civil"]?.ToString(),
                        Profesion = reader["Profesion"]?.ToString(),
                        Telefono = reader["Telefono"]?.ToString(),
                        Correo = reader["Correo"]?.ToString(),
                        Giro_Razon_Social = reader["Giro_Razon_Social"]?.ToString()
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener cliente match por ID: {IdInterno}", idInterno);
                throw new Exception($"Error al obtener cliente match: {ex.Message}", ex);
            }
        }

        // NOTA: No se implementa CREATE porque los Clientes Match se crean 
        // automáticamente al convertir un Lead desde ClientesLeads usando SP_ConvertirLeadAClienteMatch

        // ACTUALIZAR Cliente Match
        public async Task<bool> ActualizarAsync(ClienteMatch cliente)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_ClientesMatch_Update", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Parámetros del SP
                command.Parameters.AddWithValue("@ID_Interno", cliente.ID_Interno);
                command.Parameters.AddWithValue("@Tipo_Match", cliente.Tipo_Match ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Nombre", cliente.Nombre ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Rut", cliente.Rut ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Datos_adjuntos", cliente.Datos_adjuntos ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Direccion", cliente.Direccion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Comuna", cliente.Comuna ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Estado_Civil", cliente.Estado_Civil ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Profesion", cliente.Profesion ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Telefono", cliente.Telefono ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Correo", cliente.Correo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Giro_Razon_Social", cliente.Giro_Razon_Social ?? (object)DBNull.Value);

                await command.ExecuteNonQueryAsync();

                _logger.LogInformation("Cliente Match actualizado: {IdInterno}", cliente.ID_Interno);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cliente match: {IdInterno}", cliente.ID_Interno);
                throw new Exception($"Error al actualizar cliente match: {ex.Message}", ex);
            }
        }

        // ELIMINAR Cliente Match
        public async Task<bool> EliminarAsync(string idInterno)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_ClientesMatch_Delete", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID_Interno", idInterno);

                await command.ExecuteNonQueryAsync();

                _logger.LogInformation("Cliente Match eliminado: {IdInterno}", idInterno);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cliente match: {IdInterno}", idInterno);
                throw new Exception($"Error al eliminar cliente match: {ex.Message}", ex);
            }
        }

        // CONTAR total de Clientes Match
        public async Task<int> ContarTotalAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SELECT COUNT(*) FROM Clientes_Match", connection);
                var result = await command.ExecuteScalarAsync();

                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar clientes match");
                throw new Exception($"Error al contar clientes match: {ex.Message}", ex);
            }
        }
    }
}

