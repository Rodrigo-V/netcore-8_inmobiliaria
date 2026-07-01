using System.Data;
using Microsoft.Data.SqlClient;
using Inmobiliaria.Net8.DTOs;
using Inmobiliaria.Net8.Helpers;

namespace Inmobiliaria.Net8.Services
{
    public class SincronizacionPortalesService
    {
        private readonly string _connectionString;
        private readonly ILogger<SincronizacionPortalesService> _logger;

        public SincronizacionPortalesService(IConfiguration configuration, ILogger<SincronizacionPortalesService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _logger = logger;
        }

        /// <summary>
        /// Obtener datos para la vista matriz usando SP_ObtenerMatrizClics
        /// </summary>
        public async Task<List<MatrizSincronizacion>> ObtenerMatrizSincronizacionAsync(
            string? propiedad = null, 
            string? comuna = null, 
            string? region = null, 
            string? tipoPropiedad = null)
        {
            var lista = new List<MatrizSincronizacion>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_ObtenerMatrizClics", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlDefaults.CommandTimeoutSeconds;

                // Parámetros del stored procedure
                command.Parameters.AddWithValue("@Comuna", string.IsNullOrEmpty(comuna) ? (object)DBNull.Value : comuna);
                command.Parameters.AddWithValue("@Region", string.IsNullOrEmpty(region) ? (object)DBNull.Value : region);
                command.Parameters.AddWithValue("@TipoPropiedad", string.IsNullOrEmpty(tipoPropiedad) ? (object)DBNull.Value : tipoPropiedad);
                command.Parameters.AddWithValue("@Agente", DBNull.Value);
                command.Parameters.AddWithValue("@TopRegistros", 100);

                _logger.LogInformation("Ejecutando SP_ObtenerMatrizClics con Comuna={Comuna}, Region={Region}, TipoPropiedad={TipoPropiedad}", 
                    comuna ?? "null", region ?? "null", tipoPropiedad ?? "null");

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var matriz = new MatrizSincronizacion
                    {
                        ID_Propiedad = reader["ID_Propiedad"]?.ToString() ?? string.Empty,
                        Titulo = reader["Titulo"]?.ToString() ?? string.Empty,
                        Comuna = reader["Comuna"]?.ToString() ?? string.Empty,
                        Region = reader["Region"]?.ToString() ?? string.Empty,
                        TipoPropiedad = reader["TipoPropiedad"]?.ToString() ?? string.Empty,
                        Valor = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0,
                        TotalClics = reader["TotalClicsTodosPortales"] != DBNull.Value ? Convert.ToInt32(reader["TotalClicsTodosPortales"]) : 0
                    };

                    // Agregar clics por portal
                    matriz.ClicsPorPortal = new Dictionary<string, int>
                    {
                        { "PortalInmobiliario", reader["ClicsPortalInmobiliario"] != DBNull.Value ? Convert.ToInt32(reader["ClicsPortalInmobiliario"]) : 0 },
                        { "Proppit", reader["ClicsProppit"] != DBNull.Value ? Convert.ToInt32(reader["ClicsProppit"]) : 0 },
                        { "ChilePropiedades", reader["ClicsChilePropiedades"] != DBNull.Value ? Convert.ToInt32(reader["ClicsChilePropiedades"]) : 0 },
                        { "TocToc", reader["ClicsTocToc"] != DBNull.Value ? Convert.ToInt32(reader["ClicsTocToc"]) : 0 }
                    };

                    lista.Add(matriz);
                }

                _logger.LogInformation("SP_ObtenerMatrizClics retornó {Count} registros", lista.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener matriz de sincronización desde SP_ObtenerMatrizClics");
                throw;
            }

            return lista;
        }

        /// <summary>
        /// Obtener resumen de portales (consulta la matriz en BD).
        /// </summary>
        public async Task<List<ResumenPortal>> ObtenerResumenPortalesAsync()
        {
            var matrizData = await ObtenerMatrizSincronizacionAsync();
            return CalcularResumenDesdeMatriz(matrizData);
        }

        /// <summary>
        /// Calcula estadísticas por portal a partir de datos de matriz ya cargados.
        /// </summary>
        public List<ResumenPortal> CalcularResumenDesdeMatriz(List<MatrizSincronizacion> matrizData)
        {
            var lista = new List<ResumenPortal>();

            if (!matrizData.Any())
                return lista;

            var portales = new[]
            {
                ("Portal Inmobiliario", "PortalInmobiliario"),
                ("Proppit", "Proppit"),
                ("Chile Propiedades", "ChilePropiedades"),
                ("TocToc", "TocToc")
            };

            foreach (var (nombrePortal, clavePortal) in portales)
            {
                var totalClics = matrizData.Sum(m => m.ClicsPorPortal.GetValueOrDefault(clavePortal, 0));
                var propiedadesActivas = matrizData.Count(m => m.ClicsPorPortal.GetValueOrDefault(clavePortal, 0) > 0);

                lista.Add(new ResumenPortal
                {
                    NombrePortal = nombrePortal,
                    TotalClics = totalClics,
                    PropiedadesActivas = propiedadesActivas,
                    PromedioClics = propiedadesActivas > 0 ? (double)totalClics / propiedadesActivas : 0
                });
            }

            _logger.LogInformation("Resumen de portales calculado: {Count} portales", lista.Count);
            return lista;
        }

        /// <summary>
        /// Obtener propiedades con clics usando SP_ObtenerPropiedadesConClics
        /// </summary>
        public async Task<List<PropiedadConClics>> ObtenerPropiedadesConClicsAsync(
            string? buscarPropiedad = null, 
            string ordenarPor = "clicks")
        {
            var lista = new List<PropiedadConClics>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_ObtenerPropiedadesConClics", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlDefaults.CommandTimeoutSeconds;

                // Parámetros del stored procedure
                command.Parameters.AddWithValue("@BuscarPropiedad", string.IsNullOrEmpty(buscarPropiedad) ? (object)DBNull.Value : buscarPropiedad);
                command.Parameters.AddWithValue("@OrdenarPor", ordenarPor ?? "clicks");
                command.Parameters.AddWithValue("@TopRegistros", 50);

                _logger.LogInformation("Ejecutando SP_ObtenerPropiedadesConClics. Buscar={Buscar}, Ordenar={Ordenar}", 
                    buscarPropiedad ?? "null", ordenarPor);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var prop = new PropiedadConClics
                    {
                        ID_Propiedad = reader["ID_Propiedad"]?.ToString() ?? string.Empty,
                        Titulo = reader["Titulo"]?.ToString() ?? string.Empty,
                        Direccion = reader["Direccion"]?.ToString() ?? string.Empty,
                        Comuna = reader["Comuna"]?.ToString() ?? string.Empty,
                        Region = reader["Region"]?.ToString() ?? string.Empty,
                        TipoPropiedad = reader["TipoPropiedad"]?.ToString() ?? string.Empty,
                        Valor = reader["Precio"] != DBNull.Value ? Convert.ToDecimal(reader["Precio"]) : 0,
                        ImagenUrl = reader["Url_Imagen"] != DBNull.Value ? reader["Url_Imagen"]?.ToString() : null,
                        TotalClics = reader["TotalClicsTodosPortales"] != DBNull.Value ? Convert.ToInt32(reader["TotalClicsTodosPortales"]) : 0,
                        UltimoClick = reader["UltimoClick"] != DBNull.Value ? Convert.ToDateTime(reader["UltimoClick"]) : null
                    };

                    // Agregar clics por portal
                    prop.ClicsPorPortal = new List<ClicPorPortal>
                    {
                        new ClicPorPortal
                        {
                            NombrePortal = "Portal Inmobiliario",
                            CantidadClics = reader["ClicsPortalInmobiliario"] != DBNull.Value ? Convert.ToInt32(reader["ClicsPortalInmobiliario"]) : 0
                        },
                        new ClicPorPortal
                        {
                            NombrePortal = "Proppit",
                            CantidadClics = reader["ClicsProppit"] != DBNull.Value ? Convert.ToInt32(reader["ClicsProppit"]) : 0
                        },
                        new ClicPorPortal
                        {
                            NombrePortal = "Chile Propiedades",
                            CantidadClics = reader["ClicsChilePropiedades"] != DBNull.Value ? Convert.ToInt32(reader["ClicsChilePropiedades"]) : 0
                        },
                        new ClicPorPortal
                        {
                            NombrePortal = "TocToc",
                            CantidadClics = reader["ClicsTocToc"] != DBNull.Value ? Convert.ToInt32(reader["ClicsTocToc"]) : 0
                        }
                    };

                    lista.Add(prop);
                }

                _logger.LogInformation("SP_ObtenerPropiedadesConClics retornó {Count} propiedades", lista.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedades con clics desde SP_ObtenerPropiedadesConClics");
                throw;
            }

            return lista;
        }

        /// <summary>
        /// Obtener estadísticas generales usando SP_ObtenerEstadisticasGenerales
        /// </summary>
        public async Task<EstadisticasGenerales> ObtenerEstadisticasGeneralesAsync()
        {
            var estadisticas = new EstadisticasGenerales();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_ObtenerEstadisticasGenerales", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlDefaults.CommandTimeoutSeconds;

                _logger.LogInformation("Ejecutando SP_ObtenerEstadisticasGenerales");
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    estadisticas.TotalPropiedades = reader["TotalPropiedades"] != DBNull.Value 
                        ? Convert.ToInt32(reader["TotalPropiedades"]) : 0;
                    estadisticas.TotalClics = reader["TotalClics"] != DBNull.Value 
                        ? Convert.ToInt32(reader["TotalClics"]) : 0;
                    estadisticas.PropiedadesConClics = reader["PropiedadesConClics"] != DBNull.Value 
                        ? Convert.ToInt32(reader["PropiedadesConClics"]) : 0;
                    estadisticas.PromedioClicsPorPropiedad = reader["PromedioClicsPorPropiedad"] != DBNull.Value 
                        ? Convert.ToDouble(reader["PromedioClicsPorPropiedad"]) : 0;
                }

                _logger.LogInformation("Estadísticas obtenidas: Total={Total}, Clics={Clics}, ConClics={ConClics}, Promedio={Promedio}", 
                    estadisticas.TotalPropiedades, estadisticas.TotalClics, estadisticas.PropiedadesConClics, estadisticas.PromedioClicsPorPropiedad);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas generales desde SP_ObtenerEstadisticasGenerales");
                throw;
            }

            return estadisticas;
        }

        /// <summary>
        /// Obtener datos para exportar a Excel usando SP_ObtenerDatosExcelMatriz
        /// </summary>
        public async Task<List<DatosExcelMatriz>> ObtenerDatosExcelMatrizAsync(DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var lista = new List<DatosExcelMatriz>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SP_ObtenerDatosExcelMatriz", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.CommandTimeout = SqlDefaults.CommandTimeoutSeconds;

                command.Parameters.AddWithValue("@FechaDesde", fechaDesde.HasValue ? (object)fechaDesde.Value : DBNull.Value);
                command.Parameters.AddWithValue("@FechaHasta", fechaHasta.HasValue ? (object)fechaHasta.Value : DBNull.Value);

                _logger.LogInformation("Ejecutando SP_ObtenerDatosExcelMatriz. FechaDesde={FechaDesde}, FechaHasta={FechaHasta}", 
                    fechaDesde?.ToString("yyyy-MM-dd") ?? "null", fechaHasta?.ToString("yyyy-MM-dd") ?? "null");

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    lista.Add(new DatosExcelMatriz
                    {
                        FechaSincronizacion = reader["FechaSincronizacion"] != DBNull.Value ? Convert.ToDateTime(reader["FechaSincronizacion"]) : DateTime.MinValue,
                        ID_Propiedad = reader["ID_Propiedad"]?.ToString() ?? string.Empty,
                        TituloPropiedad = reader["TituloPropiedad"]?.ToString() ?? string.Empty,
                        Comuna = reader["Comuna"]?.ToString() ?? string.Empty,
                        TotalClics = reader["TotalClics"] != DBNull.Value ? Convert.ToInt32(reader["TotalClics"]) : 0,
                        ClicsPortalInmobiliario = reader["ClicsPortalInmobiliario"] != DBNull.Value ? Convert.ToInt32(reader["ClicsPortalInmobiliario"]) : 0,
                        ClicsProppit = reader["ClicsProppit"] != DBNull.Value ? Convert.ToInt32(reader["ClicsProppit"]) : 0,
                        ClicsChilePropiedades = reader["ClicsChilePropiedades"] != DBNull.Value ? Convert.ToInt32(reader["ClicsChilePropiedades"]) : 0,
                        ClicsTocToc = reader["ClicsTocToc"] != DBNull.Value ? Convert.ToInt32(reader["ClicsTocToc"]) : 0
                    });
                }

                _logger.LogInformation("SP_ObtenerDatosExcelMatriz retornó {Count} registros", lista.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener datos para Excel desde SP_ObtenerDatosExcelMatriz");
                throw;
            }

            return lista;
        }
    }
}

