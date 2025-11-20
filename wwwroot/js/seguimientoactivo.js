// JavaScript para Seguimiento Activo

let tablaSeguimiento;

function inicializarDataTablesSeguimiento() {
    console.log('📊 Inicializando DataTables...');
    tablaSeguimiento = $('#tablaSeguimiento').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
        },
        responsive: true,
        order: [[2, 'desc']],
        pageLength: 25
    });
    console.log('✅ DataTables inicializado');
}

function cargarSeguimientos() {
    console.log('📋 Cargando seguimientos...');
    
    const filtros = {
        ID_Cliente: $('#filtroCliente').val() || '',
        Agente: $('#filtroAgente').val() || '',
        Tipo_Accion: $('#filtroTipoAccion').val() || '',
        Estado: $('#filtroEstado').val() || '',
        PageNumber: 1,
        PageSize: 1000
    };
    
    $.ajax({
        url: '/SeguimientoActivo/Listar',
        type: 'GET',
        data: filtros,
        success: function(response) {
            console.log('✅ Respuesta recibida:', response);
            if (response.success && response.data) {
                actualizarTablaSeguimiento(response.data);
            }
        },
        error: function(xhr, status, error) {
            console.error('❌ Error:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Error al cargar seguimientos'
            });
        }
    });
}

function actualizarTablaSeguimiento(datos) {
    console.log('🔄 Actualizando tabla con', datos.length, 'registros');
    
    tablaSeguimiento.clear();
    
    datos.forEach(function(seg) {
        const id_cliente = seg.iD_Cliente || seg.ID_Cliente || '';
        const agente = seg.agente || seg.Agente || '';
        const fecha = seg.fecha_Accion || seg.Fecha_Accion || '';
        const tipo = seg.tipo_Accion || seg.Tipo_Accion || '';
        const desc = seg.descripcion_Accion || seg.Descripcion_Accion || '';
        const resultado = seg.resultado || seg.Resultado || '';
        const estado = seg.estado || seg.Estado || '';
        const proximo = seg.fecha_Proximo_Contacto || seg.Fecha_Proximo_Contacto || '';
        
        const badgeEstado = estado === 'Completado' ? 'badge-completado' : 'badge-pendiente';
        
        tablaSeguimiento.row.add([
            id_cliente,
            agente,
            formatearFecha(fecha),
            tipo,
            desc,
            resultado,
            `<span class="badge ${badgeEstado}">${estado}</span>`,
            formatearFecha(proximo)
        ]);
    });
    
    tablaSeguimiento.draw();
    console.log('✅ Tabla actualizada');
}

function formatearFecha(fecha) {
    if (!fecha) return '-';
    const d = new Date(fecha);
    return d.toLocaleDateString('es-ES');
}

