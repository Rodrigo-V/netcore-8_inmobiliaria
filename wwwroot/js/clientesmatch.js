// ============================================
// JavaScript para Clientes Match
// Patrón DataTables server-side (igual que Clientes Leads)
// ============================================

let tablaMatch;

function obtenerFiltrosMatch() {
    return {
        filtroID: $('#filtroID').val() || '',
        filtroNombre: $('#filtroNombre').val() || '',
        filtroTelefono: $('#filtroTelefono').val() || '',
        filtroCorreo: $('#filtroCorreo').val() || ''
    };
}

function inicializarDataTablesMatch() {
    console.log('📊 Inicializando DataTables para Clientes Match...');

    tablaMatch = $('#tablaClientesMatch').DataTable(DataTablesCommon.baseConfig({
        order: [[0, 'desc']],
        pageLength: 25,
        ajax: DataTablesCommon.ajaxPost('/ClientesMatch/GetData', function (d) {
            Object.assign(d, obtenerFiltrosMatch());
        }),
        columns: [
            { data: 'iD_Interno', name: 'ID_Interno' },
            {
                data: 'tipo_Match',
                render: function (data) {
                    var badgeClass = obtenerClaseBadgeTipoMatch(data);
                    return '<span class="badge ' + badgeClass + '">' + (data || 'N/A') + '</span>';
                }
            },
            { data: 'nombre', defaultContent: '' },
            { data: 'rut', defaultContent: 'N/A' },
            { data: 'telefono', defaultContent: 'N/A' },
            { data: 'correo', defaultContent: 'N/A' },
            { data: 'comuna', defaultContent: 'N/A' },
            { data: 'profesion', defaultContent: 'N/A' },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return generarBotonesAccion(row.iD_Interno);
                }
            }
        ],
        columnDefs: [
            { targets: -1, orderable: false }
        ]
    }));

    console.log('✅ DataTables inicializado');
}

function recargarTablaMatch() {
    if (tablaMatch) {
        tablaMatch.ajax.reload();
    }
}

// Obtener clase de badge según tipo de match
function obtenerClaseBadgeTipoMatch(tipoMatch) {
    var tipo = (tipoMatch || '').toLowerCase();

    if (tipo.includes('lead')) return 'badge-lead-convertido';
    if (tipo.includes('directo')) return 'badge-cliente-directo';
    if (tipo.includes('referido')) return 'badge-referido';
    if (tipo.includes('web')) return 'badge-contacto-web';

    return 'badge-otro';
}

// Generar botones de acción
function generarBotonesAccion(idInterno) {
    return `
        <div class="action-buttons">
            <i class="fas fa-edit edit" onclick="abrirModalModificar('${idInterno}')" title="Editar"></i>
            <i class="fas fa-trash-alt trash" onclick="eliminarClienteMatch('${idInterno}')" title="Eliminar"></i>
        </div>
    `;
}

// Abrir modal para modificar
function abrirModalModificar(idInterno) {
    console.log('📝 Abriendo modal para modificar:', idInterno);

    $.ajax({
        url: '/ClientesMatch/ObtenerPorId',
        type: 'GET',
        data: { id: idInterno },
        success: function (response) {
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
        error: function () {
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
$(document).on('click', '#btnGuardarModificar', function () {
    var form = document.getElementById('formModificar');
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    var cliente = {
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

    $.ajax({
        url: '/ClientesMatch/Actualizar',
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify(cliente),
        success: function (response) {
            if (response.success) {
                Swal.fire({
                    icon: 'success',
                    title: '¡Éxito!',
                    text: 'Cliente Match actualizado correctamente',
                    timer: 2000,
                    showConfirmButton: false
                }).then(function () {
                    $('#modificarModal').modal('hide');
                    recargarTablaMatch();
                });
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message || 'Error al actualizar el cliente'
                });
            }
        },
        error: function () {
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
    Swal.fire({
        title: '¿Está seguro?',
        text: 'Esta acción no se puede revertir',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then(function (result) {
        if (result.isConfirmed) {
            $.ajax({
                url: '/ClientesMatch/Eliminar',
                type: 'DELETE',
                data: { id: idInterno },
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: '¡Eliminado!',
                            text: 'Cliente Match eliminado correctamente',
                            timer: 2000,
                            showConfirmButton: false
                        }).then(function () {
                            recargarTablaMatch();
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: response.message || 'Error al eliminar el cliente'
                        });
                    }
                },
                error: function () {
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

$('#modificarModal').on('hidden.bs.modal', function () {
    $('#formModificar')[0].reset();
    $('#formModificar').removeClass('was-validated');
});

$(document).ready(function () {
    if (typeof inicializarDataTablesMatch === 'function') {
        inicializarDataTablesMatch();
    }

    $('#btnBuscar').on('click', function () {
        recargarTablaMatch();
    });

    $('#filtroID, #filtroNombre, #filtroTelefono, #filtroCorreo').on('keypress', function (e) {
        if (e.which === 13) {
            recargarTablaMatch();
        }
    });
});
