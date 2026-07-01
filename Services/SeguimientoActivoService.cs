using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria.Net8.DTOs;

namespace Inmobiliaria.Net8.Services
{
    public class SeguimientoActivoService
    {
        private readonly string _connectionString;
        private readonly ILogger<SeguimientoActivoService> _logger;

        public SeguimientoActivoService(IConfiguration configuration, ILogger<SeguimientoActivoService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        // LISTAR con paginación y filtros
        public async Task<(List<SeguimientoActivo> lista, int total)> ObtenerTodosAsync(FiltroSeguimientoActivo filtro)
        {
            var lista = new List<SeguimientoActivo>();
            int totalRegistros = 0;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_Seguimiento_Activo_Select", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Calcular Min y Max para paginación
                int min = ((filtro.PageNumber - 1) * filtro.PageSize) + 1;
                int max = filtro.PageNumber * filtro.PageSize;

                // Parámetros del SP
                command.Parameters.AddWithValue("@Agente", (object?)filtro.Agente ?? DBNull.Value);
                command.Parameters.AddWithValue("@ID_Cliente", (object?)filtro.ID_Cliente ?? DBNull.Value);
                command.Parameters.AddWithValue("@Tipo_Accion", (object?)filtro.Tipo_Accion ?? DBNull.Value);
                command.Parameters.AddWithValue("@Estado", (object?)filtro.Estado ?? DBNull.Value);
                command.Parameters.AddWithValue("@FechaDesde", (object?)filtro.FechaDesde ?? DBNull.Value);
                command.Parameters.AddWithValue("@FechaHasta", (object?)filtro.FechaHasta ?? DBNull.Value);
                command.Parameters.AddWithValue("@ID_Propiedad", (object?)filtro.ID_Propiedad ?? DBNull.Value);
                command.Parameters.AddWithValue("@Min", min);
                command.Parameters.AddWithValue("@Max", max);
                command.Parameters.AddWithValue("@Columna", filtro.ColumnaOrden);
                command.Parameters.AddWithValue("@Direccion", filtro.DireccionOrden);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var seguimiento = new SeguimientoActivo
                    {
                        ID_Cliente = reader["ID_Cliente"]?.ToString() ?? string.Empty,
                        ID_Propiedad = reader["ID_Propiedad"]?.ToString(),
                        Agente = reader["Agente"]?.ToString() ?? string.Empty,
                        Codigo_Agente = reader["Codigo_Agente"]?.ToString(),
                        Fecha_Accion = reader["Fecha_Accion"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Accion"]) : DateTime.Now,
                        Tipo_Accion = reader["Tipo_Accion"]?.ToString() ?? string.Empty,
                        Descripcion_Accion = reader["Descripcion_Accion"]?.ToString() ?? string.Empty,
                        Resultado = reader["Resultado"]?.ToString() ?? string.Empty,
                        Estado = reader["Estado"]?.ToString() ?? string.Empty,
                        Fecha_Proximo_Contacto = reader["Fecha_Proximo_Contacto"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Proximo_Contacto"]) : null,
                        TotalRowCount = reader["TotalRowCount"] != DBNull.Value ? Convert.ToInt32(reader["TotalRowCount"]) : 0
                    };

                    if (totalRegistros == 0 && seguimiento.TotalRowCount.HasValue)
                    {
                        totalRegistros = seguimiento.TotalRowCount.Value;
                    }

                    lista.Add(seguimiento);
                }

                _logger.LogInformation("Se obtuvieron {Count} seguimientos de {Total} total", lista.Count, totalRegistros);
                return (lista, totalRegistros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener seguimientos activos");
                throw new Exception($"Error al obtener seguimientos activos: {ex.Message}", ex);
            }
        }

        // OBTENER por Cliente
        public async Task<List<SeguimientoActivo>> ObtenerPorClienteAsync(string idCliente)
        {
            var lista = new List<SeguimientoActivo>();

            try
            {
                _logger.LogInformation("Obteniendo seguimientos para cliente: {IdCliente}", idCliente);
                
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_ObtenerAccionesCliente", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID_Cliente", idCliente);
                command.Parameters.AddWithValue("@TopRegistros", 50);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    lista.Add(new SeguimientoActivo
                    {
                        ID_Cliente = idCliente,
                        ID_Propiedad = reader["ID_Propiedad"]?.ToString(),
                        Agente = reader["Agente"]?.ToString() ?? string.Empty,
                        Fecha_Accion = reader["Fecha_Accion"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Accion"]) : DateTime.Now,
                        Tipo_Accion = reader["Tipo_Accion"]?.ToString() ?? string.Empty,
                        Descripcion_Accion = reader["Descripcion_Accion"]?.ToString() ?? string.Empty,
                        Resultado = reader["Resultado"]?.ToString() ?? string.Empty,
                        Estado = reader["Estado"]?.ToString() ?? string.Empty,
                        Fecha_Proximo_Contacto = reader["Fecha_Proximo_Contacto"] != DBNull.Value ? Convert.ToDateTime(reader["Fecha_Proximo_Contacto"]) : null
                    });
                }

                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener seguimientos del cliente: {IdCliente}", idCliente);
                throw new Exception($"Error al obtener seguimientos del cliente: {ex.Message}", ex);
            }
        }

        public async Task<List<string>> ObtenerClientesDistintosAsync()
        {
            var lista = new List<string>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    SELECT DISTINCT ID_Cliente
                    FROM Seguimiento_Activo
                    WHERE ID_Cliente IS NOT NULL AND LTRIM(RTRIM(ID_Cliente)) <> ''
                    ORDER BY ID_Cliente";

                using var command = new SqlCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var id = reader["ID_Cliente"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(id))
                        lista.Add(id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes distintos de seguimiento");
                throw;
            }

            return lista;
        }

        public async Task<List<string>> ObtenerAgentesDistintosAsync()
        {
            var lista = new List<string>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    SELECT DISTINCT Agente
                    FROM Seguimiento_Activo
                    WHERE Agente IS NOT NULL AND LTRIM(RTRIM(Agente)) <> ''
                    ORDER BY Agente";

                using var command = new SqlCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var agente = reader["Agente"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(agente))
                        lista.Add(agente);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener agentes distintos de seguimiento");
                throw;
            }

            return lista;
        }

        // AGREGAR nuevo seguimiento
        public async Task<bool> AgregarAsync(SeguimientoActivo seguimiento)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("PP_psnp_Seguimiento_Activo_AgregaxId", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@ID_Cliente", seguimiento.ID_Cliente);
                command.Parameters.AddWithValue("@ID_Propiedad", (object?)seguimiento.ID_Propiedad ?? DBNull.Value);
                command.Parameters.AddWithValue("@Agente", seguimiento.Agente);
                command.Parameters.AddWithValue("@Codigo_Agente", (object?)seguimiento.Codigo_Agente ?? DBNull.Value);
                command.Parameters.AddWithValue("@Fecha_Accion", seguimiento.Fecha_Accion);
                command.Parameters.AddWithValue("@Tipo_Accion", seguimiento.Tipo_Accion);
                command.Parameters.AddWithValue("@Descripcion_Accion", seguimiento.Descripcion_Accion);
                command.Parameters.AddWithValue("@Resultado", seguimiento.Resultado);
                command.Parameters.AddWithValue("@Estado", seguimiento.Estado);
                command.Parameters.AddWithValue("@Fecha_Proximo_Contacto", (object?)seguimiento.Fecha_Proximo_Contacto ?? DBNull.Value);

                await command.ExecuteNonQueryAsync();

                _logger.LogInformation("Seguimiento agregado para cliente: {IdCliente}", seguimiento.ID_Cliente);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar seguimiento para cliente: {IdCliente}", seguimiento.ID_Cliente);
                throw new Exception($"Error al agregar seguimiento: {ex.Message}", ex);
            }
        }
    }
}

