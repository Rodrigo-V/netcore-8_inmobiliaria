using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inmobiliaria.Net8.DTOs;
using Inmobiliaria.Net8.Services;
using System.Security.Claims;
using Microsoft.Data.SqlClient;

namespace Inmobiliaria.Net8.Controllers
{
    [Authorize] // Requiere autenticación
    public class ClientesLeadsController : Controller
    {
        private readonly ClientesLeadsService _clientesLeadsService;
        private readonly ILogger<ClientesLeadsController> _logger;
        private readonly IConfiguration _configuration;

        public ClientesLeadsController(ClientesLeadsService clientesLeadsService, ILogger<ClientesLeadsController> logger, IConfiguration configuration)
        {
            _clientesLeadsService = clientesLeadsService;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: ClientesLeads/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: ClientesLeads/GetData - DataTables server-side (Todos los leads)
        [HttpPost]
        public async Task<IActionResult> GetData()
        {
            try
            {
                var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault() ?? "1");
                var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
                var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
                var orderColumnIndex = Request.Form["order[0][column]"].FirstOrDefault() ?? "0";
                var orderDir = Request.Form["order[0][dir]"].FirstOrDefault() ?? "desc";

                // Mapear columna
                string columna = "Creado"; // por defecto
                switch (orderColumnIndex)
                {
                    case "0": columna = "ID_Cliente"; break;
                    case "3": columna = "Correo_Electronico"; break;
                    case "4": columna = "Telefono"; break;
                    case "5": columna = "Portal"; break;
                    case "7": columna = "Creado"; break;
                }

                // Construir filtro DTO
                var filtroDto = new FiltroClientesLeads
                {
                    PaginaActual = (start / Math.Max(length, 1)) + 1,
                    TamañoPagina = length,
                    ColumnaOrden = columna,
                    DireccionOrden = orderDir
                };

                var lista = await _clientesLeadsService.ObtenerTodosAsync(filtroDto);
                var total = await _clientesLeadsService.ContarTotalAsync();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = total,
                    recordsFiltered = total,
                    data = lista
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetData");
                return Json(new { draw = 1, recordsTotal = 0, recordsFiltered = 0, data = new object[0], error = ex.Message });
            }
        }

        // POST: ClientesLeads/GetDataMisLeads - DataTables server-side (Solo mis leads)
        [HttpPost]
        public async Task<IActionResult> GetDataMisLeads()
        {
            try
            {
                // Obtener el nombre del usuario logueado
                var usuarioNombre = User.FindFirst("NombreCompleto")?.Value;
                if (string.IsNullOrEmpty(usuarioNombre))
                {
                    return Json(new { draw = 1, recordsTotal = 0, recordsFiltered = 0, data = new object[0], error = "Usuario no identificado" });
                }

                var draw = Convert.ToInt32(Request.Form["draw"].FirstOrDefault() ?? "1");
                var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
                var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
                var orderColumnIndex = Request.Form["order[0][column]"].FirstOrDefault() ?? "0";
                var orderDir = Request.Form["order[0][dir]"].FirstOrDefault() ?? "desc";

                // Mapear columna
                string columna = "Creado";
                switch (orderColumnIndex)
                {
                    case "0": columna = "ID_Cliente"; break;
                    case "3": columna = "Correo_Electronico"; break;
                    case "4": columna = "Telefono"; break;
                    case "5": columna = "Portal"; break;
                    case "7": columna = "Creado"; break;
                }

                // Construir filtro con el usuario actual
                var filtroDto = new FiltroClientesLeads
                {
                    Asistente = usuarioNombre, // Filtrar por usuario logueado
                    PaginaActual = (start / Math.Max(length, 1)) + 1,
                    TamañoPagina = length,
                    ColumnaOrden = columna,
                    DireccionOrden = orderDir
                };

                var lista = await _clientesLeadsService.ObtenerTodosAsync(filtroDto);
                var total = await _clientesLeadsService.ContarTotalPorAsistenteAsync(usuarioNombre);

                return Json(new
                {
                    draw = draw,
                    recordsTotal = total,
                    recordsFiltered = total,
                    data = lista
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetDataMisLeads");
                return Json(new { draw = 1, recordsTotal = 0, recordsFiltered = 0, data = new object[0], error = ex.Message });
            }
        }

        // GET: ClientesLeads/Portales
        [HttpGet]
        public async Task<IActionResult> Portales()
        {
            try
            {
                var lista = await _clientesLeadsService.ObtenerPortalesAsync();
                return Json(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener portales");
                return Json(new List<string>());
            }
        }

        // GET: ClientesLeads/Asistentes
        [HttpGet]
        public async Task<IActionResult> Asistentes()
        {
            try
            {
                var lista = await _clientesLeadsService.ObtenerAsistentesAsync();
                return Json(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener asistentes");
                return Json(new List<string>());
            }
        }

        // GET: ClientesLeads/Estadisticas
        [HttpGet]
        public async Task<IActionResult> Estadisticas()
        {
            try
            {
                var estadisticas = await _clientesLeadsService.ObtenerEstadisticasAsync();
                return Json(new { success = true, data = estadisticas });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: ClientesLeads/Detalle/{id}
        [HttpGet]
        public async Task<IActionResult> Detalle(string id)
        {
            try
            {
                var lead = await _clientesLeadsService.ObtenerPorIdAsync(id);
                if (lead == null)
                {
                    return NotFound(new { success = false, message = "Lead no encontrado" });
                }

                return Json(new { success = true, data = lead });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle del lead: {IdCliente}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: ClientesLeads/Eliminar
        [HttpPost]
        public async Task<IActionResult> Eliminar(string id)
        {
            try
            {
                var resultado = await _clientesLeadsService.EliminarAsync(id);

                if (resultado)
                {
                    return Json(new { success = true, message = "Lead eliminado exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo eliminar el lead" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar lead: {IdCliente}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerModalModificar(string idCliente)
        {
            try
            {
                _logger.LogInformation("Cargando modal de modificación para lead: {IdCliente}", idCliente);

                var lead = await _clientesLeadsService.ObtenerPorIdAsync(idCliente);

                if (lead == null)
                {
                    _logger.LogWarning("Lead no encontrado: {IdCliente}", idCliente);
                    return Content("<div class='alert alert-danger'>Lead no encontrado</div>");
                }

                _logger.LogInformation("Lead encontrado: {Nombres} {Apellidos}", lead.Nombres, lead.Apellidos);

                // Cargar datos auxiliares para los combos
                try
                {
                    ViewBag.Propiedades = await ObtenerListaPropiedades();
                    ViewBag.Asistentes = await _clientesLeadsService.ObtenerAsistentesAsync();
                    ViewBag.Portales = await _clientesLeadsService.ObtenerPortalesAsync();

                    // Lista de seguimientos (valores fijos)
                    ViewBag.Seguimientos = new List<string>
                    {
                        "Nuevo",
                        "En Seguimiento",
                        "Con Visita Programada",
                        "En Espera",
                        "Terminado"
                    };

                    _logger.LogInformation("Listas auxiliares cargadas correctamente");
                }
                catch (Exception exListas)
                {
                    _logger.LogWarning(exListas, "Error al cargar listas auxiliares, usando valores por defecto");
                    ViewBag.Propiedades = new List<object>();
                    ViewBag.Asistentes = new List<string>();
                    ViewBag.Portales = new List<string>();
                    ViewBag.Seguimientos = new List<string> { "Nuevo", "En Seguimiento", "Con Visita Programada", "En Espera", "Terminado" };
                }

                return PartialView("ModificarModal", lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar modal de modificación: {IdCliente}", idCliente);
                return Content($"<div class='alert alert-danger' style='color: #721c24; background-color: #f8d7da; border-color: #f5c6cb;'>Error al cargar el formulario: {ex.Message}</div>");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Modificar(ClienteLead lead)
        {
            try
            {
                // Obtener el lead original para comparar cambios
                var leadOriginal = await _clientesLeadsService.ObtenerPorIdAsync(lead.ID_Cliente);
                if (leadOriginal == null)
                {
                    return Json(new { success = false, message = "Lead no encontrado" });
                }

                _logger.LogInformation("Modificando lead {IdCliente}. Estado original: {EstadoOriginal}", lead.ID_Cliente, leadOriginal.Seguimiento);

                // LÓGICA DE CAMBIO AUTOMÁTICO DE ESTADO (copiada del proyecto anterior)
                // 1. Si ya está "Terminado", mantener "Terminado"
                if (leadOriginal.Seguimiento == "Terminado")
                {
                    lead.Seguimiento = "Terminado";
                    _logger.LogInformation("Lead {IdCliente} ya estaba terminado - manteniendo estado 'Terminado'", lead.ID_Cliente);
                }
                // 2. Para cualquier otro cambio, verificar si hubo modificaciones y cambiar a "En Seguimiento"
                else if (lead.Seguimiento != "Terminado")
                {
                    // Verificar si hubo cambios en algún campo
                    bool huboCambios =
                        leadOriginal.Nombres != lead.Nombres ||
                        leadOriginal.Apellidos != lead.Apellidos ||
                        leadOriginal.Correo_Electronico != lead.Correo_Electronico ||
                        leadOriginal.Telefono != lead.Telefono ||
                        leadOriginal.Portal != lead.Portal ||
                        leadOriginal.Sexo != lead.Sexo ||
                        leadOriginal.Fecha_Contacto != lead.Fecha_Contacto ||
                        leadOriginal.ID_Unidad_Consultada != lead.ID_Unidad_Consultada ||
                        leadOriginal.Unidad_Consultada != lead.Unidad_Consultada ||
                        leadOriginal.Respuesta != lead.Respuesta ||
                        leadOriginal.Visita_Realizada != lead.Visita_Realizada ||
                        leadOriginal.Asistente != lead.Asistente;

                    if (huboCambios && leadOriginal.Seguimiento != "En Seguimiento")
                    {
                        lead.Seguimiento = "En Seguimiento";
                        _logger.LogInformation("Lead {IdCliente} con cambios detectados - cambiando de '{EstadoOriginal}' a 'En Seguimiento'", 
                            lead.ID_Cliente, leadOriginal.Seguimiento);
                    }
                    else
                    {
                        _logger.LogInformation("Lead {IdCliente} sin cambios o ya en 'En Seguimiento' - manteniendo estado: {Estado}", 
                            lead.ID_Cliente, lead.Seguimiento);
                    }
                }

                _logger.LogInformation("Lead {IdCliente} - Estado final: {EstadoFinal}", lead.ID_Cliente, lead.Seguimiento);
                
                var resultado = await _clientesLeadsService.ModificarAsync(lead);

                if (resultado)
                {
                    return Json(new { success = true, message = "Lead modificado exitosamente" });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo modificar el lead" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al modificar lead: {IdCliente}", lead.ID_Cliente);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPropiedades()
        {
            try
            {
                var propiedades = await ObtenerListaPropiedades();
                return Json(propiedades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener propiedades");
                return Json(new List<object>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerImagenPropiedad(string idPropiedad)
        {
            try
            {
                _logger.LogInformation("Obteniendo imagen para propiedad: {IdPropiedad}", idPropiedad);

                if (string.IsNullOrWhiteSpace(idPropiedad))
                {
                    return Json(new { success = false, message = "ID de propiedad requerido" });
                }

                var imagenUrl = await ConstruirUrlGoogleDrive(idPropiedad);

                if (!string.IsNullOrEmpty(imagenUrl))
                {
                    _logger.LogInformation("URL de imagen encontrada: {Url}", imagenUrl);
                    return Json(new { success = true, imagenUrl = imagenUrl });
                }
                else
                {
                    _logger.LogWarning("No se encontró imagen para propiedad: {IdPropiedad}", idPropiedad);
                    return Json(new { success = false, message = "No se encontró imagen para esta propiedad" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener imagen de propiedad: {IdPropiedad}", idPropiedad);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerModalAgregar()
        {
            try
            {
                // Crear un objeto vacío para el modelo
                var leadVacio = new ClienteLead();

                // Cargar datos auxiliares para los combos
                ViewBag.Propiedades = await ObtenerListaPropiedades();
                ViewBag.Asistentes = await _clientesLeadsService.ObtenerAsistentesAsync();
                ViewBag.Portales = await _clientesLeadsService.ObtenerPortalesAsync();

                return PartialView("AgregarModal", leadVacio);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar modal de agregar");
                return Content($"<div class='alert alert-danger'>Error al cargar el formulario: {ex.Message}</div>");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Agregar(ClienteLead lead)
        {
            try
            {
                // Validar que los campos obligatorios estén presentes
                if (string.IsNullOrEmpty(lead.Nombres) ||
                    string.IsNullOrEmpty(lead.Apellidos) ||
                    string.IsNullOrEmpty(lead.Correo_Electronico))
                {
                    return Json(new { success = false, message = "Los campos Nombres, Apellidos y Correo Electrónico son obligatorios" });
                }

                var idGenerado = await _clientesLeadsService.AgregarAsync(lead);

                if (!string.IsNullOrEmpty(idGenerado))
                {
                    return Json(new { success = true, message = "Lead agregado exitosamente", id = idGenerado });
                }
                else
                {
                    return Json(new { success = false, message = "No se pudo agregar el lead" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar lead");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConvertirAClienteMatch(string idCliente)
        {
            try
            {
                _logger.LogInformation("=== INICIO ConvertirAClienteMatch ===");
                _logger.LogInformation("ID Cliente recibido: {IdCliente}", idCliente);

                if (string.IsNullOrWhiteSpace(idCliente))
                {
                    return Json(new { success = false, message = "ID del cliente es requerido" });
                }

                // Obtener el lead desde el servicio
                var lead = await _clientesLeadsService.ObtenerPorIdAsync(idCliente);
                if (lead == null)
                {
                    _logger.LogWarning("Lead no encontrado: {IdCliente}", idCliente);
                    return Json(new { success = false, message = $"Lead con ID '{idCliente}' no encontrado" });
                }

                _logger.LogInformation("Lead encontrado: {Nombres} {Apellidos}", lead.Nombres, lead.Apellidos);

                // Verificar que el lead no esté ya en estado RESERVA
                if (lead.Seguimiento == "RESERVA")
                {
                    _logger.LogWarning("Lead ya está en RESERVA: {IdCliente}", idCliente);
                    return Json(new { success = false, message = "Este lead ya fue convertido a Cliente Match" });
                }

                // Verificar si ya existe un Cliente Match con ese ID
                var yaExiste = await _clientesLeadsService.ExisteClienteMatchAsync(idCliente);
                if (yaExiste)
                {
                    _logger.LogWarning("Ya existe un Cliente Match con ID: {IdCliente}", idCliente);
                    return Json(new { success = false, message = $"Ya existe un Cliente Match con ID '{idCliente}'" });
                }

                _logger.LogInformation("Iniciando conversión a Cliente Match...");

                // Llamar al servicio para convertir el lead
                var resultadoConversion = await _clientesLeadsService.ConvertirAClienteMatchAsync(lead);

                if (resultadoConversion)
                {
                    _logger.LogInformation("Conversión exitosa, actualizando estado del lead a RESERVA...");

                    // Actualizar el estado del lead a RESERVA
                    lead.Seguimiento = "RESERVA";
                    var resultadoActualizacion = await _clientesLeadsService.ModificarAsync(lead);

                    if (resultadoActualizacion)
                    {
                        _logger.LogInformation("Lead actualizado a RESERVA exitosamente");
                        return Json(new {
                            success = true,
                            message = "Lead convertido exitosamente a Cliente Match"
                        });
                    }
                    else
                    {
                        _logger.LogWarning("Error al actualizar estado del lead");
                        return Json(new {
                            success = false,
                            message = "Cliente Match creado pero no se pudo actualizar el estado del lead"
                        });
                    }
                }
                else
                {
                    _logger.LogError("No se pudo crear el Cliente Match");
                    return Json(new {
                        success = false,
                        message = "No se pudo crear el Cliente Match"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ConvertirAClienteMatch: {IdCliente}", idCliente);
                return Json(new {
                    success = false,
                    message = $"Error interno: {ex.Message}"
                });
            }
            finally
            {
                _logger.LogInformation("=== FIN ConvertirAClienteMatch ===");
            }
        }

        // Método privado auxiliar para obtener la lista de propiedades usando SP
        private async Task<List<object>> ObtenerListaPropiedades()
        {
            try
            {
                var propiedades = new List<object>();
                
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    
                    using (var command = new SqlCommand("PP_psnp_Propiedad_Combo", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                propiedades.Add(new
                                {
                                    ID_Propiedad = reader["ID_Propiedad"].ToString(),
                                    Titulo = reader["Titulo"].ToString()
                                });
                            }
                        }
                    }
                }
                
                _logger.LogInformation("Se obtuvieron {Count} propiedades del combo", propiedades.Count);
                return propiedades;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener lista de propiedades");
                return new List<object>();
            }
        }

        // Método para construir la URL de Google Drive basándose en la tabla Propiedades
        private async Task<string> ConstruirUrlGoogleDrive(string idPropiedad)
        {
            try
            {
                _logger.LogInformation("Consultando tabla Propiedades para obtener URL de imagen: {IdPropiedad}", idPropiedad);

                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();
                    
                    // Consultar la URL de la imagen para esta propiedad
                    var selectQuery = "SELECT Url_Imagen FROM Propiedades WHERE ID_Propiedad = @ID_Propiedad";
                    using (var command = new SqlCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ID_Propiedad", idPropiedad);

                        var result = await command.ExecuteScalarAsync();
                        if (result != null && !string.IsNullOrEmpty(result.ToString()))
                        {
                            var imagenUrl = result.ToString();
                            _logger.LogInformation("URL de imagen encontrada en BD: {Url}", imagenUrl);

                            // Si la URL ya es completa de Google Drive, convertirla a formato de visualización
                            if (imagenUrl.Contains("drive.google.com"))
                            {
                                return ConvertirUrlGoogleDrive(imagenUrl);
                            }
                            // Si es solo el ID de Google Drive, construir la URL completa
                            else if (!string.IsNullOrEmpty(imagenUrl))
                            {
                                return $"https://drive.google.com/thumbnail?id={imagenUrl}&sz=w800";
                            }
                        }
                    }
                }

                _logger.LogWarning("No se encontró URL de imagen en BD para la propiedad: {IdPropiedad}", idPropiedad);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al consultar tabla Propiedades: {IdPropiedad}", idPropiedad);
                return null;
            }
        }

        // Método para convertir URL de compartir de Google Drive a URL de visualización directa
        private string ConvertirUrlGoogleDrive(string urlCompartir)
        {
            try
            {
                // URL de compartir: https://drive.google.com/file/d/FILE_ID/view?usp=sharing
                // URL de thumbnail: https://drive.google.com/thumbnail?id=FILE_ID&sz=w800
                // URL de visualización: https://drive.google.com/uc?export=view&id=FILE_ID

                if (string.IsNullOrEmpty(urlCompartir))
                    return null;

                // Extraer el ID del archivo de la URL
                string fileId = null;

                // Patrón 1: https://drive.google.com/file/d/FILE_ID/view
                if (urlCompartir.Contains("/file/d/"))
                {
                    var startIndex = urlCompartir.IndexOf("/file/d/") + 8;
                    var endIndex = urlCompartir.IndexOf("/", startIndex);
                    if (endIndex == -1)
                        endIndex = urlCompartir.IndexOf("?", startIndex);
                    if (endIndex == -1)
                        endIndex = urlCompartir.Length;

                    fileId = urlCompartir.Substring(startIndex, endIndex - startIndex);
                }
                // Patrón 2: https://drive.google.com/open?id=FILE_ID
                else if (urlCompartir.Contains("id="))
                {
                    var startIndex = urlCompartir.IndexOf("id=") + 3;
                    var endIndex = urlCompartir.IndexOf("&", startIndex);
                    if (endIndex == -1)
                        endIndex = urlCompartir.Length;

                    fileId = urlCompartir.Substring(startIndex, endIndex - startIndex);
                }

                if (!string.IsNullOrEmpty(fileId))
                {
                    // Usar formato de thumbnail para mejor compatibilidad
                    var urlConvertida = $"https://drive.google.com/thumbnail?id={fileId}&sz=w800";
                    _logger.LogInformation("URL convertida: {Url}", urlConvertida);
                    return urlConvertida;
                }

                _logger.LogWarning("No se pudo extraer el ID del archivo de la URL: {Url}", urlCompartir);
                return urlCompartir; // Retornar la URL original si no se pudo convertir
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al convertir URL de Google Drive: {Url}", urlCompartir);
                return urlCompartir;
            }
        }

        // ===================================================
        // MÉTODOS PARA SEGUIMIENTO ACTIVO
        // ===================================================

        [HttpPost]
        public async Task<IActionResult> RegistrarAccionAgente([FromForm] string Agente, [FromForm] string ID_Cliente, 
            [FromForm] string Descripcion_Accion, [FromForm] string Tipo_Accion, [FromForm] string Resultado, 
            [FromForm] string? ID_Propiedad, [FromForm] string? Fecha_Proximo_Contacto)
        {
            try
            {
                _logger.LogInformation("Registrando acción del agente para cliente: {IdCliente}", ID_Cliente);

                var seguimiento = new SeguimientoActivo
                {
                    ID_Cliente = ID_Cliente,
                    ID_Propiedad = ID_Propiedad,
                    Agente = Agente,
                    Codigo_Agente = null,
                    Fecha_Accion = DateTime.Now,
                    Tipo_Accion = Tipo_Accion,
                    Descripcion_Accion = Descripcion_Accion,
                    Resultado = Resultado,
                    Estado = Resultado, // El estado es el mismo que el resultado
                    Fecha_Proximo_Contacto = string.IsNullOrEmpty(Fecha_Proximo_Contacto) ? null : DateTime.Parse(Fecha_Proximo_Contacto)
                };

                var seguimientoService = HttpContext.RequestServices.GetService<SeguimientoActivoService>();
                if (seguimientoService == null)
                {
                    return Json(new { status = "error", message = "Servicio de seguimiento no disponible" });
                }

                var resultado = await seguimientoService.AgregarAsync(seguimiento);

                if (resultado)
                {
                    _logger.LogInformation("Acción registrada exitosamente para cliente: {IdCliente}", ID_Cliente);
                    
                    // Si el lead está en estado "Nuevo", cambiarlo automáticamente a "En Seguimiento"
                    try
                    {
                        var leadActual = await _clientesLeadsService.ObtenerPorIdAsync(ID_Cliente);
                        if (leadActual != null && leadActual.Seguimiento == "Nuevo")
                        {
                            leadActual.Seguimiento = "En Seguimiento";
                            await _clientesLeadsService.ModificarAsync(leadActual);
                            _logger.LogInformation("Lead {IdCliente} cambiado automáticamente de 'Nuevo' a 'En Seguimiento' al registrar acción", ID_Cliente);
                        }
                    }
                    catch (Exception exUpdate)
                    {
                        _logger.LogWarning(exUpdate, "No se pudo actualizar el estado del lead {IdCliente} después de registrar la acción", ID_Cliente);
                        // No fallar si no se puede actualizar el estado, la acción ya se guardó
                    }
                    
                    return Json(new { status = "success", message = "Acción registrada correctamente" });
                }
                else
                {
                    return Json(new { status = "error", message = "No se pudo registrar la acción" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar acción del agente");
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerAccionesCliente(string idCliente)
        {
            try
            {
                _logger.LogInformation("Obteniendo acciones del cliente: {IdCliente}", idCliente);

                var seguimientoService = HttpContext.RequestServices.GetService<SeguimientoActivoService>();
                if (seguimientoService == null)
                {
                    return Json(new List<SeguimientoActivo>());
                }

                var acciones = await seguimientoService.ObtenerPorClienteAsync(idCliente);

                _logger.LogInformation("Se encontraron {Count} acciones para el cliente: {IdCliente}", acciones.Count, idCliente);
                return Json(acciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener acciones del cliente: {IdCliente}", idCliente);
                return Json(new List<SeguimientoActivo>());
            }
        }
    }
}
