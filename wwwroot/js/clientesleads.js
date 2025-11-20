// ===================================================
// VARIABLES GLOBALES
// ===================================================
var tablaLeads;
var tablaMisLeads;
var esAdministrador = false;
var esAgente = false;
var usuarioRol = '';

// ===================================================
// INICIALIZACIÓN AL CARGAR EL DOCUMENTO
// ===================================================
$(document).ready(function() {
    console.log('=== Inicializando ClientesLeads ===');
    
    // Obtener rol del usuario desde el data attribute del div principal o del ViewData
    var elementoRol = $('[data-usuario-rol]');
    console.log('Elemento con data-usuario-rol encontrado:', elementoRol.length > 0);
    console.log('Valor del data-usuario-rol:', elementoRol.attr('data-usuario-rol'));
    
    usuarioRol = elementoRol.data('usuario-rol') || '';
    esAdministrador = (usuarioRol === 'Administrador');
    esAgente = (usuarioRol === 'Agente');
    
    console.log('Usuario Rol:', usuarioRol);
    console.log('Es Administrador:', esAdministrador);
    console.log('Es Agente:', esAgente);
    
    // Verificación adicional de seguridad
    if (usuarioRol === '') {
        console.warn('⚠️ ADVERTENCIA: No se pudo obtener el rol del usuario. Se asumirá como no administrador.');
    }
    
    // Inicializar DataTables
    inicializarDataTables();
    
    // Cargar filtros
    cargarPortales();
    cargarAsistentes();
    
    // Event listeners
    configurarEventos();
});

// ===================================================
// INICIALIZAR DATATABLES
// ===================================================
function inicializarDataTables() {
    // Configuración común para ambas tablas
    var configComun = {
        "processing": true,
        "serverSide": true,
        "order": [[7, "desc"]], // Ordenar por fecha de creación descendente
        "pageLength": 10,
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json"
        },
        "columns": [
            { "data": "iD_Cliente", "name": "ID_Cliente" },
            {
                "data": null,
                "orderable": false,
                "className": "columna-imagen",
                "render": function(data, type, row) {
                    if (!row.iD_Unidad_Consultada || row.iD_Unidad_Consultada === '') {
                        return '<div class="no-imagen-tabla" title="No hay propiedad consultada"><i class="fas fa-home"></i></div>';
                    }
                    
                    if (row.imagen_Propiedad && row.imagen_Propiedad !== '') {
                        return '<img src="' + row.imagen_Propiedad + '" class="propiedad-imagen-tabla" alt="Imagen de propiedad" title="Propiedad: ' + (row.unidad_Consultada || row.iD_Unidad_Consultada) + '" onerror="this.parentNode.innerHTML=\'<div class=\\\'no-imagen-tabla\\\' title=\\\'Imagen no disponible\\\'><i class=\\\'fas fa-image\\\'></i></div>\';">';
                    }
                    
                    var containerId = 'img-container-' + row.iD_Cliente.replace(/[^a-zA-Z0-9]/g, '_');
                    return '<div id="' + containerId + '" class="imagen-cargando-tabla" title="Cargando imagen..."><i class="fas fa-image"></i></div>';
                }
            },
            {
                "data": null,
                "render": function(data, type, row) {
                    return (row.nombres || '') + ' ' + (row.apellidos || '');
                }
            },
            { "data": "correo_Electronico" },
            { "data": "telefono" },
            { "data": "portal" },
            {
                "data": "seguimiento",
                "render": function(data, type, row) {
                    if (type === 'type' || type === 'sort') {
                        return data || '';
                    }
                    
                    if (!data) return '<span class="badge-estado badge-nuevo">Sin Estado</span>';
                    
                    var badgeClass = '';
                    
                    if (data === 'Nuevo') {
                        badgeClass = 'badge-nuevo';
                    } else if (data === 'En Seguimiento') {
                        badgeClass = 'badge-contactado';
                    } else if (data === 'Con Visita Programada') {
                        badgeClass = 'badge-calificado';
                    } else if (data === 'En Espera') {
                        badgeClass = 'badge-perdido';
                    } else if (data === 'Terminado') {
                        badgeClass = 'badge-sin-seguimiento';
                    } else if (data === 'RESERVA') {
                        badgeClass = 'badge-reserva';
                    } else {
                        badgeClass = 'badge-nuevo';
                    }
                    
                    var displayText = data.length > 20 ? data.substring(0, 20) + '...' : data;
                    return '<span class="badge-estado ' + badgeClass + '" title="' + data + '">' + displayText + '</span>';
                }
            },
            {
                "data": "creado",
                "render": function(data, type, row) {
                    if (type === 'display' && data) {
                        var fecha = new Date(data);
                        return fecha.toLocaleDateString('es-ES') + ' ' + fecha.toLocaleTimeString('es-ES');
                    }
                    return data;
                }
            },
            { "data": "respuesta" },
            {
                "data": null,
                "orderable": false,
                "render": function(data, type, row) {
                    var botones = '<div class="action-buttons">';
                    
                    // Verificar si el lead está en estado RESERVA
                    var esReserva = row.seguimiento === 'RESERVA';
                    
                    // En "Todos los Leads", solo administradores pueden usar los botones
                    if (esAdministrador) {
                        if (esReserva) {
                            // Lead en RESERVA - botones deshabilitados con mensaje informativo
                            botones += '<i class="fas fa-exchange-alt disabled" style="margin-right: 8px;" title="Este lead ya fue convertido a Cliente Match" onclick="mostrarMensajeReserva()"></i>' +
                                      '<i class="fas fa-edit disabled" title="Este lead ya fue convertido a Cliente Match y no se puede modificar" onclick="mostrarMensajeReserva()"></i>' +
                                      '<i class="fas fa-trash-alt disabled" title="Este lead ya fue convertido a Cliente Match y no se puede eliminar" onclick="mostrarMensajeReserva()"></i>';
                        } else {
                            // Lead normal - botones habilitados
                            botones += '<i class="fas fa-exchange-alt" onclick="convertirAMatch(\'' + row.iD_Cliente + '\')" title="Convertir a Cliente Match" style="color: #ff9800; margin-right: 8px;"></i>' +
                                      '<i class="fas fa-edit" onclick="modificarLead(\'' + row.iD_Cliente + '\')" title="Editar Lead"></i>' +
                                      '<i class="fas fa-trash-alt" onclick="eliminarLead(\'' + row.iD_Cliente + '\')" title="Eliminar Lead"></i>';
                        }
                    } else {
                        if (esReserva) {
                            botones += '<i class="fas fa-exchange-alt disabled" style="margin-right: 8px;" title="Este lead ya fue convertido a Cliente Match" onclick="mostrarMensajeReserva()"></i>' +
                                      '<i class="fas fa-edit disabled" title="Este lead ya fue convertido a Cliente Match y no se puede modificar" onclick="mostrarMensajeReserva()"></i>' +
                                      '<i class="fas fa-trash-alt disabled" title="Este lead ya fue convertido a Cliente Match y no se puede eliminar" onclick="mostrarMensajeReserva()"></i>';
                        } else {
                            // Para agentes, mostrar botones deshabilitados con mensaje informativo normal
                            botones += '<i class="fas fa-exchange-alt disabled" style="margin-right: 8px;" title="Solo administradores pueden convertir leads en esta vista" onclick="mostrarMensajePermiso(\'convertir\')"></i>' +
                                      '<i class="fas fa-edit disabled" title="Solo administradores pueden editar leads en esta vista" onclick="mostrarMensajePermiso(\'editar\')"></i>' +
                                      '<i class="fas fa-trash-alt disabled" title="Solo administradores pueden eliminar leads" onclick="mostrarMensajePermiso(\'eliminar\')"></i>';
                        }
                    }
                    
                    botones += '</div>';
                    return botones;
                }
            }
        ]
    };
    
    // Inicializar tabla "Todos los Leads"
    tablaLeads = $('#tablaLeads').DataTable({
        ...configComun,
        "ajax": {
            "url": "/ClientesLeads/GetData",
            "type": "POST"
        }
    });
    
    // Configuración específica para "Mis Leads" con botones diferentes
    var configMisLeads = {
        "processing": true,
        "serverSide": true,
        "order": [[7, "desc"]],
        "pageLength": 10,
        "language": {
            "url": "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json"
        },
        "columns": [
            { "data": "iD_Cliente", "name": "ID_Cliente" },
            {
                "data": null,
                "orderable": false,
                "className": "columna-imagen",
                "render": function(data, type, row) {
                    if (!row.iD_Unidad_Consultada || row.iD_Unidad_Consultada === '') {
                        return '<div class="no-imagen-tabla" title="No hay propiedad consultada"><i class="fas fa-home"></i></div>';
                    }
                    
                    if (row.imagen_Propiedad && row.imagen_Propiedad !== '') {
                        return '<img src="' + row.imagen_Propiedad + '" class="propiedad-imagen-tabla" alt="Imagen de propiedad" title="Propiedad: ' + (row.unidad_Consultada || row.iD_Unidad_Consultada) + '" onerror="this.parentNode.innerHTML=\'<div class=\\\'no-imagen-tabla\\\' title=\\\'Imagen no disponible\\\'><i class=\\\'fas fa-image\\\'></i></div>\';">';
                    }
                    
                    var containerId = 'img-container-' + row.iD_Cliente.replace(/[^a-zA-Z0-9]/g, '_');
                    return '<div id="' + containerId + '" class="imagen-cargando-tabla" title="Cargando imagen..."><i class="fas fa-image"></i></div>';
                }
            },
            {
                "data": null,
                "render": function(data, type, row) {
                    return (row.nombres || '') + ' ' + (row.apellidos || '');
                }
            },
            { "data": "correo_Electronico" },
            { "data": "telefono" },
            { "data": "portal" },
            {
                "data": "seguimiento",
                "render": function(data, type, row) {
                    if (type === 'type' || type === 'sort') {
                        return data || '';
                    }
                    
                    if (!data) return '<span class="badge-estado badge-nuevo">Sin Estado</span>';
                    
                    var badgeClass = '';
                    
                    if (data === 'Nuevo') {
                        badgeClass = 'badge-nuevo';
                    } else if (data === 'En Seguimiento') {
                        badgeClass = 'badge-contactado';
                    } else if (data === 'Con Visita Programada') {
                        badgeClass = 'badge-calificado';
                    } else if (data === 'En Espera') {
                        badgeClass = 'badge-perdido';
                    } else if (data === 'Terminado') {
                        badgeClass = 'badge-sin-seguimiento';
                    } else if (data === 'RESERVA') {
                        badgeClass = 'badge-reserva';
                    } else {
                        badgeClass = 'badge-nuevo';
                    }
                    
                    var displayText = data.length > 20 ? data.substring(0, 20) + '...' : data;
                    return '<span class="badge-estado ' + badgeClass + '" title="' + data + '">' + displayText + '</span>';
                }
            },
            {
                "data": "creado",
                "render": function(data, type, row) {
                    if (type === 'display' && data) {
                        var fecha = new Date(data);
                        return fecha.toLocaleDateString('es-ES') + ' ' + fecha.toLocaleTimeString('es-ES');
                    }
                    return data;
                }
            },
            { "data": "respuesta" },
            {
                "data": null,
                "orderable": false,
                "render": function(data, type, row) {
                    var botones = '<div class="action-buttons">';
                    
                    // Verificar si el lead está en estado RESERVA
                    var esReserva = row.seguimiento === 'RESERVA';
                    
                    if (esReserva) {
                        // Lead en RESERVA - botones deshabilitados para todos
                        botones += '<i class="fas fa-exchange-alt disabled" style="margin-right: 8px;" title="Este lead ya fue convertido a Cliente Match" onclick="mostrarMensajeReserva()"></i>' +
                                  '<i class="fas fa-edit disabled" title="Este lead ya fue convertido a Cliente Match y no se puede modificar" onclick="mostrarMensajeReserva()"></i>';
                        
                        // Solo administradores pueden eliminar, pero en estado RESERVA tampoco
                        if (esAdministrador) {
                            botones += '<i class="fas fa-trash-alt disabled" title="Este lead ya fue convertido a Cliente Match y no se puede eliminar" onclick="mostrarMensajeReserva()"></i>';
                        }
                    } else {
                        // Lead normal - botones habilitados
                        botones += '<i class="fas fa-exchange-alt" onclick="convertirAMatch(\'' + row.iD_Cliente + '\')" title="Convertir a Cliente Match" style="color: #ff9800; margin-right: 8px;"></i>' +
                                  '<i class="fas fa-edit" onclick="modificarLead(\'' + row.iD_Cliente + '\')" title="Editar Lead"></i>';
                        
                        // Solo administradores pueden eliminar
                        if (esAdministrador) {
                            botones += '<i class="fas fa-trash-alt" onclick="eliminarLead(\'' + row.iD_Cliente + '\')" title="Eliminar Lead"></i>';
                        }
                    }
                    
                    botones += '</div>';
                    return botones;
                }
            }
        ],
        "ajax": {
            "url": "/ClientesLeads/GetDataMisLeads",
            "type": "POST"
        }
    };
    
    // Inicializar tabla "Mis Leads"
    tablaMisLeads = $('#tablaMisLeads').DataTable(configMisLeads);
    
    console.log('DataTables inicializadas correctamente');
}

// ===================================================
// CARGAR FILTROS
// ===================================================
function cargarPortales() {
    $.ajax({
        url: '/ClientesLeads/Portales',
        type: 'GET',
        success: function(response) {
            if (response.success && response.data) {
                var select = $('#filtroPortal');
                select.empty();
                select.append('<option value="">Todos los portales</option>');
                
                response.data.forEach(function(portal) {
                    select.append('<option value="' + portal + '">' + portal + '</option>');
                });
            }
        },
        error: function(xhr, status, error) {
            console.error('Error al cargar portales:', error);
        }
    });
}

function cargarAsistentes() {
    $.ajax({
        url: '/ClientesLeads/Asistentes',
        type: 'GET',
        success: function(response) {
            if (response.success && response.data) {
                var select = $('#filtroAsistente');
                select.empty();
                select.append('<option value="">Todos los asistentes</option>');
                
                response.data.forEach(function(asistente) {
                    select.append('<option value="' + asistente + '">' + asistente + '</option>');
                });
            }
        },
        error: function(xhr, status, error) {
            console.error('Error al cargar asistentes:', error);
        }
    });
}

// ===================================================
// CONFIGURAR EVENTOS
// ===================================================
function configurarEventos() {
    // Cambio de filtros
    $('#filtroPortal, #filtroAsistente, #filtroEstado').on('change', function() {
        if (tablaLeads) {
            tablaLeads.ajax.reload();
        }
        if (tablaMisLeads) {
            tablaMisLeads.ajax.reload();
        }
    });
    
    // Botón agregar nuevo lead
    $('#btnNuevoLead').on('click', function() {
        mostrarModalNuevoLead();
    });
}

// ===================================================
// FUNCIONES DE ACCIÓN
// ===================================================
function verDetalle(idCliente) {
    console.log('Ver detalle del lead:', idCliente);
    
    $.ajax({
        url: '/ClientesLeads/Detalle/' + idCliente,
        type: 'GET',
        success: function(response) {
            if (response.success && response.data) {
                mostrarModalDetalle(response.data);
            } else {
                alert('Error al obtener el detalle: ' + (response.message || 'Error desconocido'));
            }
        },
        error: function(xhr, status, error) {
            console.error('Error al obtener detalle:', error);
            alert('Error al obtener el detalle del lead');
        }
    });
}

function editarLead(idCliente) {
    console.log('Editar lead:', idCliente);
    
    if (!esAdministrador) {
        alert('No tienes permisos para editar leads');
        return;
    }
    
    $.ajax({
        url: '/ClientesLeads/Detalle/' + idCliente,
        type: 'GET',
        success: function(response) {
            if (response.success && response.data) {
                mostrarModalEditar(response.data);
            } else {
                alert('Error al obtener los datos: ' + (response.message || 'Error desconocido'));
            }
        },
        error: function(xhr, status, error) {
            console.error('Error al obtener datos para editar:', error);
            alert('Error al cargar los datos del lead');
        }
    });
}

function eliminarLead(idCliente) {
    console.log('Eliminar lead:', idCliente);
    
    if (!esAdministrador) {
        alert('No tienes permisos para eliminar leads');
        return;
    }
    
    if (!confirm('¿Está seguro de que desea eliminar este lead?')) {
        return;
    }
    
    $.ajax({
        url: '/ClientesLeads/Eliminar',
        type: 'POST',
        data: { id: idCliente },
        success: function(response) {
            if (response.success) {
                alert('Lead eliminado exitosamente');
                
                // Recargar ambas tablas
                if (tablaLeads) {
                    tablaLeads.ajax.reload();
                }
                if (tablaMisLeads) {
                    tablaMisLeads.ajax.reload();
                }
            } else {
                alert('Error al eliminar el lead: ' + (response.message || 'Error desconocido'));
            }
        },
        error: function(xhr, status, error) {
            console.error('Error al eliminar lead:', error);
            alert('Error al eliminar el lead');
        }
    });
}

// ===================================================
// MODALES (Placeholder - implementar según necesidad)
// ===================================================
function mostrarModalDetalle(lead) {
    // TODO: Implementar modal de detalle
    console.log('Mostrar modal de detalle:', lead);
    alert('Detalle del lead:\n\n' + 
          'ID: ' + lead.id_Cliente + '\n' +
          'Nombre: ' + (lead.nombres || '') + ' ' + (lead.apellidos || '') + '\n' +
          'Email: ' + (lead.correo_Electronico || 'N/A') + '\n' +
          'Teléfono: ' + (lead.telefono || 'N/A') + '\n' +
          'Portal: ' + (lead.portal || 'N/A') + '\n' +
          'Estado: ' + (lead.seguimiento || 'N/A'));
}

function mostrarModalEditar(lead) {
    // TODO: Implementar modal de edición
    console.log('Mostrar modal de edición:', lead);
    alert('Función de edición - Implementar modal');
}

function mostrarModalNuevoLead() {
    // TODO: Implementar modal para nuevo lead
    console.log('Mostrar modal para nuevo lead');
    alert('Función agregar nuevo lead - Implementar modal');
}

// ===================================================
// UTILIDADES
// ===================================================
function recargarTablas() {
    if (tablaLeads) {
        tablaLeads.ajax.reload();
    }
    if (tablaMisLeads) {
        tablaMisLeads.ajax.reload();
    }
}

// ===================================================
// FUNCIONALIDAD DE BOTÓN TOGGLE DE ESTADÍSTICAS
// ===================================================
$('#toggleStats').click(function() {
    var statsContainer = $('#statsContainer');
    var btn = $(this);
    var tooltip = $('#toggleTooltip');
    
    if (statsContainer.hasClass('hidden')) {
        statsContainer.removeClass('hidden').addClass('visible');
        btn.removeClass('collapsed');
        tooltip.text('Ocultar Estadísticas');
    } else {
        statsContainer.removeClass('visible').addClass('hidden');
        btn.addClass('collapsed');
        tooltip.text('Mostrar Estadísticas');
    }
});

// ===================================================
// CARGAR ESTADÍSTICAS Y GRÁFICOS
// ===================================================
var misLeadsChart = null;

function cargarEstadisticasMisLeads() {
    $.ajax({
        url: '/ClientesLeads/Estadisticas',
        type: 'GET',
        success: function(response) {
            if (response.success && response.data) {
                var datos = response.data;
                
                // Valores por defecto si no vienen del backend
                var total = datos.total || 0;
                var nuevo = datos.nuevo || 0;
                var enSeguimiento = datos.enSeguimiento || 0;
                var conVisita = datos.conVisitaProgramada || 0;
                var enEspera = datos.enEspera || 0;
                var terminado = datos.terminado || 0;
                
                // Actualizar los valores
                $('#totalMisLeads').text(total);
                $('#misLeadsNuevo').text(nuevo);
                $('#misLeadsEnSeguimiento').text(enSeguimiento);
                $('#misLeadsConVisita').text(conVisita);
                $('#misLeadsEnEspera').text(enEspera);
                $('#misLeadsTerminado').text(terminado);
                
                // Animar las barras de progreso
                var porcentajeNuevo = total > 0 ? (nuevo / total) * 100 : 0;
                var porcentajeEnSeguimiento = total > 0 ? (enSeguimiento / total) * 100 : 0;
                var porcentajeConVisita = total > 0 ? (conVisita / total) * 100 : 0;
                var porcentajeEnEspera = total > 0 ? (enEspera / total) * 100 : 0;
                var porcentajeTerminado = total > 0 ? (terminado / total) * 100 : 0;
                
                setTimeout(function() {
                    $('#progressMisLeadsNuevo').css('width', porcentajeNuevo + '%');
                    $('#progressMisLeadsEnSeguimiento').css('width', porcentajeEnSeguimiento + '%');
                    $('#progressMisLeadsConVisita').css('width', porcentajeConVisita + '%');
                    $('#progressMisLeadsEnEspera').css('width', porcentajeEnEspera + '%');
                    $('#progressMisLeadsTerminado').css('width', porcentajeTerminado + '%');
                }, 200);
                
                // Crear gráfico de torta
                if (typeof Chart !== 'undefined') {
                    var ctx = document.getElementById('misLeadsChart');
                    if (ctx) {
                        if (misLeadsChart) {
                            misLeadsChart.destroy();
                        }
                        
                        misLeadsChart = new Chart(ctx, {
                            type: 'doughnut',
                            data: {
                                labels: ['Nuevo', 'En Seguimiento', 'Con Visita', 'En Espera', 'Terminado'],
                                datasets: [{
                                    data: [nuevo, enSeguimiento, conVisita, enEspera, terminado],
                                    backgroundColor: [
                                        '#3498db',
                                        '#f39c12',
                                        '#27ae60',
                                        '#e67e22',
                                        '#95a5a6'
                                    ],
                                    borderColor: '#1D1D1D',
                                    borderWidth: 2
                                }]
                            },
                            options: {
                                responsive: true,
                                maintainAspectRatio: false,
                                plugins: {
                                    legend: {
                                        position: 'bottom',
                                        labels: {
                                            color: '#ffffff',
                                            font: {
                                                size: 10
                                            },
                                            padding: 8
                                        }
                                    }
                                }
                            }
                        });
                    }
                }
            }
        },
        error: function(xhr, status, error) {
            console.error('Error al cargar estadísticas:', error);
            // Mostrar valores en 0 en caso de error
            $('#totalMisLeads').text(0);
            $('#misLeadsNuevo').text(0);
            $('#misLeadsEnSeguimiento').text(0);
            $('#misLeadsConVisita').text(0);
            $('#misLeadsEnEspera').text(0);
            $('#misLeadsTerminado').text(0);
        }
    });
}

// Cargar estadísticas al inicializar
setTimeout(function() {
    cargarEstadisticasMisLeads();
}, 500);

// ===================================================
// FUNCIONES PARA MODALES
// ===================================================
function abrirModalAgregar() {
    console.log('➕ Abriendo modal para agregar nuevo lead');
    
    if (!esAdministrador) {
        alert('Acceso Denegado\n\nSolo los administradores pueden agregar nuevos leads.');
        return;
    }
    
    // Mostrar indicador de carga en el modal
    var $modalBody = $('#modalAgregarLead .modal-body');
    $modalBody.html(`
        <div class="text-center" style="padding: 40px;">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p style="margin-top: 15px; color: white;">Cargando formulario...</p>
        </div>
    `);
    
    // Abrir el modal
    var modal = new bootstrap.Modal(document.getElementById('modalAgregarLead'));
    modal.show();
    
    // Cargar el contenido del modal desde el servidor
    $.ajax({
        url: '/ClientesLeads/ObtenerModalAgregar',
        type: 'GET',
        success: function(html) {
            console.log('✅ Contenido del modal cargado');
            $modalBody.html(html);
            
            // Inicializar el botón de guardar
            inicializarBotonGuardarLead();
        },
        error: function(xhr, status, error) {
            console.error('❌ Error al cargar el modal:', error);
            $modalBody.html(`
                <div class="alert alert-danger" role="alert">
                    <i class="fas fa-exclamation-triangle"></i>
                    <strong>Error al cargar el formulario</strong>
                    <p>No se pudo cargar el formulario. Por favor, intente nuevamente.</p>
                    <p><small>Error: ${error}</small></p>
                </div>
            `);
        }
    });
}

// Función para inicializar el botón de guardar nuevo lead
function inicializarBotonGuardarLead() {
    $('#btnGuardarLead').off('click').on('click', function() {
        console.log('💾 Guardando nuevo lead');
        
        var $btn = $(this);
        var btnTextoOriginal = $btn.html();
        
        // Validar formulario
        if (typeof window.validateLeadForm === 'function') {
            if (!window.validateLeadForm()) {
                console.log('⚠️ Formulario inválido');
                return;
            }
        }
        
        // Validar campos obligatorios
        var errores = [];
        
        var nombres = $('#nombres').val().trim();
        var apellidos = $('#apellidos').val().trim();
        var correo = $('#correo').val().trim();
        
        if (!nombres) {
            errores.push('El campo Nombres es obligatorio');
        }
        
        if (!apellidos) {
            errores.push('El campo Apellidos es obligatorio');
        }
        
        if (!correo) {
            errores.push('El campo Correo Electrónico es obligatorio');
        } else {
            // Validar formato de correo
            var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(correo)) {
                errores.push('El formato del correo electrónico no es válido');
            }
        }
        
        if (errores.length > 0) {
            alert('❌ Errores de validación:\n\n' + errores.join('\n'));
            return;
        }
        
        // Deshabilitar botón durante el proceso
        $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Guardando...');
        
        // Recolectar datos del formulario
        var datos = {
            Nombres: nombres,
            Apellidos: apellidos,
            Correo_Electronico: correo,
            Telefono: $('#telefono').val().trim(),
            Sexo: $('#sexo').val(),
            Fecha_Contacto: $('#fechaContacto').val(),
            Portal: $('#portal').val(),
            Asistente: $('#asistente').val(),
            ID_Unidad_Consultada: $('#ddlPropiedad').val(),
            Unidad_Consultada: $('#hdnUnidadConsultada').val(),
            Seguimiento: $('#seguimiento').val().trim(),
            Respuesta: $('#respuesta').val().trim()
        };
        
        console.log('📤 Enviando datos:', datos);
        
        // Enviar datos al servidor
        $.ajax({
            url: '/ClientesLeads/Agregar',
            type: 'POST',
            data: datos,
            success: function(response) {
                console.log('📥 Respuesta recibida:', response);
                
                if (response.success) {
                    alert('✅ Lead agregado exitosamente');
                    
                    // Cerrar modal
                    bootstrap.Modal.getInstance(document.getElementById('modalAgregarLead')).hide();
                    
                    // Recargar tablas y estadísticas
                    recargarTablas();
                    cargarEstadisticasMisLeads();
                } else {
                    alert('❌ Error al agregar el lead: ' + (response.message || 'Error desconocido'));
                    $btn.prop('disabled', false).html(btnTextoOriginal);
                }
            },
            error: function(xhr, status, error) {
                console.error('❌ Error al agregar lead:', error);
                alert('❌ Error de conexión al agregar el lead');
                $btn.prop('disabled', false).html(btnTextoOriginal);
            }
        });
    });
}

function mostrarMensajeReserva() {
    alert('Este lead ya fue convertido a Cliente Match y no puede ser modificado o eliminado.');
}

function mostrarMensajePermiso(accion) {
    var mensaje = '';
    switch(accion) {
        case 'convertir':
            mensaje = 'Solo los administradores pueden convertir leads a Cliente Match en la vista "Todos los Leads".';
            break;
        case 'editar':
            mensaje = 'Solo los administradores pueden editar leads en la vista "Todos los Leads".';
            break;
        case 'eliminar':
            mensaje = 'Solo los administradores pueden eliminar leads.';
            break;
        default:
            mensaje = 'No tienes permisos para realizar esta acción.';
    }
    alert(mensaje);
}

// Variable global para almacenar el lead a convertir
var leadAConvertir = null;

// Función para abrir el modal de conversión a cliente match
function convertirAMatch(idCliente) {
    console.log('🔄 Abriendo modal de conversión para ID:', idCliente);
    
    if (!esAdministrador) {
        mostrarMensajePermiso('convertir');
        return;
    }
    
    // Buscar los datos del lead en las tablas activas
    var leadData = null;
    
    // Buscar en la tabla de todos los leads
    if (typeof tablaLeads !== 'undefined' && tablaLeads) {
        var rows = tablaLeads.rows().data();
        for (var i = 0; i < rows.length; i++) {
            if (rows[i].iD_Cliente == idCliente) {
                leadData = rows[i];
                console.log('✅ Lead encontrado en tabla Todos los Leads');
                break;
            }
        }
    }
    
    // Si no se encontró, buscar en la tabla de mis leads
    if (!leadData && typeof tablaMisLeads !== 'undefined' && tablaMisLeads) {
        var rows = tablaMisLeads.rows().data();
        for (var i = 0; i < rows.length; i++) {
            if (rows[i].iD_Cliente == idCliente) {
                leadData = rows[i];
                console.log('✅ Lead encontrado en tabla Mis Leads');
                break;
            }
        }
    }
    
    if (leadData) {
        // Verificar si ya está en estado RESERVA
        if (leadData.seguimiento === 'RESERVA') {
            alert('⚠️ Este lead ya fue convertido a Cliente Match\n\nNo se puede realizar esta acción nuevamente.');
            return;
        }
        
        // Almacenar los datos globalmente
        leadAConvertir = leadData;
        
        // Llenar el modal con los datos del lead
        $('#modalMatch_IdCliente').text(leadData.iD_Cliente || 'N/A');
        $('#modalMatch_Portal').text(leadData.portal || 'N/A');
        $('#modalMatch_Nombres').text(leadData.nombres || 'N/A');
        $('#modalMatch_Apellidos').text(leadData.apellidos || 'N/A');
        $('#modalMatch_Email').text(leadData.correo_Electronico || 'N/A');
        $('#modalMatch_Telefono').text(leadData.telefono || 'N/A');
        $('#modalMatch_Estado').text(leadData.seguimiento || 'Sin Estado');
        
        // Mostrar el modal
        var modal = new bootstrap.Modal(document.getElementById('modalConvertirMatch'));
        modal.show();
        
        console.log('✅ Modal de conversión abierto con datos del lead');
    } else {
        console.error('❌ No se encontraron datos del lead');
        alert('❌ No se pudieron obtener los datos del lead.\n\nPor favor, recargue la página e intente nuevamente.');
    }
}

// Evento para el botón de confirmar conversión
$(document).on('click', '#btnConfirmarConversion', function() {
    console.log('✅ Confirmando conversión a Cliente Match');
    
    if (!leadAConvertir) {
        alert('❌ Error: No hay datos del lead para convertir');
        return;
    }
    
    // Deshabilitar el botón para evitar doble click
    var btn = $(this);
    var originalText = btn.html();
    btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Convirtiendo...');
    
    // Hacer la llamada AJAX para convertir el lead
    $.ajax({
        url: '/ClientesLeads/ConvertirAClienteMatch',
        type: 'POST',
        data: { idCliente: leadAConvertir.iD_Cliente },
        success: function(response) {
            console.log('📥 Respuesta recibida:', response);
            
            if (response.success) {
                alert('✅ Lead convertido exitosamente a Cliente Match!\n\n' +
                      'Cliente: ' + leadAConvertir.nombres + ' ' + leadAConvertir.apellidos + '\n' +
                      'El lead ahora está en estado RESERVA.');
                
                // Cerrar el modal
                bootstrap.Modal.getInstance(document.getElementById('modalConvertirMatch')).hide();
                
                // Recargar las tablas
                recargarTablas();
                cargarEstadisticasMisLeads();
                
                // Limpiar la variable global
                leadAConvertir = null;
            } else {
                alert('❌ Error al convertir el lead:\n\n' + (response.message || 'Error desconocido'));
            }
            
            // Restaurar el botón
            btn.prop('disabled', false).html(originalText);
        },
        error: function(xhr, status, error) {
            console.error('❌ Error en la conversión:', error);
            alert('❌ Error de conexión al convertir el lead\n\n' + error);
            
            // Restaurar el botón
            btn.prop('disabled', false).html(originalText);
        }
    });
});

// Limpiar datos cuando se cierra el modal
$('#modalConvertirMatch').on('hidden.bs.modal', function () {
    leadAConvertir = null;
    console.log('🧹 Modal cerrado - datos limpiados');
});

function modificarLead(idCliente) {
    console.log('📝 Abriendo modal para modificar lead:', idCliente);
    
    // Mostrar indicador de carga en el modal
    var $modalBody = $('#modalModificarLead .modal-body');
    $modalBody.html(`
        <div class="text-center" style="padding: 40px;">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p style="margin-top: 15px; color: white;">Cargando información del lead...</p>
        </div>
    `);
    
    // Abrir el modal
    var modal = new bootstrap.Modal(document.getElementById('modalModificarLead'));
    modal.show();
    
    // Cargar el contenido del modal desde el servidor
    $.ajax({
        url: '/ClientesLeads/ObtenerModalModificar',
        type: 'GET',
        data: { idCliente: idCliente },
        success: function(html) {
            console.log('✅ Contenido del modal cargado');
            $modalBody.html(html);
            
            // Inicializar el botón de guardar
            inicializarBotonGuardarModificacion(idCliente);
        },
        error: function(xhr, status, error) {
            console.error('❌ Error al cargar el modal:', error);
            $modalBody.html(`
                <div class="alert alert-danger" role="alert">
                    <i class="fas fa-exclamation-triangle"></i>
                    <strong>Error al cargar el formulario</strong>
                    <p>No se pudo cargar la información del lead. Por favor, intente nuevamente.</p>
                    <p><small>Error: ${error}</small></p>
                </div>
            `);
        }
    });
}

// Función para inicializar el botón de guardar modificación
function inicializarBotonGuardarModificacion(idCliente) {
    $('#btnGuardarCambiosLead').off('click').on('click', function() {
        console.log('💾 Guardando cambios del lead:', idCliente);
        
        var $btn = $(this);
        var btnTextoOriginal = $btn.html();
        
        // Validar campos obligatorios
        var errores = [];
        
        var nombres = $('input[name="Nombres"]').val().trim();
        var apellidos = $('input[name="Apellidos"]').val().trim();
        var correo = $('input[name="Correo_Electronico"]').val().trim();
        
        if (!nombres) {
            errores.push('El campo Nombres es obligatorio');
        }
        
        if (!apellidos) {
            errores.push('El campo Apellidos es obligatorio');
        }
        
        if (!correo) {
            errores.push('El campo Correo Electrónico es obligatorio');
        } else {
            // Validar formato de correo
            var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailRegex.test(correo)) {
                errores.push('El formato del correo electrónico no es válido');
            }
        }
        
        if (errores.length > 0) {
            var $errorContainer = $('#errorContainer');
            var $errorList = $('#errorList');
            $errorList.empty();
            errores.forEach(function(error) {
                $errorList.append('<li>' + error + '</li>');
            });
            $errorContainer.show();
            
            // Hacer scroll al mensaje de error
            $errorContainer[0].scrollIntoView({ behavior: 'smooth' });
            return;
        }
        
        // Ocultar mensajes de error
        $('#errorContainer').hide();
        
        // Deshabilitar botón durante el proceso
        $btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Guardando...');
        
        // Recolectar datos del formulario
        var datos = {
            ID_Cliente: idCliente,
            ID_Unidad_Consultada: $('#ddlPropiedadEdit').val(),
            Unidad_Consultada: $('#hdnUnidadConsultadaEdit').val(),
            Asistente: esAgente ? $('input[name="Asistente"]').val() : $('#ddlAsistenteEdit').val(),
            Fecha_Contacto: esAgente ? $('#hdnFechaContacto').val() : $('#inputFechaContacto').val(),
            Nombres: nombres,
            Apellidos: apellidos,
            Correo_Electronico: correo,
            Telefono: $('input[name="Telefono"]').val(),
            Portal: $('#ddlPortalEdit').val(),
            Sexo: $('#ddlSexoEdit').val(),
            Seguimiento: esAgente ? $('#seguimientoHidden').val() : $('#ddlSeguimientoEdit').val(),
            Visita_Realizada: $('#Visita_Realizada').is(':checked'),
            Respuesta: $('textarea[name="Respuesta"]').val()
        };
        
        console.log('📤 Enviando datos:', datos);
        
        // Enviar datos al servidor
        $.ajax({
            url: '/ClientesLeads/Modificar',
            type: 'POST',
            data: datos,
            success: function(response) {
                console.log('📥 Respuesta recibida:', response);
                
                if (response.success) {
                    alert('✅ Lead modificado exitosamente');
                    
                    // Cerrar modal
                    bootstrap.Modal.getInstance(document.getElementById('modalModificarLead')).hide();
                    
                    // Recargar tablas y estadísticas
                    recargarTablas();
                    cargarEstadisticasMisLeads();
                } else {
                    alert('❌ Error al modificar el lead: ' + (response.message || 'Error desconocido'));
                    $btn.prop('disabled', false).html(btnTextoOriginal);
                }
            },
            error: function(xhr, status, error) {
                console.error('❌ Error al modificar lead:', error);
                alert('❌ Error de conexión al modificar el lead');
                $btn.prop('disabled', false).html(btnTextoOriginal);
            }
        });
    });
}

function eliminarLead(idCliente) {
    if (!esAdministrador) {
        mostrarMensajePermiso('eliminar');
        return;
    }
    
    if (confirm('¿Estás seguro de que deseas eliminar este lead?\n\nID: ' + idCliente + '\n\nEsta acción no se puede deshacer.')) {
        $.ajax({
            url: '/ClientesLeads/Eliminar',
            type: 'POST',
            data: { id: idCliente },
            success: function(response) {
                if (response.success) {
                    alert('Lead eliminado exitosamente.');
                    recargarTablas();
                    cargarEstadisticasMisLeads();
                } else {
                    alert('Error al eliminar el lead: ' + (response.message || 'Error desconocido'));
                }
            },
            error: function(xhr, status, error) {
                console.error('Error al eliminar lead:', error);
                alert('Error al eliminar el lead');
            }
        });
    }
}

// ===================================================
// FUNCIONES PARA MODAL AGREGAR
// ===================================================

function cargarDatosAgregar() {
    console.log('📋 Cargando datos para combos del modal agregar...');
    
    // Cargar propiedades
    $.ajax({
        url: '/ClientesLeads/Propiedades',
        type: 'GET',
        dataType: 'json',
        success: function(data) {
            console.log('✅ Propiedades recibidas:', data);
            var $combo = $('#ddlPropiedad');
            $combo.empty();
            $combo.append($('<option>', { value: '', text: '-- Seleccione una propiedad --' }));
            
            if (Array.isArray(data)) {
                $.each(data, function(i, item) {
                    // Soportar tanto PascalCase como camelCase
                    var idPropiedad = item.ID_Propiedad || item.iD_Propiedad || item.id_Propiedad || '';
                    var titulo = item.Titulo || item.titulo || '';
                    
                    if (idPropiedad) {
                        $combo.append($('<option>', {
                            value: idPropiedad,
                            text: idPropiedad + ' - ' + titulo
                        }));
                    }
                });
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error cargando propiedades:', error);
        }
    });
    
    // Cargar asistentes
    $.ajax({
        url: '/ClientesLeads/Asistentes',
        type: 'GET',
        dataType: 'json',
        success: function(data) {
            console.log('✅ Asistentes recibidos:', data);
            var $combo = $('#asistente');
            $combo.empty();
            $combo.append($('<option>', { value: '', text: '-- Seleccione un asistente --' }));
            
            if (Array.isArray(data)) {
                $.each(data, function(i, nombre) {
                    $combo.append($('<option>', {
                        value: nombre,
                        text: nombre
                    }));
                });
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error cargando asistentes:', error);
        }
    });
    
    // Cargar portales
    $.ajax({
        url: '/ClientesLeads/Portales',
        type: 'GET',
        dataType: 'json',
        success: function(data) {
            console.log('✅ Portales recibidos:', data);
            var $combo = $('#portal');
            $combo.empty();
            $combo.append($('<option>', { value: '', text: '-- Seleccione un portal --' }));
            
            if (Array.isArray(data)) {
                $.each(data, function(i, nombre) {
                    $combo.append($('<option>', {
                        value: nombre,
                        text: nombre
                    }));
                });
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error cargando portales:', error);
        }
    });
    
    console.log('✅ Carga de datos iniciada para modal agregar');
}

function actualizarImagenPropiedadAgregar(idPropiedad) {
    console.log('🖼️ Cargando imagen para propiedad:', idPropiedad);
    
    var container = $('#propiedadImagenContainerAgregar');
    
    // Mostrar indicador de carga
    container.html(
        '<div class="loading-image-agregar">' +
            '<i class="fas fa-spinner fa-spin"></i>' +
            '<p style="color: #888; margin-top: 10px;">Cargando imagen...</p>' +
        '</div>'
    );
    
    // Buscar la imagen en la lista de propiedades
    $.ajax({
        url: '/ClientesLeads/Propiedades',
        type: 'GET',
        dataType: 'json',
        success: function(data) {
            var propiedad = data.find(function(p) { 
                var id = p.ID_Propiedad || p.iD_Propiedad || p.id_Propiedad || '';
                return id === idPropiedad; 
            });
            
            var urlImagen = propiedad ? (propiedad.Url_Imagen || propiedad.url_Imagen || '') : '';
            if (propiedad && urlImagen) {
                
                // Convertir URL de Google Drive si es necesario
                if (urlImagen.includes('drive.google.com/file/d/')) {
                    var matches = urlImagen.match(/\/d\/([a-zA-Z0-9_-]+)/);
                    if (matches && matches[1]) {
                        urlImagen = 'https://drive.google.com/thumbnail?id=' + matches[1] + '&sz=w800';
                    }
                }
                
                // Crear elemento de imagen
                var img = new Image();
                img.onload = function() {
                    container.html(
                        '<img src="' + urlImagen + '" ' +
                             'alt="Imagen de la propiedad" ' +
                             'class="propiedad-imagen-agregar" ' +
                             'onerror="this.style.display=\'none\'; this.parentElement.innerHTML=\'<div class=\\"no-image-content-agregar\\"><i class=\\"fas fa-image\\" style=\\"font-size: 48px; color: #555;\\"></i><p style=\\"color: #888;\\">Error al cargar imagen</p></div>\';">'
                    );
                };
                img.onerror = function() {
                    container.html(
                        '<div class="no-image-content-agregar">' +
                            '<i class="fas fa-image" style="font-size: 48px; color: #555;"></i>' +
                            '<p style="color: #888; margin: 0;">Error al cargar imagen</p>' +
                        '</div>'
                    );
                };
                img.src = urlImagen;
            } else {
                // No hay imagen disponible
                container.html(
                    '<div class="no-image-content-agregar">' +
                        '<i class="fas fa-image" style="font-size: 48px; color: #555; margin-bottom: 10px;"></i>' +
                        '<p style="color: #888; margin: 0;">No hay imagen disponible</p>' +
                        '<small style="color: #888; font-size: 11px; margin-top: 5px;">' +
                            'Esta propiedad no tiene imagen asignada' +
                        '</small>' +
                    '</div>'
                );
            }
        },
        error: function() {
            container.html(
                '<div class="no-image-content-agregar">' +
                    '<i class="fas fa-exclamation-triangle" style="font-size: 48px; color: #e74c3c;"></i>' +
                    '<p style="color: #888;">Error al obtener información de la propiedad</p>' +
                '</div>'
            );
        }
    });
}

// ===================================================
// FUNCIONES PARA MODAL MODIFICAR  
// ===================================================

// NOTA: Estas funciones antiguas fueron eliminadas.
// Ahora se usan las funciones definidas más abajo que son más completas.

function actualizarImagenPropiedad(idPropiedad) {
    console.log('🖼️ Actualizando imagen de propiedad (modificar):', idPropiedad);
    
    var container = $('#propiedadImagenContainer');
    
    if (!idPropiedad || idPropiedad === '') {
        container.html(
            '<i class="fas fa-image" style="font-size: 48px; color: #555;"></i>' +
            '<p style="color: #888;">Seleccione una propiedad para ver la imagen</p>'
        );
        return;
    }
    
    // Mostrar indicador de carga
    container.html(
        '<i class="fas fa-spinner fa-spin" style="font-size: 32px; color: #3498db;"></i>' +
        '<p style="color: #888; margin-top: 10px;">Cargando imagen...</p>'
    );
    
    // Obtener la imagen de la propiedad
    $.ajax({
        url: '/ClientesLeads/Propiedades',
        type: 'GET',
        dataType: 'json',
        success: function(data) {
            var propiedad = data.find(function(p) { 
                var id = p.ID_Propiedad || p.iD_Propiedad || p.id_Propiedad || '';
                return id === idPropiedad; 
            });
            
            var urlImagen = propiedad ? (propiedad.Url_Imagen || propiedad.url_Imagen || '') : '';
            if (propiedad && urlImagen) {
                
                // Convertir URL de Google Drive
                if (urlImagen.includes('drive.google.com/file/d/')) {
                    var matches = urlImagen.match(/\/d\/([a-zA-Z0-9_-]+)/);
                    if (matches && matches[1]) {
                        urlImagen = 'https://drive.google.com/thumbnail?id=' + matches[1] + '&sz=w800';
                    }
                }
                
                // Cargar imagen
                var img = new Image();
                img.onload = function() {
                    container.html(
                        '<img src="' + urlImagen + '" ' +
                             'alt="Imagen de la propiedad" ' +
                             'class="propiedad-imagen" ' +
                             'style="max-width: 100%; max-height: 300px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.4);">'
                    );
                };
                img.onerror = function() {
                    container.html(
                        '<i class="fas fa-image" style="font-size: 48px; color: #555;"></i>' +
                        '<p style="color: #888;">Error al cargar imagen</p>'
                    );
                };
                img.src = urlImagen;
            } else {
                container.html(
                    '<i class="fas fa-image" style="font-size: 48px; color: #555;"></i>' +
                    '<p style="color: #888;">No hay imagen disponible</p>' +
                    '<small style="color: #666; font-size: 11px;">Esta propiedad no tiene imagen asignada</small>'
                );
            }
        },
        error: function() {
            container.html(
                '<i class="fas fa-exclamation-triangle" style="font-size: 48px; color: #e74c3c;"></i>' +
                '<p style="color: #888;">Error al obtener datos de la propiedad</p>'
            );
        }
    });
}

// ===================================================
// FUNCIONES PARA MODAL AGREGAR
// ===================================================

// Función para cargar datos de combos en modal agregar
function cargarDatosAgregar() {
    // Cargar portales
    $.get('/ClientesLeads/Portales')
        .done(function(data) {
            var $combo = $('#portal');
            $combo.empty().append('<option value="">Seleccionar portal</option>');
            
            $.each(data, function(i, item) {
                $combo.append($('<option>', {
                    value: item,
                    text: item
                }));
            });
            console.log('✅ Portales cargados:', data.length);
        })
        .fail(function(error) {
            console.error('❌ Error al cargar portales:', error);
        });
    
    // Cargar asistentes
    $.get('/ClientesLeads/Asistentes')
        .done(function(data) {
            var $combo = $('#asistente');
            $combo.empty().append('<option value="">Seleccionar asistente</option>');
            
            $.each(data, function(i, item) {
                $combo.append($('<option>', {
                    value: item,
                    text: item
                }));
            });
            console.log('✅ Asistentes cargados:', data.length);
        })
        .fail(function(error) {
            console.error('❌ Error al cargar asistentes:', error);
        });
    
    // Cargar propiedades
    $.get('/ClientesLeads/ObtenerPropiedades')
        .done(function(data) {
            var $combo = $('#ddlPropiedad');
            $combo.empty().append('<option value="">Seleccionar propiedad</option>');
            
            $.each(data, function(i, item) {
                // Soportar tanto PascalCase como camelCase (alfanuméricos)
                var idPropiedad = item.ID_Propiedad || item.iD_Propiedad || item.id_Propiedad || '';
                var titulo = item.Titulo || item.titulo || '';
                
                if (idPropiedad) {
                    $combo.append($('<option>', {
                        value: idPropiedad,
                        text: idPropiedad + ' - ' + titulo
                    }));
                }
            });
            console.log('✅ Propiedades cargadas:', data.length);
        })
        .fail(function(error) {
            console.error('❌ Error al cargar propiedades:', error);
        });
}

// Función para actualizar imagen de propiedad en modal agregar
function actualizarImagenPropiedadAgregar(idPropiedad) {
    console.log('🖼️ Cargando imagen de propiedad:', idPropiedad);
    
    var container = $('#propiedadImagenContainerAgregar');
    
    if (!idPropiedad || idPropiedad === '') {
        container.html(`
            <div class="no-image-content-agregar" style="display: flex; flex-direction: column; align-items: center;">
                <i class="fas fa-image" style="font-size: 48px; color: #555; margin-bottom: 10px;"></i>
                <p style="color: #888; margin: 0; font-size: 14px;">No hay imagen disponible</p>
                <small style="color: #888; font-size: 11px; margin-top: 5px;">
                    Seleccione una propiedad para ver la imagen
                </small>
            </div>
        `);
        return;
    }
    
    // Mostrar indicador de carga
    container.html(`
        <div class="loading-image-agregar" style="display: flex; flex-direction: column; align-items: center;">
            <i class="fas fa-spinner fa-spin" style="font-size: 24px; color: #3498db; margin-bottom: 10px;"></i>
            <p style="color: #888; margin: 0; font-size: 14px;">Cargando imagen...</p>
        </div>
    `);
    
    // Llamar al endpoint para obtener la URL de la imagen
    $.ajax({
        url: '/ClientesLeads/ObtenerImagenPropiedad',
        type: 'POST',
        data: { idPropiedad: idPropiedad },
        success: function(response) {
            console.log('✅ Respuesta del servidor:', response);
            
            if (response && response.success && response.imagenUrl) {
                container.html(`
                    <img src="${response.imagenUrl}" 
                         class="propiedad-imagen-agregar" 
                         alt="Imagen de la propiedad"
                         onerror="this.style.display='none'; this.nextElementSibling.style.display='flex';"
                         onload="this.style.display='block'; this.nextElementSibling.style.display='none';">
                    <div class="no-image-content-agregar" style="display: none; flex-direction: column; align-items: center;">
                        <i class="fas fa-image" style="font-size: 48px; color: #555; margin-bottom: 10px;"></i>
                        <p style="color: #888; margin: 0; font-size: 14px;">Imagen no disponible</p>
                        <small style="color: #888; font-size: 11px; margin-top: 5px;">
                            No se pudo cargar la imagen de la propiedad
                        </small>
                    </div>
                    <div style="margin-top: 10px; text-align: center;">
                        <small style="color: #28a745; font-size: 11px; font-style: italic;">
                            <i class="fas fa-cloud"></i> Imagen cargada desde Google Drive
                        </small>
                    </div>
                `);
                console.log('✅ Imagen cargada exitosamente');
            } else {
                container.html(`
                    <div class="no-image-content-agregar" style="display: flex; flex-direction: column; align-items: center;">
                        <i class="fas fa-image" style="font-size: 48px; color: #555; margin-bottom: 10px;"></i>
                        <p style="color: #888; margin: 0; font-size: 14px;">No hay imagen disponible</p>
                        <small style="color: #888; font-size: 11px; margin-top: 5px;">
                            No se encontró imagen para esta propiedad
                        </small>
                    </div>
                `);
                console.log('⚠️ No se encontró imagen para la propiedad');
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error al cargar imagen:', error);
            container.html(`
                <div class="no-image-content-agregar" style="display: flex; flex-direction: column; align-items: center;">
                    <i class="fas fa-exclamation-triangle" style="font-size: 48px; color: #e74c3c; margin-bottom: 10px;"></i>
                    <p style="color: #e74c3c; margin: 0; font-size: 14px;">Error al cargar imagen</p>
                    <small style="color: #888; font-size: 11px; margin-top: 5px;">
                        Ocurrió un error al intentar cargar la imagen
                    </small>
                </div>
            `);
        }
    });
}

// ===================================================
// FUNCIONES PARA MODAL MODIFICAR  
// ===================================================

// Función para configurar permisos por rol en modal modificar
function configurarPermisosPorRol() {
    console.log('🔐 Configurando permisos por rol...');
    
    if (esAgente) {
        // Deshabilitar campo de asistente
        $('#asisenteFieldContainer').addClass('agent-readonly');
        $('#ddlAsistenteEdit').prop('readonly', true).css({
            'background-color': '#272727',
            'color': '#ccc',
            'cursor': 'not-allowed'
        });
        $('#asisenteReadonlyNote').show();
        
        // Deshabilitar campo de fecha de contacto
        $('#fechaContactoFieldContainer').addClass('agent-readonly');
        $('#inputFechaContacto').prop('readonly', true).css({
            'background-color': '#272727',
            'color': '#ccc',
            'cursor': 'not-allowed'
        });
        $('#fechaReadonlyNote').show();
        
        // Deshabilitar campo de seguimiento
        $('#seguimientoFieldContainer').addClass('agent-readonly-seguimiento');
        $('#ddlSeguimientoEdit').prop('readonly', true).css({
            'background-color': '#272727',
            'color': '#ccc',
            'cursor': 'not-allowed',
            'pointer-events': 'none'
        });
        $('#seguimientoReadonlyNote').show();
        
        // Mostrar checkbox de "Lead Terminado"
        $('#leadTerminadoRow').show();
        
        console.log('✅ Campos restringidos para agente configurados');
    } else {
        console.log('✅ Sin restricciones - usuario es administrador');
    }
}

// Función para inicializar checkbox de visita realizada
function inicializarCheckboxVisita() {
    var $visitaCheckbox = $('#Visita_Realizada');
    var visitaInicialmenteChecked = $visitaCheckbox.is(':checked');
    
    if (visitaInicialmenteChecked) {
        $visitaCheckbox.prop('disabled', true);
        console.log('✅ Checkbox de visita ya estaba marcado - mantener deshabilitado');
    }
    
    $visitaCheckbox.on('change', function() {
        if ($(this).is(':checked')) {
            $(this).prop('disabled', true);
            $(this).closest('.visita-realizada-container').find('.checkbox-help')
                .removeClass('checkbox-help')
                .addClass('checkbox-help confirmed')
                .html('<i class="fas fa-check-circle"></i> Visita confirmada - No se puede desmarcar');
            
            console.log('✅ Checkbox de visita marcado - ahora deshabilitado permanentemente');
        }
    });
    
    $visitaCheckbox.on('click', function(e) {
        if ($(this).prop('disabled')) {
            e.preventDefault();
            return false;
        }
    });
}

// Función para inicializar checkbox de lead terminado (agentes)
function inicializarCheckboxLeadTerminado() {
    var $leadTerminadoCheckbox = $('#leadTerminado');
    var leadYaTerminado = $leadTerminadoCheckbox.is(':checked');
    
    if (leadYaTerminado) {
        $leadTerminadoCheckbox.prop('disabled', true);
        $('#leadTerminadoHelp').html('<i class="fas fa-check-circle"></i> Lead ya marcado como terminado - No se puede desmarcar')
            .css('color', '#e17055');
        console.log('✅ Lead ya estaba terminado - mantener checkbox deshabilitado');
    }
    
    $leadTerminadoCheckbox.on('change', function() {
        if ($(this).is(':checked')) {
            $('#seguimientoHidden').val('Terminado');
            $(this).prop('disabled', true);
            $('#leadTerminadoHelp')
                .html('<i class="fas fa-check-circle"></i> Lead ya marcado como terminado - No se puede desmarcar')
                .css('color', '#e17055');
            
            console.log('✅ Lead marcado como terminado - seguimiento actualizado');
        }
    });
    
    $leadTerminadoCheckbox.on('click', function(e) {
        if ($(this).prop('disabled')) {
            e.preventDefault();
            return false;
        }
    });
}

// Función para cargar datos iniciales en modal modificar
function cargarDatosInicialesModificar(propiedadActual, asistenteActual, portalActual, seguimientoActual) {
    console.log('📥 Cargando datos iniciales de los combos...');
    console.log('  - Propiedad actual:', propiedadActual);
    console.log('  - Asistente actual:', asistenteActual);
    console.log('  - Portal actual:', portalActual);
    console.log('  - Seguimiento actual:', seguimientoActual);
    
    // Cargar propiedades, asistentes y portales
    $.when(
        cargarPropiedadesModificar(),
        cargarAsistentesModificar(),
        cargarPortalesModificar()
    ).done(function() {
        console.log('✅ Todos los datos iniciales cargados');
        
        // Preseleccionar valores actuales
        if (propiedadActual) {
            $('#ddlPropiedadEdit').val(propiedadActual);
            console.log('✅ Propiedad preseleccionada:', propiedadActual);
            
            // Cargar imagen de la propiedad
            setTimeout(function() {
                actualizarImagenPropiedad(propiedadActual);
            }, 500);
        }
        
        if (asistenteActual) {
            $('#ddlAsistenteEdit').val(asistenteActual);
            console.log('✅ Asistente preseleccionado:', asistenteActual);
        }
        
        if (portalActual) {
            $('#ddlPortalEdit').val(portalActual);
            console.log('✅ Portal preseleccionado:', portalActual);
        }
        
        if (seguimientoActual) {
            $('#ddlSeguimientoEdit').val(seguimientoActual);
            console.log('✅ Seguimiento preseleccionado:', seguimientoActual);
        }
    });
}

// Cargar propiedades para modal modificar
function cargarPropiedadesModificar() {
    return $.get('/ClientesLeads/ObtenerPropiedades')
        .done(function(data) {
            var $combo = $('#ddlPropiedadEdit');
            $combo.empty().append('<option value="">Seleccionar propiedad</option>');
            
            $.each(data, function(i, item) {
                // Soportar tanto PascalCase como camelCase (alfanuméricos)
                var idPropiedad = item.ID_Propiedad || item.iD_Propiedad || item.id_Propiedad || '';
                var titulo = item.Titulo || item.titulo || '';
                
                if (idPropiedad) {
                    $combo.append($('<option>', {
                        value: idPropiedad,
                        text: idPropiedad + ' - ' + titulo
                    }));
                }
            });
            console.log('✅ Propiedades cargadas en modal modificar:', data.length);
        })
        .fail(function(error) {
            console.error('❌ Error al cargar propiedades:', error);
        });
}

// Cargar asistentes para modal modificar
function cargarAsistentesModificar() {
    return $.get('/ClientesLeads/Asistentes')
        .done(function(data) {
            var $combo = $('#ddlAsistenteEdit');
            $combo.empty().append('<option value="">Seleccionar asistente</option>');
            
            $.each(data, function(i, item) {
                $combo.append($('<option>', {
                    value: item,
                    text: item
                }));
            });
            console.log('✅ Asistentes cargados en modal modificar:', data.length);
        })
        .fail(function(error) {
            console.error('❌ Error al cargar asistentes:', error);
        });
}

// Cargar portales para modal modificar
function cargarPortalesModificar() {
    return $.get('/ClientesLeads/Portales')
        .done(function(data) {
            var $combo = $('#ddlPortalEdit');
            $combo.empty().append('<option value="">Seleccionar portal</option>');
            
            $.each(data, function(i, item) {
                $combo.append($('<option>', {
                    value: item,
                    text: item
                }));
            });
            console.log('✅ Portales cargados en modal modificar:', data.length);
        })
        .fail(function(error) {
            console.error('❌ Error al cargar portales:', error);
        });
}

// Función para actualizar imagen de propiedad en modal modificar
function actualizarImagenPropiedad(idPropiedad) {
    console.log('🖼️ Actualizando imagen de propiedad:', idPropiedad);
    
    var container = $('#propiedadImagenContainer');
    
    if (!idPropiedad || idPropiedad === '') {
        container.html(`
            <div class="no-image-content" style="display: flex; flex-direction: column; align-items: center;">
                <i class="fas fa-image"></i>
                <p>No hay imagen disponible</p>
                <small style="color: #888; font-size: 11px; margin-top: 5px;">
                    Seleccione una propiedad para cargar la imagen
                </small>
            </div>
        `);
        return;
    }
    
    // Mostrar indicador de carga
    container.html(`
        <div class="loading-image" style="display: flex; flex-direction: column; align-items: center;">
            <i class="fas fa-spinner fa-spin" style="font-size: 24px; color: #3498db; margin-bottom: 10px;"></i>
            <p>Cargando imagen...</p>
        </div>
    `);
    
    // Llamar al endpoint para obtener la URL de la imagen
    $.ajax({
        url: '/ClientesLeads/ObtenerImagenPropiedad',
        type: 'POST',
        data: { idPropiedad: idPropiedad },
        success: function(response) {
            if (response && response.success && response.imagenUrl) {
                container.html(`
                    <img src="${response.imagenUrl}" class="propiedad-imagen" alt="Imagen de la propiedad"
                         onerror="this.style.display='none'; this.nextElementSibling.style.display='flex';"
                         onload="this.style.display='block'; this.nextElementSibling.style.display='none';">
                    <div class="no-image-content" style="display: none; flex-direction: column; align-items: center;">
                        <i class="fas fa-image"></i>
                        <p>Imagen no disponible</p>
                    </div>
                    <div style="margin-top: 10px; text-align: center;">
                        <small style="color: #28a745; font-size: 11px; font-style: italic;">
                            <i class="fas fa-cloud"></i> Imagen cargada automáticamente desde Google Drive
                        </small>
                    </div>
                `);
            } else {
                container.html(`
                    <div class="no-image-content" style="display: flex; flex-direction: column; align-items: center;">
                        <i class="fas fa-image"></i>
                        <p>No hay imagen disponible</p>
                    </div>
                `);
            }
        },
        error: function() {
            container.html(`
                <div class="no-image-content" style="display: flex; flex-direction: column; align-items: center;">
                    <i class="fas fa-exclamation-triangle" style="color: #e74c3c;"></i>
                    <p>Error al cargar imagen</p>
                </div>
            `);
        }
    });
}

