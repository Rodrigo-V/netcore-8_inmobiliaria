// JavaScript para Sincronización de Portales 2

$(document).ready(function() {
    console.log('🚀 Módulo Sincronización de Portales 2 cargado');
    console.log('📊 Total de propiedades en el DOM:', $('.property-card').length);
});

// Función para ver detalles de clics (llamada desde la vista)
function verDetallesClics(idPropiedad) {
    console.log('📊 Ver detalles de clics para:', idPropiedad);
    
    Swal.fire({
        title: 'Detalles de Clics',
        html: `
            <p>Detalles de clics para propiedad: <strong>${idPropiedad}</strong></p>
            <p class="text-muted" style="margin-top: 15px;">Funcionalidad en desarrollo</p>
            <small>Aquí se mostrarán las estadísticas detalladas de clics por portal</small>
        `,
        icon: 'info',
        confirmButtonColor: '#3498db'
    });
}

// Función para actualizar estadísticas
function actualizarEstadisticas() {
    Swal.fire({
        title: 'Actualizando...',
        text: 'Recargando estadísticas',
        icon: 'info',
        timer: 1000,
        showConfirmButton: false
    }).then(() => {
        location.reload();
    });
}

console.log('✅ Sincronización de Portales 2 JS cargado');
