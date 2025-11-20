using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using Inmobiliaria.Net8.DTOs;

namespace Inmobiliaria.Net8.Services
{
    public class AutenticacionService
    {
        private readonly string _connectionString;
        private readonly ILogger<AutenticacionService> _logger;

        public AutenticacionService(IConfiguration configuration, ILogger<AutenticacionService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        public async Task<Usuario?> ValidarCredencialesAsync(string correoElectronico, string clave)
        {
            try
            {
                _logger.LogInformation("=== SERVICIO AUTENTICACION ===");
                _logger.LogInformation("Correo recibido: '{Correo}'", correoElectronico);
                _logger.LogInformation("Clave recibida: '{Clave}'", string.IsNullOrEmpty(clave) ? "VACIA" : "***");
                
                using var connection = new SqlConnection(_connectionString);
                
                _logger.LogInformation("Abriendo conexión a BD...");
                await connection.OpenAsync();
                _logger.LogInformation("✅ Conexión abierta exitosamente");
                
                // Usar la clave directamente sin hashear (igual que el sistema actual)
                _logger.LogDebug("Clave sin hash: {Clave}", clave);
                
                using var command = new SqlCommand("SP_ValidarCredenciales", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Correo_Electronico", correoElectronico);
                command.Parameters.AddWithValue("@Clave_Hash", clave); // Enviamos la clave en texto plano
                
                _logger.LogInformation("Ejecutando SP_ValidarCredenciales...");
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    _logger.LogInformation("✅ Usuario encontrado en BD");
                    
                    var usuario = new Usuario
                    {
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Nombres = reader["Nombres"].ToString() ?? string.Empty,
                        Apellidos = reader["Apellidos"].ToString() ?? string.Empty,
                        Correo_Electronico = reader["Correo_Electronico"].ToString() ?? string.Empty,
                        Clave = reader["Clave"].ToString() ?? string.Empty,
                        Rol = reader["Rol"].ToString() ?? string.Empty,
                        Activo = Convert.ToBoolean(reader["Activo"]),
                        Fecha_Creacion = Convert.ToDateTime(reader["Fecha_Creacion"]),
                        Ultimo_Acceso = reader["Ultimo_Acceso"] == DBNull.Value 
                            ? null 
                            : Convert.ToDateTime(reader["Ultimo_Acceso"])
                    };

                    _logger.LogInformation("Usuario creado: {NombreCompleto} - Rol: {Rol}", 
                        usuario.NombreCompleto, usuario.Rol);
                    return usuario;
                }
                else
                {
                    _logger.LogWarning("❌ No se encontró usuario con esas credenciales");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR en ValidarCredencialesAsync: {Message}", ex.Message);
                throw new Exception($"Error al validar credenciales: {ex.Message}", ex);
            }
        }

        public async Task<bool> ActualizarUltimoAccesoAsync(int idUsuario)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = "UPDATE Usuarios SET Ultimo_Acceso = @FechaAcceso WHERE ID_Usuario = @ID_Usuario";
                
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@FechaAcceso", DateTime.Now);
                command.Parameters.AddWithValue("@ID_Usuario", idUsuario);
                
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar último acceso");
                throw new Exception($"Error al actualizar último acceso: {ex.Message}", ex);
            }
        }

        public string EncriptarClave(string clave)
        {
            using var sha256Hash = SHA256.Create();
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(clave));
            
            var builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            var usuarios = new List<Usuario>();
            
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand("SP_ObtenerUsuarios", connection);
                command.CommandType = CommandType.StoredProcedure;
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var usuario = new Usuario
                    {
                        ID_Usuario = Convert.ToInt32(reader["ID_Usuario"]),
                        Nombres = reader["Nombres"].ToString() ?? string.Empty,
                        Apellidos = reader["Apellidos"].ToString() ?? string.Empty,
                        Correo_Electronico = reader["Correo_Electronico"].ToString() ?? string.Empty,
                        Rol = reader["Rol"].ToString() ?? string.Empty,
                        Activo = Convert.ToBoolean(reader["Activo"]),
                        Fecha_Creacion = Convert.ToDateTime(reader["Fecha_Creacion"]),
                        Ultimo_Acceso = reader["Ultimo_Acceso"] == DBNull.Value 
                            ? null 
                            : Convert.ToDateTime(reader["Ultimo_Acceso"])
                    };
                    usuarios.Add(usuario);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                throw new Exception($"Error al obtener usuarios: {ex.Message}", ex);
            }
            
            return usuarios;
        }

        public async Task<bool> EsAdministradorAsync(int idUsuario)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand("SP_EsAdministrador", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID_Usuario", idUsuario);

                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return Convert.ToBoolean(reader["Es_Administrador"]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si usuario es administrador");
                throw new Exception($"Error al verificar si usuario es administrador: {ex.Message}", ex);
            }

            return false;
        }

        public async Task<bool> EsAgenteAsync(int idUsuario)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = new SqlCommand("SP_EsAgente", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@ID_Usuario", idUsuario);

                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return Convert.ToBoolean(reader["Es_Agente"]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si usuario es agente");
                throw new Exception($"Error al verificar si usuario es agente: {ex.Message}", ex);
            }

            return false;
        }
    }
}

