// ============================================
// JavaScript para Clientes Match
// ============================================

let tablaMatch;

// Inicializar DataTables
function inicializarDataTablesMatch() {
    console.log('📊 Inicializando DataTables para Clientes Match...');
    
    tablaMatch = $('#tablaClientesMatch').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
        },
        responsive: true,
        order: [[0, 'desc']],
        pageLength: 25,
        dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip',
        columnDefs: [
            { targets: -1, orderable: false } // Columna de acciones no ordenable
        ]
    });
    
    console.log('✅ DataTables inicializado');
}

// Cargar Clientes Match con filtros
function cargarClientesMatch() {
    console.log('📋 Cargando Clientes Match...');
    
    // Obtener valores de filtros
    const filtros = {
        ID_Interno: $('#filtroID').val() || '',
        Nombre: $('#filtroNombre').val() || '',
        Telefono: $('#filtroTelefono').val() || '',
        Correo: $('#filtroCorreo').val() || '',
        ColumnaOrden: 'ID_Interno',
        DireccionOrden: 'DESC'
    };
    
    console.log('📤 Enviando filtros:', filtros);
    
    $.ajax({
        url: '/ClientesMatch/Listar',
        type: 'GET',
        data: filtros,
        success: function(response) {
            console.log('✅ Respuesta recibida:', response);
            console.log('Type of response:', typeof response);
            console.log('response.success:', response.success);
            console.log('response.data:', response.data);
            
            if (response.success && response.data) {
                console.log('👍 Respuesta válida, actualizando tabla con', response.data.length, 'registros');
                actualizarTablaMatch(response.data);
            } else if (response.success === false) {
                console.error('❌ Error en la respuesta:', response.message);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message || 'Error al cargar los clientes match'
                });
            } else {
                console.warn('⚠️ Respuesta sin formato esperado:', response);
                // Intentar cargar de todas formas si hay datos
                if (Array.isArray(response)) {
                    console.log('📊 La respuesta es un array directo, usándolo');
                    actualizarTablaMatch(response);
                } else {
                    Swal.fire({
                        icon: 'warning',
                        title: 'Advertencia',
                        text: 'La respuesta no tiene el formato esperado'
                    });
                }
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error AJAX:', {
                status: status,
                error: error,
                xhr: xhr,
                responseText: xhr.responseText
            });
            Swal.fire({
                icon: 'error',
                title: 'Error de Conexión',
                text: 'Error al cargar los clientes match: ' + error
            });
        }
    });
}

// Actualizar tabla con datos
function actualizarTablaMatch(datos) {
    console.log('🔄 Actualizando tabla con', datos.length, 'registros');
    console.log('📋 Primer cliente de ejemplo:', datos[0]);
    
    // Limpiar tabla
    tablaMatch.clear();
    console.log('🗑️ Tabla limpiada');
    
    // Agregar filas
    let contador = 0;
    datos.forEach(function(cliente) {
        // Los datos vienen en minúsculas desde el servidor
        const id = cliente.iD_Interno || cliente.ID_Interno || '';
        const tipoMatch = cliente.tipo_Match || cliente.Tipo_Match || 'N/A';
        const nombre = cliente.nombre || cliente.Nombre || '';
        const rut = cliente.rut || cliente.Rut || 'N/A';
        const telefono = cliente.telefono || cliente.Telefono || 'N/A';
        const correo = cliente.correo || cliente.Correo || 'N/A';
        const comuna = cliente.comuna || cliente.Comuna || 'N/A';
        const profesion = cliente.profesion || cliente.Profesion || 'N/A';
        
        const badgeClass = obtenerClaseBadgeTipoMatch(tipoMatch);
        
        const fila = [
            id,
            `<span class="badge ${badgeClass}">${tipoMatch}</span>`,
            nombre,
            rut,
            telefono,
            correo,
            comuna,
            profesion,
            generarBotonesAccion(id)
        ];
        
        if (contador === 0) {
            console.log('📝 Primera fila a agregar:', fila);
        }
        
        tablaMatch.row.add(fila);
        contador++;
    });
    
    console.log('✅ Se agregaron', contador, 'filas');
    
    // Redibujar tabla
    tablaMatch.draw();
    console.log('✅ Tabla redibujada');
    
    // Verificar que la tabla tenga filas
    const totalFilas = tablaMatch.rows().count();
    console.log('📊 Total de filas en la tabla después del draw:', totalFilas);
}

// Obtener clase de badge según tipo de match
function obtenerClaseBadgeTipoMatch(tipoMatch) {
    const tipo = (tipoMatch || '').toLowerCase();
    
    if (tipo.includes('lead')) return 'badge-lead-convertido';
    if (tipo.includes('directo')) return 'badge-cliente-directo';
    if (tipo.includes('referido')) return 'badge-referido';
    if (tipo.includes('web')) return 'badge-contacto-web';
    
    return 'badge-otro';
}

// Generar botones de acción
function generarBotonesAccion(idInterno) {
    return `
        <button class="btn btn-sm btn-warning btn-action" onclick="abrirModalModificar('${idInterno}')">
            <i class="fas fa-edit"></i> Editar
        </button>
        <button class="btn btn-sm btn-danger btn-action" onclick="eliminarClienteMatch('${idInterno}')">
            <i class="fas fa-trash"></i> Eliminar
        </button>
    `;
}

// Abrir modal para modificar
function abrirModalModificar(idInterno) {
    console.log('📝 Abriendo modal para modificar:', idInterno);
    
    $.ajax({
        url: '/ClientesMatch/ObtenerPorId',
        type: 'GET',
        data: { id: idInterno },
        success: function(response) {
            console.log('✅ Cliente recibido:', response);
            
            if (response.success && response.data) {
                cargarDatosEnFormularioModificar(response.data);
                $('#modificarModal').modal('show');
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message || 'Error al obtener los datos del cliente'
                });
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error al obtener cliente:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Error al obtener los datos del cliente'
            });
        }
    });
}

// Cargar datos en formulario de modificar
function cargarDatosEnFormularioModificar(cliente) {
    console.log('📥 Cargando datos en formulario:', cliente);
    
    // Los datos vienen en minúsculas desde el servidor
    $('#modificar_ID_Interno').val(cliente.iD_Interno || cliente.ID_Interno || '');
    $('#modificar_ID_Display').val(cliente.iD_Interno || cliente.ID_Interno || '');
    $('#modificar_Tipo_Match').val(cliente.tipo_Match || cliente.Tipo_Match || '');
    $('#modificar_Nombre').val(cliente.nombre || cliente.Nombre || '');
    $('#modificar_Rut').val(cliente.rut || cliente.Rut || '');
    $('#modificar_Telefono').val(cliente.telefono || cliente.Telefono || '');
    $('#modificar_Correo').val(cliente.correo || cliente.Correo || '');
    $('#modificar_Direccion').val(cliente.direccion || cliente.Direccion || '');
    $('#modificar_Comuna').val(cliente.comuna || cliente.Comuna || '');
    $('#modificar_Estado_Civil').val(cliente.estado_Civil || cliente.Estado_Civil || '');
    $('#modificar_Profesion').val(cliente.profesion || cliente.Profesion || '');
    $('#modificar_Giro_Razon_Social').val(cliente.giro_Razon_Social || cliente.Giro_Razon_Social || '');
    $('#modificar_Datos_adjuntos').val(cliente.datos_adjuntos || cliente.Datos_adjuntos || '');
}

// Guardar cambios (modificar)
$(document).on('click', '#btnGuardarModificar', function() {
    console.log('💾 Guardando cambios...');
    
    // Validar formulario
    const form = document.getElementById('formModificar');
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }
    
    // Recopilar datos
    const cliente = {
        ID_Interno: $('#modificar_ID_Interno').val(),
        Tipo_Match: $('#modificar_Tipo_Match').val(),
        Nombre: $('#modificar_Nombre').val(),
        Rut: $('#modificar_Rut').val() || null,
        Telefono: $('#modificar_Telefono').val() || null,
        Correo: $('#modificar_Correo').val() || null,
        Direccion: $('#modificar_Direccion').val() || null,
        Comuna: $('#modificar_Comuna').val() || null,
        Estado_Civil: $('#modificar_Estado_Civil').val() || null,
        Profesion: $('#modificar_Profesion').val() || null,
        Giro_Razon_Social: $('#modificar_Giro_Razon_Social').val() || null,
        Datos_adjuntos: $('#modificar_Datos_adjuntos').val() || null
    };
    
    console.log('📤 Enviando datos:', cliente);
    
    // Enviar al servidor
    $.ajax({
        url: '/ClientesMatch/Actualizar',
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify(cliente),
        success: function(response) {
            console.log('✅ Respuesta del servidor:', response);
            
            if (response.success) {
                Swal.fire({
                    icon: 'success',
                    title: '¡Éxito!',
                    text: 'Cliente Match actualizado correctamente',
                    timer: 2000,
                    showConfirmButton: false
                }).then(() => {
                    $('#modificarModal').modal('hide');
                    cargarClientesMatch();
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message || 'Error al actualizar el cliente'
                });
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error al actualizar:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Error al actualizar el cliente match'
            });
        }
    });
});

// Eliminar cliente match
function eliminarClienteMatch(idInterno) {
    console.log('🗑️ Solicitud para eliminar:', idInterno);
    
    Swal.fire({
        title: '¿Está seguro?',
        text: "Esta acción no se puede revertir",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/ClientesMatch/Eliminar',
                type: 'DELETE',
                data: { id: idInterno },
                success: function(response) {
                    console.log('✅ Respuesta del servidor:', response);
                    
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: '¡Eliminado!',
                            text: 'Cliente Match eliminado correctamente',
                            timer: 2000,
                            showConfirmButton: false
                        }).then(() => {
                            cargarClientesMatch();
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: response.message || 'Error al eliminar el cliente'
                        });
                    }
                },
                error: function(xhr, status, error) {
                    console.error('❌ Error al eliminar:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Error al eliminar el cliente match'
                    });
                }
            });
        }
    });
}

// Limpiar formulario al cerrar modal
$('#modificarModal').on('hidden.bs.modal', function () {
    $('#formModificar')[0].reset();
    $('#formModificar').removeClass('was-validated');
});

