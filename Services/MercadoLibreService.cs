using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Inmobiliaria.Net8.DTOs;

namespace Inmobiliaria.Net8.Services
{
    public class MercadoLibreService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly string _connectionString;
        private readonly string _siteId; // MLC para Chile, MLA para Argentina, etc.

        public MercadoLibreService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _clientId = configuration["MercadoLibre:ClientId"]
                ?? throw new InvalidOperationException("MercadoLibre:ClientId no está configurado.");
            _clientSecret = configuration["MercadoLibre:ClientSecret"]
                ?? throw new InvalidOperationException("MercadoLibre:ClientSecret no está configurado.");
            _redirectUri = configuration["MercadoLibre:RedirectUri"]
                ?? throw new InvalidOperationException("MercadoLibre:RedirectUri no está configurado.");
            _siteId = configuration["MercadoLibre:SiteId"] ?? "MLC"; // Por defecto Chile
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        #region Autenticación y Autorización

        /// <summary>
        /// Obtiene la URL para que el usuario autorice la aplicación
        /// </summary>
        public string GetAuthorizationUrl()
        {
            string baseUrl = GetAuthBaseUrl();
            return $"{baseUrl}/authorization?response_type=code&client_id={_clientId}&redirect_uri={_redirectUri}";
        }

        /// <summary>
        /// Intercambia el código de autorización por un access token
        /// </summary>
        public async Task<MercadoLibreToken> GetAccessTokenAsync(string code)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mercadolibre.com/oauth/token");
                
                var parameters = new Dictionary<string, string>
                {
                    { "grant_type", "authorization_code" },
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "code", code },
                    { "redirect_uri", _redirectUri }
                };

                request.Content = new FormUrlEncodedContent(parameters);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener token: {content}");
                }

                var token = JsonSerializer.Deserialize<MercadoLibreToken>(content);
                
                // Guardar en base de datos
                await GuardarTokenAsync(token);
                
                return token;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetAccessTokenAsync: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Renueva el access token usando el refresh token
        /// </summary>
        public async Task<MercadoLibreToken> RefreshAccessTokenAsync(string refreshToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mercadolibre.com/oauth/token");
                
                var parameters = new Dictionary<string, string>
                {
                    { "grant_type", "refresh_token" },
                    { "client_id", _clientId },
                    { "client_secret", _clientSecret },
                    { "refresh_token", refreshToken }
                };

                request.Content = new FormUrlEncodedContent(parameters);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al renovar token: {content}");
                }

                var token = JsonSerializer.Deserialize<MercadoLibreToken>(content);
                
                // Actualizar en base de datos
                await ActualizarTokenAsync(token);
                
                return token;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en RefreshAccessTokenAsync: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un access token válido (renueva si es necesario)
        /// </summary>
        public async Task<string> GetValidAccessTokenAsync()
        {
            var tokenDB = await ObtenerTokenActivoAsync();
            
            if (tokenDB == null)
            {
                throw new Exception("No hay token de Mercado Libre configurado. Debe autorizar la aplicación primero.");
            }

            // Si el token expira en menos de 1 hora, renovarlo
            if (tokenDB.FechaExpiracion <= DateTime.Now.AddHours(1))
            {
                var newToken = await RefreshAccessTokenAsync(tokenDB.RefreshToken);
                return newToken.access_token;
            }

            return tokenDB.AccessToken;
        }

        #endregion

        #region Consultas de Visitas

        /// <summary>
        /// Obtiene las visitas totales de un item
        /// </summary>
        public async Task<int> GetItemTotalVisitsAsync(string itemId)
        {
            try
            {
                var accessToken = await GetValidAccessTokenAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"https://api.mercadolibre.com/visits/items?ids={itemId}");
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener visitas: {content}");
                }

                var result = JsonSerializer.Deserialize<Dictionary<string, int>>(content);
                return result.ContainsKey(itemId) ? result[itemId] : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetItemTotalVisitsAsync: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene las visitas de un item en un rango de fechas
        /// </summary>
        public async Task<ItemVisits> GetItemVisitsAsync(string itemId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                var accessToken = await GetValidAccessTokenAsync();
                
                var dateFromStr = dateFrom.ToString("yyyy-MM-dd");
                var dateToStr = dateTo.ToString("yyyy-MM-dd");
                
                var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"https://api.mercadolibre.com/items/visits?ids={itemId}&date_from={dateFromStr}&date_to={dateToStr}");
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener visitas: {content}");
                }

                return JsonSerializer.Deserialize<ItemVisits>(content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetItemVisitsAsync: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene las visitas detalladas por día de un item
        /// </summary>
        public async Task<ItemVisitsTimeWindow> GetItemVisitsTimeWindowAsync(string itemId, int lastDays)
        {
            try
            {
                var accessToken = await GetValidAccessTokenAsync();
                
                var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"https://api.mercadolibre.com/items/{itemId}/visits/time_window?last={lastDays}&unit=day");
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener visitas por día: {content}");
                }

                return JsonSerializer.Deserialize<ItemVisitsTimeWindow>(content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetItemVisitsTimeWindowAsync: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene todas las visitas de los items de un usuario
        /// </summary>
        public async Task<UserVisits> GetUserItemsVisitsAsync(long userId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                var accessToken = await GetValidAccessTokenAsync();
                
                var dateFromStr = dateFrom.ToString("yyyy-MM-dd");
                var dateToStr = dateTo.ToString("yyyy-MM-dd");
                
                var request = new HttpRequestMessage(HttpMethod.Get, 
                    $"https://api.mercadolibre.com/users/{userId}/items_visits?date_from={dateFromStr}&date_to={dateToStr}");
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener visitas del usuario: {content}");
                }

                return JsonSerializer.Deserialize<UserVisits>(content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetUserItemsVisitsAsync: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene estadísticas completas de visitas de un item
        /// </summary>
        public async Task<EstadisticasVisitasML> GetEstadisticasVisitasAsync(string itemId, string tituloPropiedad)
        {
            try
            {
                var estadisticas = new EstadisticasVisitasML
                {
                    ItemId = itemId,
                    TituloPropiedad = tituloPropiedad,
                    FechaConsulta = DateTime.Now,
                    VisitasPorDia = new List<VisitasPorDia>()
                };

                // Obtener visitas totales
                estadisticas.VisitasTotales = await GetItemTotalVisitsAsync(itemId);

                // Obtener visitas de los últimos 30 días
                var visits30 = await GetItemVisitsAsync(itemId, DateTime.Now.AddDays(-30), DateTime.Now);
                estadisticas.VisitasUltimos30Dias = visits30?.total_visits ?? 0;

                // Obtener visitas de los últimos 7 días
                var visits7 = await GetItemVisitsAsync(itemId, DateTime.Now.AddDays(-7), DateTime.Now);
                estadisticas.VisitasUltimos7Dias = visits7?.total_visits ?? 0;

                // Obtener visitas detalladas por día (últimos 30 días)
                var visitsTimeWindow = await GetItemVisitsTimeWindowAsync(itemId, 30);
                
                if (visitsTimeWindow?.results != null)
                {
                    foreach (var result in visitsTimeWindow.results)
                    {
                        estadisticas.VisitasPorDia.Add(new VisitasPorDia
                        {
                            Fecha = DateTime.Parse(result.date),
                            Visitas = result.total
                        });
                    }
                }

                return estadisticas;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en GetEstadisticasVisitasAsync: {ex.Message}", ex);
            }
        }

        #endregion

        #region Gestión de Tokens en Base de Datos

        /// <summary>
        /// Guarda un nuevo token en la base de datos
        /// </summary>
        private async Task GuardarTokenAsync(MercadoLibreToken token)
        {
            try
            {
                using (var conexion = new SqlConnection(_connectionString))
                {
                    await conexion.OpenAsync();
                    
                    using (var comando = new SqlCommand("PP_psnp_MercadoLibre_GuardarToken", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        
                        comando.Parameters.AddWithValue("@UserId", token.user_id);
                        comando.Parameters.AddWithValue("@AccessToken", token.access_token);
                        comando.Parameters.AddWithValue("@RefreshToken", token.refresh_token);
                        comando.Parameters.AddWithValue("@ExpiresIn", token.expires_in);
                        
                        await comando.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar token en BD: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza un token existente en la base de datos
        /// </summary>
        private async Task ActualizarTokenAsync(MercadoLibreToken token)
        {
            try
            {
                using (var conexion = new SqlConnection(_connectionString))
                {
                    await conexion.OpenAsync();
                    
                    using (var comando = new SqlCommand("PP_psnp_MercadoLibre_ActualizarToken", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        
                        comando.Parameters.AddWithValue("@UserId", token.user_id);
                        comando.Parameters.AddWithValue("@AccessToken", token.access_token);
                        comando.Parameters.AddWithValue("@RefreshToken", token.refresh_token);
                        comando.Parameters.AddWithValue("@ExpiresIn", token.expires_in);
                        
                        await comando.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar token en BD: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el token activo desde la base de datos
        /// </summary>
        private async Task<MercadoLibreTokenDB> ObtenerTokenActivoAsync()
        {
            try
            {
                using (var conexion = new SqlConnection(_connectionString))
                {
                    await conexion.OpenAsync();
                    
                    using (var comando = new SqlCommand("PP_psnp_MercadoLibre_ObtenerTokenActivo", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        
                        using (var reader = await comando.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new MercadoLibreTokenDB
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    UserId = reader.GetInt64(reader.GetOrdinal("UserId")),
                                    AccessToken = reader.GetString(reader.GetOrdinal("AccessToken")),
                                    RefreshToken = reader.GetString(reader.GetOrdinal("RefreshToken")),
                                    FechaExpiracion = reader.GetDateTime(reader.GetOrdinal("FechaExpiracion")),
                                    FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FechaCreacion")),
                                    Activo = reader.GetBoolean(reader.GetOrdinal("Activo"))
                                };
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener token de BD: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el estado de la conexión con Mercado Libre
        /// </summary>
        public async Task<EstadoConexionML> ObtenerEstadoConexionAsync()
        {
            try
            {
                var token = await ObtenerTokenActivoAsync();
                
                if (token == null)
                {
                    return new EstadoConexionML
                    {
                        Conectado = false,
                        Mensaje = "No hay conexión configurada con Mercado Libre"
                    };
                }

                var diasRestantes = (token.FechaExpiracion - DateTime.Now).Days;
                
                return new EstadoConexionML
                {
                    Conectado = true,
                    UserId = token.UserId,
                    FechaExpiracion = token.FechaExpiracion,
                    DiasRestantes = diasRestantes > 0 ? diasRestantes : 0,
                    Mensaje = diasRestantes > 30 
                        ? "Conexión activa" 
                        : diasRestantes > 0 
                            ? $"La conexión expira en {diasRestantes} días. Considere renovarla pronto."
                            : "La conexión ha expirado. Debe autorizar nuevamente."
                };
            }
            catch (Exception ex)
            {
                return new EstadoConexionML
                {
                    Conectado = false,
                    Mensaje = $"Error al verificar conexión: {ex.Message}"
                };
            }
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Obtiene la URL base de autenticación según el site configurado
        /// </summary>
        private string GetAuthBaseUrl()
        {
            return _siteId switch
            {
                "MLC" => "https://auth.mercadolibre.cl",
                "MLA" => "https://auth.mercadolibre.com.ar",
                "MLB" => "https://auth.mercadolibre.com.br",
                "MLM" => "https://auth.mercadolibre.com.mx",
                "MCO" => "https://auth.mercadolibre.com.co",
                "MLU" => "https://auth.mercadolibre.com.uy",
                "MPE" => "https://auth.mercadolibre.com.pe",
                _ => "https://auth.mercadolibre.cl" // Por defecto Chile
            };
        }

        #endregion
    }
}

