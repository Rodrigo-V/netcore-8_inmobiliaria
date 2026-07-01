// Configuración IDs de Portales
// Patrón DataTables server-side (igual que Clientes Leads)

let tablaPortales;

$(document).ready(function () {
    inicializarDataTablesPortales();
    configurarEventosPortales();
});

function inicializarDataTablesPortales() {
    tablaPortales = $('#tablaPropiedades').DataTable(DataTablesCommon.baseConfig({
        order: [[0, 'asc']],
        ajax: DataTablesCommon.ajaxPost('/ConfiguracionPortales/GetData'),
        columns: [
            { data: 'iD_Propiedad', name: 'ID_Propiedad' },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    var imagenUrl = row.url_Imagen || row.imagen_Propiedad;
                    if (imagenUrl) {
                        return '<img src="' + imagenUrl + '" style="width: 50px; height: 50px; object-fit: cover; border-radius: 4px;">';
                    }
                    return '<div class="text-center text-muted"><i class="fas fa-image fa-2x"></i></div>';
                }
            },
            { data: 'titulo', name: 'Titulo' },
            { data: 'direccion', name: 'Direccion' },
            { data: 'id_TocToc', name: 'id_TocToc', defaultContent: '' },
            { data: 'id_ChilePropiedades', name: 'id_ChilePropiedades', defaultContent: '' },
            { data: 'id_PortalInmobiliario', name: 'id_PortalInmobiliario', defaultContent: '' },
            { data: 'id_Proppit', name: 'id_Proppit', defaultContent: '' },
            { data: 'id_PortalRosch', name: 'id_PortalRosch', defaultContent: '' },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return '<button class="btn btn-primary btn-sm" onclick="configurarPortales(\'' + row.iD_Propiedad + '\')">' +
                        '<i class="fas fa-cog"></i> Configurar' +
                        '</button>';
                }
            }
        ]
    }));
}

function configurarEventosPortales() {
    $('#formConfiguracionPortales').on('submit', function (e) {
        e.preventDefault();

        var formData = $(this).serialize();

        $.post('/ConfiguracionPortales/GuardarIDsPortales', formData)
            .done(function (response) {
                if (response.success) {
                    var modalEl = document.getElementById('modalConfiguracionPortales');
                    var modal = bootstrap.Modal.getInstance(modalEl);
                    modal.hide();
                    alert('Configuración guardada exitosamente');
                    tablaPortales.ajax.reload(null, false);
                } else {
                    alert('Error al guardar: ' + response.message);
                }
            })
            .fail(function (xhr, status, error) {
                alert('Error al guardar la configuración: ' + error);
            });
    });
}

window.configurarPortales = function (idPropiedad) {
    var datosPropiedad = tablaPortales.rows().data().toArray().find(function (item) {
        return item.iD_Propiedad === idPropiedad;
    });

    if (datosPropiedad) {
        $('#idPropiedadModal').val(idPropiedad);
        $('#infoPropiedadModal').html(
            '<strong>ID:</strong> ' + datosPropiedad.iD_Propiedad + '<br>' +
            '<strong>Código:</strong> ' + (datosPropiedad.codigo_Referencia || 'N/A') + '<br>' +
            '<strong>Título:</strong> ' + (datosPropiedad.titulo || 'N/A') + '<br>' +
            '<strong>Dirección:</strong> ' + (datosPropiedad.direccion || 'N/A') + '<br>' +
            '<strong>Comuna:</strong> ' + (datosPropiedad.comuna || 'N/A')
        );

        cargarIDsPortales(idPropiedad);

        var modal = new bootstrap.Modal(document.getElementById('modalConfiguracionPortales'));
        modal.show();
    }
};

function cargarIDsPortales(idPropiedad) {
    $.get('/ConfiguracionPortales/ObtenerIDsPortales', { idPropiedad: idPropiedad })
        .done(function (response) {
            if (response.success && response.data) {
                $('#idChilePropiedades').val(response.data.id_ChilePropiedades || '');
                $('#idPortalInmobiliario').val(response.data.id_PortalInmobiliario || '');
                $('#idTocToc').val(response.data.id_TocToc || '');
                $('#idProppit').val(response.data.id_Proppit || '');
                $('#idPortalRosch').val(response.data.id_PortalRosch || '');
            }
        })
        .fail(function () {
            alert('Error: No se pudieron cargar los IDs de portales existentes');
        });
}
