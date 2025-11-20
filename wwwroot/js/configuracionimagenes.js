// JavaScript para Configuración de Imágenes

$(document).ready(function() {
    console.log('🚀 Inicializando Configuración de Imágenes...');
    
    // Búsqueda de propiedades
    let timeoutBusqueda;
    $('#buscarPropiedad').on('input', function() {
        clearTimeout(timeoutBusqueda);
        const termino = $(this).val();
        
        timeoutBusqueda = setTimeout(function() {
            buscarPropiedades(termino);
        }, 500);
    });
});

function buscarPropiedades(termino) {
    console.log('🔍 Buscando propiedades:', termino);
    
    $.ajax({
        url: '/ConfiguracionImagenes/BuscarPropiedades',
        type: 'POST',
        data: { termino: termino },
        success: function(response) {
            if (response.success) {
                actualizarGrid(response.propiedades);
            }
        },
        error: function(error) {
            console.error('❌ Error al buscar:', error);
        }
    });
}

function actualizarGrid(propiedades) {
    const grid = $('#gridPropiedades');
    grid.empty();
    
    propiedades.forEach(function(prop) {
        const tieneImagen = prop.url_Imagen || prop.Url_Imagen;
        const idProp = prop.iD_Propiedad || prop.ID_Propiedad;
        const titulo = prop.title || prop.Title;
        const comuna = prop.comuna || prop.Comuna;
        
        const card = `
            <div class="property-card" data-id="${idProp}">
                <div class="property-image">
                    ${tieneImagen 
                        ? `<img src="${tieneImagen}" alt="${titulo}" style="width: 100%; height: 200px; object-fit: cover;">`
                        : '<i class="fas fa-image"></i>'
                    }
                </div>
                <div class="property-info">
                    <div class="property-id">${idProp}</div>
                    <div class="property-title">${titulo}</div>
                    <div class="property-details">
                        <small><i class="fas fa-map-marker-alt"></i> ${comuna}</small>
                    </div>
                    <div class="property-actions">
                        <button onclick="editarUrl('${idProp}')" class="btn btn-sm btn-primary">
                            <i class="fas fa-edit"></i> URL
                        </button>
                        ${tieneImagen 
                            ? `<button onclick="eliminarImagen('${idProp}')" class="btn btn-sm btn-danger">
                                <i class="fas fa-trash"></i>
                              </button>`
                            : ''
                        }
                    </div>
                </div>
            </div>
        `;
        
        grid.append(card);
    });
}

function editarUrl(idPropiedad) {
    console.log('📝 Editando URL para:', idPropiedad);
    
    Swal.fire({
        title: 'Actualizar URL de Imagen',
        html: `
            <div style="text-align: left;">
                <p><strong>ID Propiedad:</strong> ${idPropiedad}</p>
                <label style="display: block; margin-top: 15px; margin-bottom: 5px;">URL de Google Drive:</label>
                <input type="text" id="urlImagen" class="swal2-input" style="width: 90%;" placeholder="https://drive.google.com/file/d/...">
                <small style="display: block; margin-top: 5px; color: #666;">Pega la URL de compartir de Google Drive</small>
            </div>
        `,
        showCancelButton: true,
        confirmButtonText: 'Guardar',
        cancelButtonText: 'Cancelar',
        preConfirm: () => {
            const url = document.getElementById('urlImagen').value;
            if (!url) {
                Swal.showValidationMessage('Por favor ingrese una URL');
                return false;
            }
            return url;
        }
    }).then((result) => {
        if (result.isConfirmed) {
            guardarUrlImagen(idPropiedad, result.value);
        }
    });
}

function guardarUrlImagen(idPropiedad, urlImagen) {
    $.ajax({
        url: '/ConfiguracionImagenes/ActualizarUrlImagen',
        type: 'POST',
        data: { 
            idPropiedad: idPropiedad,
            urlImagen: urlImagen
        },
        success: function(response) {
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
        error: function(error) {
            console.error('❌ Error:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Error al guardar la URL'
            });
        }
    });
}

function eliminarImagen(idPropiedad) {
    Swal.fire({
        title: '¿Eliminar imagen?',
        text: 'Se eliminará la URL de la imagen de esta propiedad',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/ConfiguracionImagenes/EliminarImagen',
                type: 'POST',
                data: { idPropiedad: idPropiedad },
                success: function(response) {
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
                error: function(error) {
                    console.error('❌ Error:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Error al eliminar la imagen'
                    });
                }
            });
        }
    });
}

console.log('✅ Configuración de Imágenes inicializado');

