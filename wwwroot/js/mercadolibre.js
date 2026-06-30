/**
 * Funciones para interactuar con Mercado Libre API
 * @author Sistema Inmobiliaria
 * @version 1.0
 */

const MercadoLibre = {
    /**
     * Abre un modal con las estadísticas de visitas de una publicación
     * @param {string} itemId - ID del item de Mercado Libre
     * @param {string} titulo - Título de la propiedad
     */
    verVisitasModal: function(itemId, titulo) {
        if (!itemId) {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'No se proporcionó un ID de publicación válido'
            });
            return;
        }

        // Crear modal si no existe
        let modalElement = document.getElementById('modalVisitasML');
        if (!modalElement) {
            modalElement = document.createElement('div');
            modalElement.innerHTML = `
                <div class="modal fade" id="modalVisitasML" tabindex="-1" aria-labelledby="modalVisitasMLLabel" aria-hidden="true">
                    <div class="modal-dialog modal-lg modal-dialog-centered modal-dialog-scrollable">
                        <div class="modal-content" id="contenidoModalVisitasML">
                            <div class="modal-body text-center py-5">
                                <div class="spinner-border text-primary" role="status">
                                    <span class="visually-hidden">Cargando...</span>
                                </div>
                                <p class="mt-3">Cargando estadísticas de Mercado Libre...</p>
                            </div>
                        </div>
                    </div>
                </div>
            `;
            document.body.appendChild(modalElement);
        }

        // Mostrar modal
        const modal = new bootstrap.Modal(document.getElementById('modalVisitasML'));
        modal.show();

        // Cargar contenido via AJAX
        fetch(`/MercadoLibre/ModalVisitas?itemId=${encodeURIComponent(itemId)}&titulo=${encodeURIComponent(titulo || 'Propiedad')}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al cargar las estadísticas');
                }
                return response.text();
            })
            .then(html => {
                document.getElementById('contenidoModalVisitasML').innerHTML = html;
            })
            .catch(error => {
                console.error('Error:', error);
                document.getElementById('contenidoModalVisitasML').innerHTML = `
                    <div class="modal-header bg-danger text-white">
                        <h5 class="modal-title">
                            <i class="bi bi-exclamation-triangle-fill"></i>
                            Error
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="alert alert-danger mb-0">
                            <i class="bi bi-x-circle me-2"></i>
                            ${error.message}
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            Cerrar
                        </button>
                    </div>
                `;
            });
    },

    /**
     * Obtiene las visitas de un item en formato JSON
     * @param {string} itemId - ID del item de Mercado Libre
     * @returns {Promise} Promise con los datos de visitas
     */
    obtenerVisitas: async function(itemId) {
        try {
            const response = await fetch(`/MercadoLibre/ObtenerVisitasJson?itemId=${encodeURIComponent(itemId)}`);
            if (!response.ok) {
                throw new Error('Error al obtener visitas');
            }
            return await response.json();
        } catch (error) {
            console.error('Error al obtener visitas:', error);
            throw error;
        }
    },

    /**
     * Obtiene las visitas detalladas por día
     * @param {string} itemId - ID del item de Mercado Libre
     * @param {number} dias - Número de días a consultar (1-150)
     * @returns {Promise} Promise con los datos de visitas por día
     */
    obtenerVisitasPorDia: async function(itemId, dias = 30) {
        try {
            const response = await fetch(`/MercadoLibre/ObtenerVisitasPorDia?itemId=${encodeURIComponent(itemId)}&dias=${dias}`);
            if (!response.ok) {
                throw new Error('Error al obtener visitas por día');
            }
            return await response.json();
        } catch (error) {
            console.error('Error al obtener visitas por día:', error);
            throw error;
        }
    },

    /**
     * Muestra las visitas en un elemento específico del DOM
     * @param {string} itemId - ID del item de Mercado Libre
     * @param {string} elementId - ID del elemento donde mostrar las visitas
     */
    mostrarVisitasEnElemento: function(itemId, elementId) {
        const elemento = document.getElementById(elementId);
        if (!elemento) {
            console.error('Elemento no encontrado:', elementId);
            return;
        }

        elemento.innerHTML = '<span class="text-muted"><i class="bi bi-hourglass-split"></i> Cargando...</span>';

        this.obtenerVisitas(itemId)
            .then(data => {
                if (data.success) {
                    elemento.innerHTML = `
                        <div class="d-flex align-items-center">
                            <i class="bi bi-eye-fill text-primary me-2"></i>
                            <strong>${data.visitasTotales.toLocaleString('es-CL')}</strong>
                            <small class="text-muted ms-2">visitas</small>
                        </div>
                    `;
                    elemento.title = `Últimos 30 días: ${data.visitasUltimos30Dias.toLocaleString('es-CL')} | Últimos 7 días: ${data.visitasUltimos7Dias.toLocaleString('es-CL')}`;
                } else {
                    elemento.innerHTML = '<span class="text-danger"><i class="bi bi-x-circle"></i> Error</span>';
                }
            })
            .catch(error => {
                elemento.innerHTML = '<span class="text-danger"><i class="bi bi-x-circle"></i> Error</span>';
                console.error('Error:', error);
            });
    },

    /**
     * Crea un badge con las visitas para agregar a cualquier elemento
     * @param {string} itemId - ID del item de Mercado Libre
     * @returns {Promise<string>} Promise con el HTML del badge
     */
    crearBadgeVisitas: async function(itemId) {
        try {
            const data = await this.obtenerVisitas(itemId);
            if (data.success) {
                return `<span class="badge bg-primary" title="Visitas en Mercado Libre">
                            <i class="bi bi-eye-fill"></i> ${data.visitasTotales.toLocaleString('es-CL')}
                        </span>`;
            }
            return '<span class="badge bg-secondary"><i class="bi bi-eye-slash"></i></span>';
        } catch (error) {
            return '<span class="badge bg-danger"><i class="bi bi-x"></i></span>';
        }
    },

    /**
     * Verifica el estado de la conexión con Mercado Libre
     * @returns {Promise<boolean>} Promise que indica si hay conexión activa
     */
    verificarConexion: async function() {
        try {
            const response = await fetch('/MercadoLibre/EstadoConexion');
            return response.ok;
        } catch (error) {
            console.error('Error al verificar conexión:', error);
            return false;
        }
    },

    /**
     * Muestra una notificación si no hay conexión con Mercado Libre
     */
    notificarSinConexion: function() {
        Swal.fire({
            icon: 'warning',
            title: 'Sin conexión a Mercado Libre',
            html: 'No hay conexión configurada con Mercado Libre.<br>Por favor, configure la conexión en <strong>Configuración > Mercado Libre</strong>',
            confirmButtonText: 'Ir a Configuración',
            showCancelButton: true,
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                window.location.href = '/MercadoLibre/Index';
            }
        });
    }
};

// Hacer disponible globalmente
window.MercadoLibre = MercadoLibre;

