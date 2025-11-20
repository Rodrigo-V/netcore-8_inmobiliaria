// JavaScript para Gestión de Usuarios

$(document).ready(function() {
    console.log('🚀 Inicializando Gestión de Usuarios...');
    
    // Inicializar DataTables si existe la tabla
    if ($('#tablaUsuarios').length > 0) {
        $('#tablaUsuarios').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.13.7/i18n/es-ES.json'
            },
            responsive: true,
            order: [[0, 'asc']],
            pageLength: 10,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Todos"]]
        });
        console.log('✅ DataTables inicializado');
    }
});

// Función para eliminar usuario
function eliminarUsuario(idUsuario) {
    console.log('🗑️ Intentando eliminar usuario:', idUsuario);
    
    Swal.fire({
        title: '¿Está seguro?',
        text: 'Esta acción desactivará el usuario y no podrá revertirse',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            // Obtener el token anti-forgery
            const token = $('input[name="__RequestVerificationToken"]').val();
            
            $.ajax({
                url: '/GestionUsuarios/Eliminar',
                type: 'POST',
                data: { 
                    id: idUsuario,
                    __RequestVerificationToken: token
                },
                success: function(response) {
                    console.log('✅ Respuesta recibida:', response);
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Eliminado',
                            text: response.message,
                            timer: 2000
                        }).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: response.message
                        });
                    }
                },
                error: function(xhr, status, error) {
                    console.error('❌ Error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Error al eliminar usuario: ' + error
                    });
                }
            });
        }
    });
}

// Función para cambiar estado del usuario
function cambiarEstado(idUsuario, activo) {
    console.log('🔄 Cambiando estado del usuario:', idUsuario, 'a', activo);
    
    const accion = activo ? 'activar' : 'desactivar';
    
    Swal.fire({
        title: '¿Confirmar cambio?',
        text: `¿Desea ${accion} este usuario?`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: activo ? '#28a745' : '#ffc107',
        cancelButtonColor: '#6c757d',
        confirmButtonText: `Sí, ${accion}`,
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/GestionUsuarios/CambiarEstado',
                type: 'POST',
                data: { 
                    id: idUsuario,
                    activo: activo
                },
                success: function(response) {
                    console.log('✅ Respuesta recibida:', response);
                    if (response.success) {
                        Swal.fire({
                            icon: 'success',
                            title: 'Éxito',
                            text: response.message,
                            timer: 2000
                        }).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: response.message
                        });
                    }
                },
                error: function(xhr, status, error) {
                    console.error('❌ Error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Error al cambiar estado: ' + error
                    });
                }
            });
        }
    });
}

// Validar formulario de creación
if ($('#formCrearUsuario').length > 0) {
    $('#formCrearUsuario').on('submit', function(e) {
        const clave = $('#clave').val();
        const confirmarClave = $('#confirmarClave').val();
        
        if (clave !== confirmarClave) {
            e.preventDefault();
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Las contraseñas no coinciden'
            });
            return false;
        }
        
        if (clave.length < 6) {
            e.preventDefault();
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'La contraseña debe tener al menos 6 caracteres'
            });
            return false;
        }
    });
}

// Validar formulario de edición
if ($('#formEditarUsuario').length > 0) {
    $('#formEditarUsuario').on('submit', function(e) {
        const cambiarClave = $('#cambiarClave').is(':checked');
        
        if (cambiarClave) {
            const nuevaClave = $('#nuevaClave').val();
            const confirmarClave = $('#confirmarClave').val();
            
            if (!nuevaClave || nuevaClave.length < 6) {
                e.preventDefault();
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'La nueva contraseña debe tener al menos 6 caracteres'
                });
                return false;
            }
            
            if (nuevaClave !== confirmarClave) {
                e.preventDefault();
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: 'Las contraseñas no coinciden'
                });
                return false;
            }
        }
    });
}

console.log('✅ Gestión de Usuarios inicializado correctamente');

