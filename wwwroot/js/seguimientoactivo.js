// JavaScript para Seguimiento Activo
// Patrón DataTables server-side (igual que Clientes Leads)

let tablaSeguimiento;

function obtenerFiltrosSeguimiento() {
    return {
        filtroCliente: $('#filtroCliente').val() || '',
        filtroAgente: $('#filtroAgente').val() || '',
        filtroTipoAccion: $('#filtroTipoAccion').val() || '',
        filtroEstado: $('#filtroEstado').val() || ''
    };
}

function cargarFiltrosSeguimiento() {
    $.get('/SeguimientoActivo/Clientes', function (response) {
        if (response.success && response.data) {
            var select = $('#filtroCliente');
            select.find('option:not(:first)').remove();
            response.data.forEach(function (id) {
                select.append($('<option>', { value: id, text: id }));
            });
        }
    });

    $.get('/SeguimientoActivo/Agentes', function (response) {
        if (response.success && response.data) {
            var select = $('#filtroAgente');
            select.find('option:not(:first)').remove();
            response.data.forEach(function (agente) {
                select.append($('<option>', { value: agente, text: agente }));
            });
        }
    });
}

function inicializarDataTablesSeguimiento() {
    console.log('📊 Inicializando DataTables Seguimiento Activo...');

    tablaSeguimiento = $('#tablaSeguimiento').DataTable(DataTablesCommon.baseConfig({
        order: [[2, 'desc']],
        pageLength: 25,
        ajax: DataTablesCommon.ajaxPost('/SeguimientoActivo/GetData', function (d) {
            Object.assign(d, obtenerFiltrosSeguimiento());
        }),
        columns: [
            { data: 'iD_Cliente', name: 'ID_Cliente' },
            { data: 'agente', name: 'Agente' },
            {
                data: 'fecha_Accion',
                name: 'Fecha_Accion',
                render: function (data, type) {
                    if (type === 'display' || type === 'filter') {
                        return formatearFecha(data);
                    }
                    return data;
                }
            },
            { data: 'tipo_Accion', name: 'Tipo_Accion' },
            { data: 'descripcion_Accion', name: 'Descripcion_Accion' },
            { data: 'resultado', name: 'Resultado' },
            {
                data: 'estado',
                name: 'Estado',
                render: function (data) {
                    var badgeEstado = data === 'Completado' ? 'badge-completado' : 'badge-pendiente';
                    return '<span class="badge ' + badgeEstado + '">' + (data || '') + '</span>';
                }
            },
            {
                data: 'fecha_Proximo_Contacto',
                name: 'Fecha_Proximo_Contacto',
                render: function (data, type) {
                    if (type === 'display' || type === 'filter') {
                        return formatearFecha(data);
                    }
                    return data;
                }
            }
        ]
    }));

    console.log('✅ DataTables inicializado');
}

function recargarTablaSeguimiento() {
    if (tablaSeguimiento) {
        tablaSeguimiento.ajax.reload();
    }
}

function formatearFecha(fecha) {
    if (!fecha) return '-';
    var d = new Date(fecha);
    return d.toLocaleDateString('es-ES');
}

$(document).ready(function () {
    cargarFiltrosSeguimiento();

    if (typeof inicializarDataTablesSeguimiento === 'function') {
        inicializarDataTablesSeguimiento();
    }

    $('#btnBuscar').on('click', function () {
        recargarTablaSeguimiento();
    });

    $('#filtroCliente, #filtroAgente, #filtroTipoAccion, #filtroEstado').on('change', function () {
        recargarTablaSeguimiento();
    });
});
