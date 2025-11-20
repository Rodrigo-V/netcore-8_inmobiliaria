using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria.Net8.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace Inmobiliaria.Net8.Services
{
    public class GestionUsuariosService
    {
        private readonly string _connectionString;
        private readonly ILogger<GestionUsuariosService> _logger;

        public GestionUsuariosService(IConfiguration configuration, ILogger<GestionUsuariosService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        // Obtener lista de usuarios
        public async Task<List<Usuario>> ObtenerUsuariosAsync(string? rol = null, bool? activo = null, string? buscar = null)
        {
            var usuarios = new List<Usuario>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_ObtenerUsuarios", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Rol", (object?)rol ?? DBNull.Value);
                command.Parameters.AddWithValue("@Activo", (object?)activo ?? DBNull.Value);
                command.Parameters.AddWithValue("@Buscar", (object?)buscar ?? DBNull.Value);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    usuarios.Add(MapearUsuario(reader));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                throw;
            }

            return usuarios;
        }

        // Obtener usuario por ID
        public async Task<Usuario?> ObtenerUsuarioPorIdAsync(int idUsuario)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_ObtenerUsuarioPorId", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID_Usuario", idUsuario);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return MapearUsuario(reader);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario por ID: {IdUsuario}", idUsuario);
                throw;
            }

            return null;
        }

        // Crear usuario
        public async Task<Usuario?> CrearUsuarioAsync(string nombres, string apellidos, string correoElectronico, 
            string clave, string rol, string? usuarioActual)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_CrearUsuario", connection);
                command.CommandType = CommandType.StoredProcedure;
                
                command.Parameters.AddWithValue("@Nombres", nombres);
                command.Parameters.AddWithValue("@Apellidos", apellidos);
                command.Parameters.AddWithValue("@Correo_Electronico", correoElectronico);
                command.Parameters.AddWithValue("@Clave", HashPassword(clave));
                command.Parameters.AddWithValue("@Rol", rol);
                command.Parameters.AddWithValue("@Activo", true);
                command.Parameters.AddWithValue("@Creado_Por", (object?)usuarioActual ?? DBNull.Value);

                var idUsuarioParam = new SqlParameter("@ID_Usuario", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(idUsuarioParam);

                await command.ExecuteNonQueryAsync();

                var idUsuario = (int)idUsuarioParam.Value;
                return await ObtenerUsuarioPorIdAsync(idUsuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                throw;
            }
        }

        // Actualizar usuario
        public async Task<Usuario?> ActualizarUsuarioAsync(int idUsuario, string? nombres, string? apellidos, 
            string? correoElectronico, string? rol, bool? activo, string? usuarioActual, 
            bool cambiarClave = false, string? nuevaClave = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_ActualizarUsuario", connection);
                command.CommandType = CommandType.StoredProcedure;
                
                command.Parameters.AddWithValue("@ID_Usuario", idUsuario);
                command.Parameters.AddWithValue("@Nombres", (object?)nombres ?? DBNull.Value);
                command.Parameters.AddWithValue("@Apellidos", (object?)apellidos ?? DBNull.Value);
                command.Parameters.AddWithValue("@Correo_Electronico", (object?)correoElectronico ?? DBNull.Value);
                command.Parameters.AddWithValue("@Rol", (object?)rol ?? DBNull.Value);
                command.Parameters.AddWithValue("@Activo", (object?)activo ?? DBNull.Value);
                command.Parameters.AddWithValue("@Modificado_Por", (object?)usuarioActual ?? DBNull.Value);

                if (cambiarClave && !string.IsNullOrEmpty(nuevaClave))
                {
                    command.Parameters.AddWithValue("@Clave", HashPassword(nuevaClave));
                }
                else
                {
                    command.Parameters.AddWithValue("@Clave", DBNull.Value);
                }

                await command.ExecuteNonQueryAsync();

                return await ObtenerUsuarioPorIdAsync(idUsuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
                throw;
            }
        }

        // Eliminar usuario (lógico)
        public async Task<bool> EliminarUsuarioAsync(int idUsuario, string? usuarioActual)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_EliminarUsuario", connection);
                command.CommandType = CommandType.StoredProcedure;
                
                command.Parameters.AddWithValue("@ID_Usuario", idUsuario);
                command.Parameters.AddWithValue("@Eliminado_Por", (object?)usuarioActual ?? DBNull.Value);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
                throw;
            }
        }

        // Eliminar usuario físicamente
        public async Task<bool> EliminarUsuarioFisicoAsync(int idUsuario)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_EliminarUsuarioFisico", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID_Usuario", idUsuario);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario físicamente");
                throw;
            }
        }

        // Cambiar clave
        public async Task<bool> CambiarClaveUsuarioAsync(int idUsuario, string nuevaClave, string? usuarioActual)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_CambiarClaveUsuario", connection);
                command.CommandType = CommandType.StoredProcedure;
                
                command.Parameters.AddWithValue("@ID_Usuario", idUsuario);
                command.Parameters.AddWithValue("@Nueva_Clave", HashPassword(nuevaClave));
                command.Parameters.AddWithValue("@Modificado_Por", (object?)usuarioActual ?? DBNull.Value);

                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar clave");
                throw;
            }
        }

        // Obtener estadísticas
        public async Task<EstadisticasUsuarios> ObtenerEstadisticasAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_ObtenerEstadisticasUsuarios", connection);
                command.CommandType = CommandType.StoredProcedure;

                var estadisticas = new EstadisticasUsuarios();

                using var reader = await command.ExecuteReaderAsync();
                
                // Primera tabla: Estadísticas generales
                if (await reader.ReadAsync())
                {
                    estadisticas.TotalUsuarios = reader.GetInt32(0);
                    estadisticas.UsuariosActivos = reader.GetInt32(1);
                    estadisticas.UsuariosInactivos = reader.GetInt32(2);
                    estadisticas.TotalRoles = reader.GetInt32(3);
                    estadisticas.UsuariosActivosUltimaSemana = reader.GetInt32(4);
                    estadisticas.UsuariosActivosUltimoMes = reader.GetInt32(5);
                }

                // Segunda tabla: Estadísticas por rol
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        estadisticas.EstadisticasPorRol.Add(new EstadisticaRol
                        {
                            Rol = reader.GetString(0),
                            Cantidad = reader.GetInt32(1),
                            Activos = reader.GetInt32(2),
                            Inactivos = reader.GetInt32(3)
                        });
                    }
                }

                return estadisticas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                throw;
            }
        }

        // Verificar si correo existe
        public async Task<bool> ExisteCorreoElectronicoAsync(string correo, int? idUsuario = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = idUsuario.HasValue
                    ? "SELECT COUNT(*) FROM Usuarios WHERE Correo_Electronico = @Correo AND ID_Usuario != @IdUsuario"
                    : "SELECT COUNT(*) FROM Usuarios WHERE Correo_Electronico = @Correo";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Correo", correo);
                if (idUsuario.HasValue)
                {
                    command.Parameters.AddWithValue("@IdUsuario", idUsuario.Value);
                }

                var count = (int)await command.ExecuteScalarAsync()!;
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar correo electrónico");
                throw;
            }
        }

        // Obtener roles disponibles
        public List<string> ObtenerRolesDisponibles()
        {
            return new List<string>
            {
                "Administrador",
                "Agente",
                "Supervisor"
            };
        }

        // Mapear usuario desde DataReader
        private Usuario MapearUsuario(SqlDataReader reader)
        {
            return new Usuario
            {
                ID_Usuario = reader.GetInt32(reader.GetOrdinal("ID_Usuario")),
                Nombres = reader.GetString(reader.GetOrdinal("Nombres")),
                Apellidos = reader.GetString(reader.GetOrdinal("Apellidos")),
                Correo_Electronico = reader.GetString(reader.GetOrdinal("Correo_Electronico")),
                Rol = reader.GetString(reader.GetOrdinal("Rol")),
                Activo = reader.GetBoolean(reader.GetOrdinal("Activo")),
                Fecha_Creacion = reader.GetDateTime(reader.GetOrdinal("Fecha_Creacion")),
                Ultimo_Acceso = reader.IsDBNull(reader.GetOrdinal("Ultimo_Acceso")) 
                    ? null 
                    : reader.GetDateTime(reader.GetOrdinal("Ultimo_Acceso"))
            };
        }

        // Hash de contraseña (simple - usar BCrypt en producción)
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}

