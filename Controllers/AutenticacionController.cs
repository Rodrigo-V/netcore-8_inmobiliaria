using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Inmobiliaria.Net8.DTOs;
using Inmobiliaria.Net8.Services;

namespace Inmobiliaria.Net8.Controllers
{
    public class AutenticacionController : Controller
    {
        private readonly AutenticacionService _autenticacionService;
        private readonly ILogger<AutenticacionController> _logger;

        public AutenticacionController(AutenticacionService autenticacionService, ILogger<AutenticacionController> logger)
        {
            _autenticacionService = autenticacionService;
            _logger = logger;
        }

        // GET: Autenticacion/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            // Limpiar sesión anterior si existe
            if (User.Identity?.IsAuthenticated == true)
            {
                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return View();
        }

        // POST: Autenticacion/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            try
            {
                _logger.LogInformation("=== INICIO LOGIN ===");
                _logger.LogInformation("Correo recibido: '{Correo}'", model?.Correo_Electronico);
                _logger.LogInformation("Clave recibida: '{Clave}'", string.IsNullOrEmpty(model?.Clave) ? "VACIA" : "***");
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ERROR: ModelState no válido");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning("  - {ErrorMessage}", error.ErrorMessage);
                    }
                    return Json(new { success = false, message = "Por favor, complete todos los campos requeridos." });
                }

                _logger.LogInformation("Llamando al servicio de autenticación...");
                var usuario = await _autenticacionService.ValidarCredencialesAsync(model.Correo_Electronico, model.Clave);
                
                if (usuario != null)
                {
                    _logger.LogInformation("✅ Usuario encontrado: {NombreCompleto} - Rol: {Rol}", 
                        usuario.NombreCompleto, usuario.Rol);
                    
                    // Crear claims para el usuario
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, usuario.Correo_Electronico),
                        new Claim(ClaimTypes.Email, usuario.Correo_Electronico),
                        new Claim(ClaimTypes.Role, usuario.Rol),
                        new Claim("ID_Usuario", usuario.ID_Usuario.ToString()),
                        new Claim("NombreCompleto", usuario.NombreCompleto),
                        new Claim("Iniciales", usuario.InicialAvatar)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Configurar propiedades de autenticación
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RecordarUsuario,
                        ExpiresUtc = model.RecordarUsuario 
                            ? DateTimeOffset.UtcNow.AddDays(30) 
                            : DateTimeOffset.UtcNow.AddHours(8)
                    };

                    // Iniciar sesión
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        claimsPrincipal,
                        authProperties);

                    // Actualizar último acceso
                    await _autenticacionService.ActualizarUltimoAccesoAsync(usuario.ID_Usuario);

                    _logger.LogInformation("✅ Login exitoso - Redirigiendo a ClientesLeads");
                    return Json(new { success = true, message = "Login exitoso", redirectUrl = Url.Action("Index", "ClientesLeads") });
                }
                else
                {
                    _logger.LogWarning("❌ Credenciales incorrectas - Usuario no encontrado");
                    return Json(new { success = false, message = "Credenciales incorrectas. Verifique su correo y clave." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR CRÍTICO en Login: {Message}", ex.Message);
                return Json(new { success = false, message = $"Error en el servidor: {ex.Message}" });
            }
        }

        // POST: Autenticacion/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                _logger.LogInformation("Sesión cerrada exitosamente");
                
                return Json(new { success = true, message = "Sesión cerrada exitosamente", redirectUrl = Url.Action("Login", "Autenticacion") });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión");
                return Json(new { success = false, message = $"Error al cerrar sesión: {ex.Message}" });
            }
        }

        // GET: Autenticacion/AccesoDenegado
        [AllowAnonymous]
        public IActionResult AccesoDenegado()
        {
            return View();
        }

        // GET: Autenticacion/GetUsuarioActual
        [HttpGet]
        public IActionResult GetUsuarioActual()
        {
            try
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userData = new
                    {
                        ID_Usuario = User.FindFirst("ID_Usuario")?.Value,
                        NombreCompleto = User.FindFirst("NombreCompleto")?.Value,
                        Rol = User.FindFirst(ClaimTypes.Role)?.Value,
                        Iniciales = User.FindFirst("Iniciales")?.Value,
                        Correo = User.Identity.Name
                    };

                    return Json(new { success = true, data = userData });
                }

                return Json(new { success = false, message = "Usuario no autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario actual");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Autenticacion/DiagnosticoUsuarios
        [AllowAnonymous]
        public async Task<IActionResult> DiagnosticoUsuarios()
        {
            try
            {
                var usuarios = await _autenticacionService.GetUsuariosAsync();
                
                var diagnostico = new
                {
                    TotalUsuarios = usuarios.Count,
                    Usuarios = usuarios.Select(u => new
                    {
                        ID = u.ID_Usuario,
                        Nombre = $"{u.Nombres} {u.Apellidos}",
                        Correo = u.Correo_Electronico,
                        Rol = u.Rol,
                        Activo = u.Activo,
                        UltimoAcceso = u.Ultimo_Acceso?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca"
                    }).ToList(),
                    CredencialesDisponibles = new[]
                    {
                        new { Correo = "admin@rosch.cl", Clave = "admin123", Rol = "Administrador" },
                        new { Correo = "gerente@rosch.cl", Clave = "admin123", Rol = "Gerente" },
                        new { Correo = "agente@rosch.cl", Clave = "agent123", Rol = "Agente" },
                        new { Correo = "usuario@rosch.cl", Clave = "user123", Rol = "Usuario" },
                        new { Correo = "rvilchez@rosch.cl", Clave = "admin123", Rol = "Administrador" }
                    }
                };

                return Json(diagnostico);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener diagnóstico");
                return Json(new 
                { 
                    success = false, 
                    message = $"Error al obtener diagnóstico: {ex.Message}",
                    error = ex.ToString()
                });
            }
        }

        // GET: Autenticacion/TestLogin
        [AllowAnonymous]
        public async Task<IActionResult> TestLogin(string correo = "admin@rosch.cl", string clave = "admin123", bool doLogin = false)
        {
            try
            {
                _logger.LogInformation("=== TEST LOGIN DIRECTO ===");
                _logger.LogInformation("Probando: {Correo} / {Clave}", correo, clave);
                _logger.LogInformation("DoLogin: {DoLogin}", doLogin);
                
                var usuario = await _autenticacionService.ValidarCredencialesAsync(correo, clave);
                
                if (usuario != null)
                {
                    _logger.LogInformation("✅ Usuario encontrado: {NombreCompleto} - Rol: {Rol}", 
                        usuario.NombreCompleto, usuario.Rol);
                    
                    if (doLogin)
                    {
                        _logger.LogInformation("Creando sesión real...");
                        
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, usuario.Correo_Electronico),
                            new Claim(ClaimTypes.Email, usuario.Correo_Electronico),
                            new Claim(ClaimTypes.Role, usuario.Rol),
                            new Claim("ID_Usuario", usuario.ID_Usuario.ToString()),
                            new Claim("NombreCompleto", usuario.NombreCompleto),
                            new Claim("Iniciales", usuario.InicialAvatar)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            claimsPrincipal,
                            new AuthenticationProperties { ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

                        _logger.LogInformation("✅ Sesión creada exitosamente");
                        
                        return Json(new
                        {
                            success = true,
                            message = "Login exitoso - Sesión creada",
                            redirectUrl = Url.Action("Index", "ClientesLeads"),
                            usuario = new
                            {
                                ID = usuario.ID_Usuario,
                                Nombre = usuario.NombreCompleto,
                                Correo = usuario.Correo_Electronico,
                                Rol = usuario.Rol,
                                Activo = usuario.Activo
                            }
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            success = true,
                            message = "Credenciales válidas (solo validación)",
                            usuario = new
                            {
                                ID = usuario.ID_Usuario,
                                Nombre = usuario.NombreCompleto,
                                Correo = usuario.Correo_Electronico,
                                Rol = usuario.Rol,
                                Activo = usuario.Activo
                            }
                        });
                    }
                }
                else
                {
                    _logger.LogWarning("❌ Credenciales incorrectas - Usuario no encontrado");
                    return Json(new { success = false, message = "Credenciales incorrectas" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR en TestLogin: {Message}", ex.Message);
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}

