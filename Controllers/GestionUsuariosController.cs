using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.DTOs;
using Inmobiliaria.Net8.Services;

namespace Inmobiliaria.Net8.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class GestionUsuariosController : Controller
    {
        private readonly GestionUsuariosService _gestionUsuariosService;
        private readonly ILogger<GestionUsuariosController> _logger;

        public GestionUsuariosController(
            GestionUsuariosService gestionUsuariosService,
            ILogger<GestionUsuariosController> logger)
        {
            _gestionUsuariosService = gestionUsuariosService;
            _logger = logger;
        }

        // GET: GestionUsuarios
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var usuarios = await _gestionUsuariosService.ObtenerUsuariosAsync();
                var roles = _gestionUsuariosService.ObtenerRolesDisponibles();

                EstadisticasUsuarios estadisticas;
                try
                {
                    estadisticas = await _gestionUsuariosService.ObtenerEstadisticasAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error al obtener estadísticas, usando valores por defecto");
                    estadisticas = new EstadisticasUsuarios
                    {
                        TotalUsuarios = usuarios.Count,
                        UsuariosActivos = usuarios.Count(u => u.Activo),
                        UsuariosInactivos = usuarios.Count(u => !u.Activo),
                        TotalRoles = roles.Count,
                        UsuariosActivosUltimaSemana = 0,
                        UsuariosActivosUltimoMes = 0,
                        EstadisticasPorRol = new List<EstadisticaRol>()
                    };
                }

                ViewBag.Roles = roles;
                ViewBag.Estadisticas = estadisticas;

                return View(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuarios");
                TempData["Error"] = $"Error al cargar usuarios: {ex.Message}";
                
                ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                ViewBag.Estadisticas = new EstadisticasUsuarios();
                
                return View(new List<Usuario>());
            }
        }

        // GET: GestionUsuarios/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
            return View();
        }

        // POST: GestionUsuarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(string nombres, string apellidos, string correoElectronico, 
            string clave, string confirmarClave, string rol)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrEmpty(nombres) || string.IsNullOrEmpty(apellidos) || 
                    string.IsNullOrEmpty(correoElectronico) || string.IsNullOrEmpty(clave) || 
                    string.IsNullOrEmpty(rol))
                {
                    TempData["Error"] = "Todos los campos son obligatorios";
                    ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                    return View();
                }

                if (clave != confirmarClave)
                {
                    TempData["Error"] = "Las claves no coinciden";
                    ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                    return View();
                }

                if (clave.Length < 6)
                {
                    TempData["Error"] = "La clave debe tener al menos 6 caracteres";
                    ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                    return View();
                }

                // Verificar si el correo ya existe
                var existeCorreo = await _gestionUsuariosService.ExisteCorreoElectronicoAsync(correoElectronico);
                if (existeCorreo)
                {
                    TempData["Error"] = "El correo electrónico ya está registrado";
                    ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                    return View();
                }

                // Obtener usuario actual
                var usuarioActual = User.Identity?.Name;

                // Crear usuario
                var nuevoUsuario = await _gestionUsuariosService.CrearUsuarioAsync(
                    nombres, apellidos, correoElectronico, clave, rol, usuarioActual);

                if (nuevoUsuario != null)
                {
                    TempData["Success"] = $"Usuario '{nuevoUsuario.NombreCompleto}' creado exitosamente";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "No se pudo crear el usuario";
                    ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                    return View();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario");
                TempData["Error"] = $"Error al crear usuario: {ex.Message}";
                ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                return View();
            }
        }

        // GET: GestionUsuarios/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                var usuario = await _gestionUsuariosService.ObtenerUsuarioPorIdAsync(id);
                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction("Index");
                }

                ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                return View(usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar usuario");
                TempData["Error"] = $"Error al cargar usuario: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // POST: GestionUsuarios/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, string nombres, string apellidos, string correoElectronico, 
            string rol, bool activo, bool cambiarClave = false, string? nuevaClave = null, string? confirmarClave = null)
        {
            try
            {
                // Validaciones
                if (string.IsNullOrEmpty(nombres) || string.IsNullOrEmpty(apellidos) || 
                    string.IsNullOrEmpty(correoElectronico) || string.IsNullOrEmpty(rol))
                {
                    TempData["Error"] = "Los campos obligatorios no pueden estar vacíos";
                    ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                    var usuario = await _gestionUsuariosService.ObtenerUsuarioPorIdAsync(id);
                    return View(usuario);
                }

                if (cambiarClave)
                {
                    if (string.IsNullOrEmpty(nuevaClave))
                    {
                        TempData["Error"] = "La nueva clave es obligatoria";
                        ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                        var usuario = await _gestionUsuariosService.ObtenerUsuarioPorIdAsync(id);
                        return View(usuario);
                    }

                    if (nuevaClave != confirmarClave)
                    {
                        TempData["Error"] = "Las claves no coinciden";
                        ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                        var usuario = await _gestionUsuariosService.ObtenerUsuarioPorIdAsync(id);
                        return View(usuario);
                    }

                    if (nuevaClave.Length < 6)
                    {
                        TempData["Error"] = "La clave debe tener al menos 6 caracteres";
                        ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                        var usuario = await _gestionUsuariosService.ObtenerUsuarioPorIdAsync(id);
                        return View(usuario);
                    }
                }

                // Verificar si el correo ya existe en otro usuario
                var existeCorreo = await _gestionUsuariosService.ExisteCorreoElectronicoAsync(correoElectronico, id);
                if (existeCorreo)
                {
                    TempData["Error"] = "El correo electrónico ya está registrado por otro usuario";
                    ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                    var usuario = await _gestionUsuariosService.ObtenerUsuarioPorIdAsync(id);
                    return View(usuario);
                }

                // Obtener usuario actual
                var usuarioActual = User.Identity?.Name;

                // Actualizar usuario
                var usuarioActualizado = await _gestionUsuariosService.ActualizarUsuarioAsync(
                    id, nombres, apellidos, correoElectronico, rol, activo, usuarioActual, cambiarClave, nuevaClave);

                if (usuarioActualizado != null)
                {
                    TempData["Success"] = $"Usuario '{usuarioActualizado.NombreCompleto}' actualizado exitosamente";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["Error"] = "No se pudo actualizar el usuario";
                    ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                    var usuario = await _gestionUsuariosService.ObtenerUsuarioPorIdAsync(id);
                    return View(usuario);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario");
                TempData["Error"] = $"Error al actualizar usuario: {ex.Message}";
                ViewBag.Roles = _gestionUsuariosService.ObtenerRolesDisponibles();
                var usuario = await _gestionUsuariosService.ObtenerUsuarioPorIdAsync(id);
                return View(usuario);
            }
        }

        // POST: GestionUsuarios/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var usuarioActual = User.Identity?.Name;
                var resultado = await _gestionUsuariosService.EliminarUsuarioAsync(id, usuarioActual);

                if (resultado)
                {
                    return Json(new { success = true, message = "Usuario eliminado exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo eliminar el usuario" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario");
                return Json(new { success = false, message = $"Error al eliminar usuario: {ex.Message}" });
            }
        }

        // POST: GestionUsuarios/CambiarEstado
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(int id, bool activo)
        {
            try
            {
                var usuarioActual = User.Identity?.Name;
                var resultado = await _gestionUsuariosService.ActualizarUsuarioAsync(
                    id, null, null, null, null, activo, usuarioActual);

                if (resultado != null)
                {
                    return Json(new { success = true, message = $"Usuario {(activo ? "activado" : "desactivado")} exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo cambiar el estado del usuario" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado");
                return Json(new { success = false, message = $"Error al cambiar estado: {ex.Message}" });
            }
        }

        // GET: GestionUsuarios/ObtenerEstadisticas
        [HttpGet]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            try
            {
                var estadisticas = await _gestionUsuariosService.ObtenerEstadisticasAsync();
                return Json(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return Json(new { error = ex.Message });
            }
        }

        // GET: GestionUsuarios/VerificarCorreo
        [HttpGet]
        public async Task<IActionResult> VerificarCorreo(string correo, int? idUsuario = null)
        {
            try
            {
                var existe = await _gestionUsuariosService.ExisteCorreoElectronicoAsync(correo, idUsuario);
                return Json(new { existe = existe });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar correo");
                return Json(new { error = ex.Message });
            }
        }

        // GET: GestionUsuarios/ObtenerUsuarios
        [HttpGet]
        public async Task<IActionResult> ObtenerUsuarios(string? rol = null, bool? activo = null, string? buscar = null)
        {
            try
            {
                var usuarios = await _gestionUsuariosService.ObtenerUsuariosAsync(rol, activo, buscar);
                var usuariosJson = usuarios.Select(u => new
                {
                    u.ID_Usuario,
                    u.Nombres,
                    u.Apellidos,
                    u.Correo_Electronico,
                    u.Rol,
                    u.Activo,
                    u.Fecha_Creacion,
                    u.Ultimo_Acceso,
                    NombreCompleto = u.NombreCompleto,
                    InicialAvatar = u.InicialAvatar,
                    EstadoUltimoAcceso = u.Ultimo_Acceso?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca",
                    EstadoUsuario = u.Activo ? "Activo" : "Inactivo"
                });

                return Json(usuariosJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios");
                return Json(new { error = ex.Message });
            }
        }
    }
}

