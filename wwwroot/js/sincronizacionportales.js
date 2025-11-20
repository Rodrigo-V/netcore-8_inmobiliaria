// JavaScript para Sincronización de Portales

$(document).ready(function() {
    console.log('🚀 Módulo Sincronización de Portales cargado');
    
    // Inicializar DataTables si existe la tabla
    if ($('#tablaMatriz').length > 0) {
        console.log('📊 Inicializando DataTables...');
    }
});

// Función para descargar Excel
function descargarExcel() {
    console.log('📥 Descargando Excel...');
    
    Swal.fire({
        title: 'Descargar Excel',
        html: `
            <div style="text-align: left;">
                <label style="display: block; margin-bottom: 5px;">Fecha Desde (opcional):</label>
                <input type="date" id="fechaDesde" class="swal2-input" style="width: 90%;">
                
                <label style="display: block; margin-top: 15px; margin-bottom: 5px;">Fecha Hasta (opcional):</label>
                <input type="date" id="fechaHasta" class="swal2-input" style="width: 90%;">
                
                <small style="display: block; margin-top: 10px; color: #666;">
                    Si no especificas fechas, se descargarán los últimos 30 días
                </small>
            </div>
        `,
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-download"></i> Descargar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#28a745',
        preConfirm: () => {
            const fechaDesde = document.getElementById('fechaDesde').value;
            const fechaHasta = document.getElementById('fechaHasta').value;
            return { fechaDesde, fechaHasta };
        }
    }).then((result) => {
        if (result.isConfirmed) {
            const { fechaDesde, fechaHasta } = result.value;
            
            let url = '/SincronizacionPortales/DescargarExcel';
            const params = [];
            if (fechaDesde) params.push(`fechaDesde=${fechaDesde}`);
            if (fechaHasta) params.push(`fechaHasta=${fechaHasta}`);
            if (params.length > 0) url += '?' + params.join('&');
            
            console.log('🔗 URL de descarga:', url);
            
            Swal.fire({
                title: 'Generando Excel...',
                text: 'Por favor espera',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });
            
            console.log('📥 Iniciando descarga...');
            
            // Usar un elemento <a> oculto para forzar la descarga
            const link = document.createElement('a');
            link.href = url;
            link.download = 'MatrizSincronizacion.xlsx';
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            
            console.log('✅ Descarga iniciada');
            
            setTimeout(() => {
                Swal.close();
                Swal.fire({
                    icon: 'success',
                    title: 'Excel descargado',
                    text: 'El archivo se ha descargado correctamente',
                    timer: 2000,
                    showConfirmButton: false
                });
            }, 2000);
        }
    });
}

console.log('✅ Sincronización de Portales JS cargado');
