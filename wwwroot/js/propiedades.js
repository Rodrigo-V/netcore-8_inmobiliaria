// Regiones de Chile con sus códigos
const regionesChile = [
    { codigo: "15", nombre: "Arica y Parinacota" },
    { codigo: "01", nombre: "Tarapacá" },
    { codigo: "02", nombre: "Antofagasta" },
    { codigo: "03", nombre: "Atacama" },
    { codigo: "04", nombre: "Coquimbo" },
    { codigo: "05", nombre: "Valparaíso" },
    { codigo: "13", nombre: "Metropolitana de Santiago" },
    { codigo: "06", nombre: "O'Higgins" },
    { codigo: "07", nombre: "Maule" },
    { codigo: "16", nombre: "Ñuble" },
    { codigo: "08", nombre: "Biobío" },
    { codigo: "09", nombre: "La Araucanía" },
    { codigo: "14", nombre: "Los Ríos" },
    { codigo: "10", nombre: "Los Lagos" },
    { codigo: "11", nombre: "Aysén" },
    { codigo: "12", nombre: "Magallanes" }
];

$(document).ready(function () {
    console.log('✅ Inicializando módulo de Propiedades...');
    
    // Inicializar DataTable
    loadGridData();
    
    // Event listener para botón agregar propiedad
    $("#btnNuevaPropiedad").on("click", function () {
        abrirModalAgregar();
    });
});

// Variable global para la tabla
var tablaPropiedad = null;

function loadGridData() {
    console.log('📊 Cargando DataTable de propiedades...');

    tablaPropiedad = $('#propiedadTable').DataTable(DataTablesCommon.baseConfig({
        ajax: DataTablesCommon.ajaxPost('/Propiedades/GetData'),
        order: [[0, 'desc']],
        columns: [
            { data: "id_Propiedad", title: "ID" },
            { data: "codigo_Referencia", title: "Código" },
            { data: "title", title: "Título" },
            { data: "tipo_elemento", title: "Tipo" },
            { 
                data: "valor", 
                title: "Precio",
                render: function(data) {
                    return data ? formatearPrecio(data) : '';
                }
            },
            { data: "comuna", title: "Comuna" },
            { data: "dormitorios_Banos", title: "Dorm/Baños" },
            { data: "metros", title: "Metros" },
            { data: "estado", title: "Estado" },
            { data: "agente_Responsable", title: "Agente" },
            {
                data: null,
                title: "Acciones",
                orderable: false,
                render: function (data, type, row) {
                    return `
                        <div class="action-buttons">
                            <i class="fas fa-edit edit" onclick="editarPropiedad('${row.id_Propiedad}')" title="Editar"></i>
                            <i class="fas fa-trash-alt trash" onclick="eliminarPropiedad('${row.id_Propiedad}')" title="Eliminar"></i>
                        </div>
                    `;
                }
            }
        ],
        pageLength: 10,
        lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
        dom: 'Blfrtip',
        buttons: [
            {
                extend: 'print',
                text: '<i class="fas fa-print"></i> Imprimir',
                className: 'buttons-print'
            },
            {
                extend: 'excelHtml5',
                text: '<i class="fas fa-file-excel"></i> Excel',
                className: 'buttons-excel'
            },
            {
                extend: 'csvHtml5',
                text: '<i class="fas fa-file-csv"></i> CSV',
                className: 'buttons-csv'
            },
            {
                extend: 'pdfHtml5',
                text: '<i class="fas fa-file-pdf"></i> PDF',
                className: 'buttons-pdf'
            }
        ],
        drawCallback: function() {
            console.log('✅ Tabla renderizada');
        }
    }));
}

function formatearPrecio(precio) {
    if (!precio) return '';
    // Eliminar caracteres no numéricos excepto punto y coma
    let numero = precio.toString().replace(/[^\d.,]/g, '');
    // Convertir a número y formatear
    if (numero) {
        return '$' + parseFloat(numero).toLocaleString('es-CL');
    }
    return precio;
}

function abrirModalAgregar() {
    console.log('➕ Abriendo modal para agregar propiedad...');
    
    $.ajax({
        url: window.SiteRoot + "Propiedades/AgregarModal",
        type: "GET",
        success: function (response) {
            console.log('✅ Modal de agregar cargado');
            
            Swal.fire({
                title: 'Agregar Propiedad',
                html: response,
                width: '90%',
                background: '#1D1D1D',
                color: '#fff',
                showCancelButton: true,
                confirmButtonText: '<i class="fas fa-save"></i> Guardar',
                cancelButtonText: '<i class="fas fa-times"></i> Cancelar',
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                didOpen: () => {
                    // Cargar catálogos después de que el modal esté en el DOM
                    console.log('📋 Cargando catálogos...');
                    cargarRegiones();
                    cargarCatalogos();
                    configurarEventoRegion();
                },
                preConfirm: () => {
                    return validarYGuardarNuevaPropiedad();
                },
                allowOutsideClick: () => !Swal.isLoading()
            });
        },
        error: function (xhr, status, error) {
            console.error('❌ Error al cargar modal de agregar:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'No se pudo cargar el formulario',
                background: '#1D1D1D',
                color: '#fff'
            });
        }
    });
}

function cargarRegiones() {
    console.log('📍 Cargando regiones...');
    const selectRegion = $('#selNuevoRegion');
    selectRegion.empty();
    selectRegion.append('<option value="">Seleccione región...</option>');
    
    regionesChile.forEach(region => {
        selectRegion.append(`<option value="${region.nombre}">${region.nombre}</option>`);
    });
    
    console.log('✅ Regiones cargadas:', regionesChile.length);
}

function configurarEventoRegion() {
    console.log('⚙️ Configurando evento de región...');
    $('#selNuevoRegion').off('change').on('change', function() {
        const regionNombre = $(this).val();
        console.log('🔄 Región seleccionada:', regionNombre);
        if (regionNombre) {
            cargarComunas(regionNombre);
        } else {
            $('#selNuevoComuna').empty().append('<option value="">Seleccione región primero...</option>').prop('disabled', true);
        }
    });
}

async function cargarComunas(regionNombre) {
    console.log('🏘️ Cargando comunas para:', regionNombre);
    const selectComuna = $('#selNuevoComuna');
    selectComuna.empty().append('<option value="">Cargando comunas...</option>').prop('disabled', true);
    
    try {
        const region = regionesChile.find(r => r.nombre === regionNombre);
        if (!region) {
            console.error('❌ Región no encontrada:', regionNombre);
            return;
        }
        
        console.log('🌐 Llamando API para región:', region.codigo);
        const response = await fetch(`https://apis.digital.gob.cl/dpa/regiones/${region.codigo}/comunas`);
        const comunas = await response.json();
        
        console.log('✅ Comunas obtenidas:', comunas.length);
        
        selectComuna.empty();
        selectComuna.append('<option value="">Seleccione comuna...</option>');
        
        comunas.forEach(comuna => {
            selectComuna.append(`<option value="${comuna.nombre}">${comuna.nombre}</option>`);
        });
        
        selectComuna.prop('disabled', false);
    } catch (error) {
        console.error('❌ Error al cargar comunas:', error);
        selectComuna.empty().append('<option value="">Error al cargar comunas</option>');
    }
}

async function cargarCatalogos() {
    console.log('📦 Cargando catálogos desde el servidor...');
    
    // Cargar tipos de propiedad
    try {
        console.log('🏠 Cargando tipos de propiedad...');
        const urlTipos = window.SiteRoot + 'Propiedades/ObtenerTiposPropiedad';
        console.log('URL:', urlTipos);
        
        const resTipos = await fetch(urlTipos);
        const dataTipos = await resTipos.json();
        
        console.log('Respuesta tipos:', dataTipos);
        
        if (dataTipos.success) {
            const selectTipo = $('#selNuevoTipoPropiedad');
            selectTipo.find('option:not(:first)').remove(); // Limpiar excepto el primer option
            
            dataTipos.data.forEach(tipo => {
                selectTipo.append(`<option value="${tipo}">${tipo}</option>`);
            });
            console.log('✅ Tipos de propiedad cargados:', dataTipos.data.length);
        } else {
            console.error('❌ Error en respuesta de tipos:', dataTipos.message);
        }
    } catch (error) {
        console.error('❌ Error al cargar tipos:', error);
    }
    
    // Cargar estados
    try {
        console.log('📊 Cargando estados...');
        const urlEstados = window.SiteRoot + 'Propiedades/ObtenerEstados';
        console.log('URL:', urlEstados);
        
        const resEstados = await fetch(urlEstados);
        const dataEstados = await resEstados.json();
        
        console.log('Respuesta estados:', dataEstados);
        
        if (dataEstados.success) {
            const selectEstado = $('#selNuevoEstado');
            selectEstado.find('option:not(:first)').remove();
            
            dataEstados.data.forEach(estado => {
                selectEstado.append(`<option value="${estado}">${estado}</option>`);
            });
            
            // Seleccionar "Disponible" por defecto
            selectEstado.val('Disponible');
            console.log('✅ Estados cargados:', dataEstados.data.length);
        } else {
            console.error('❌ Error en respuesta de estados:', dataEstados.message);
        }
    } catch (error) {
        console.error('❌ Error al cargar estados:', error);
    }
    
    // Cargar agentes
    try {
        console.log('👥 Cargando agentes...');
        const urlAgentes = window.SiteRoot + 'Propiedades/ObtenerAgentes';
        console.log('URL:', urlAgentes);
        
        const resAgentes = await fetch(urlAgentes);
        const dataAgentes = await resAgentes.json();
        
        console.log('Respuesta agentes:', dataAgentes);
        
        if (dataAgentes.success) {
            const selectAgente = $('#selNuevoAgenteResponsable');
            selectAgente.find('option:not(:first)').remove();
            
            dataAgentes.data.forEach(agente => {
                selectAgente.append(`<option value="${agente}">${agente}</option>`);
            });
            console.log('✅ Agentes cargados:', dataAgentes.data.length);
        } else {
            console.error('❌ Error en respuesta de agentes:', dataAgentes.message);
        }
    } catch (error) {
        console.error('❌ Error al cargar agentes:', error);
    }
    
    console.log('✅ Carga de catálogos completada');
}

function validarYGuardarNuevaPropiedad() {
    const titulo = $('#txtNuevoTitulo').val();
    const tipoPropiedad = $('#selNuevoTipoPropiedad').val();
    const region = $('#selNuevoRegion').val();
    const estado = $('#selNuevoEstado').val();
    
    // Validaciones
    if (!titulo || titulo.trim() === '') {
        Swal.showValidationMessage('El título es obligatorio');
        return false;
    }
    
    if (!tipoPropiedad) {
        Swal.showValidationMessage('El tipo de propiedad es obligatorio');
        return false;
    }
    
    if (!region) {
        Swal.showValidationMessage('La región es obligatoria');
        return false;
    }
    
    if (!estado) {
        Swal.showValidationMessage('El estado es obligatorio');
        return false;
    }
    
    const propiedad = {
        Title: titulo,
        Tipo_elemento: tipoPropiedad,
        Direccion: $('#txtNuevoDireccion').val(),
        Comuna: $('#selNuevoComuna').val(),
        Region: region,
        Valor: $('#txtNuevoPrecio').val(),
        Dormitorios: $('#selNuevoDormitorios').val(),
        Banos: $('#selNuevoBanos').val(),
        Estado: estado,
        M2_Construidos: $('#txtNuevoMetrosConstruidos').val(),
        M2_Terreno: $('#txtNuevoMetrosTerreno').val(),
        Agente_Responsable: $('#selNuevoAgenteResponsable').val()
    };
    
    console.log('📝 Datos a guardar:', propiedad);
    
    return $.ajax({
        url: window.SiteRoot + "Propiedades/Agregar",
        type: "POST",
        contentType: 'application/json',
        data: JSON.stringify(propiedad),
        success: function (response) {
            if (response.success) {
                console.log('✅ Propiedad agregada exitosamente');
                Swal.fire({
                    icon: 'success',
                    title: 'Éxito',
                    text: response.message || 'Propiedad agregada correctamente',
                    background: '#1D1D1D',
                    color: '#fff'
                });
                tablaPropiedad.ajax.reload();
            } else {
                console.error('❌ Error al agregar:', response.message);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message || 'Error al agregar la propiedad',
                    background: '#1D1D1D',
                    color: '#fff'
                });
            }
        },
        error: function (xhr, status, error) {
            console.error('❌ Error AJAX al agregar:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Error al comunicarse con el servidor',
                background: '#1D1D1D',
                color: '#fff'
            });
        }
    });
}

function editarPropiedad(idPropiedad) {
    console.log('✏️ Editando propiedad:', idPropiedad);
    
    $.ajax({
        url: window.SiteRoot + "Propiedades/ObtenerPorId",
        type: "POST",
        data: { idPropiedad: idPropiedad },
        success: function (response) {
            console.log('✅ Datos de propiedad cargados');
            
            Swal.fire({
                title: 'Modificar Propiedad',
                html: response,
                width: '90%',
                background: '#1D1D1D',
                color: '#fff',
                showCancelButton: true,
                confirmButtonText: '<i class="fas fa-save"></i> Guardar Cambios',
                cancelButtonText: '<i class="fas fa-times"></i> Cancelar',
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                didOpen: async () => {
                    // Obtener valores originales desde los campos ocultos
                    const tipoOriginal = $('#hiddenTipoPropiedad').val() || '';
                    const regionOriginal = $('#hiddenRegion').val() || '';
                    const comunaOriginal = $('#hiddenComuna').val() || '';
                    const estadoOriginal = $('#hiddenEstado').val() || '';
                    const agenteOriginal = $('#hiddenAgenteResponsable').val() || '';
                    const dormitoriosOriginal = $('#hiddenDormitorios').val() || '';
                    const banosOriginal = $('#hiddenBanos').val() || '';
                    
                    console.log('📋 Valores originales a preseleccionar:', {
                        tipo: tipoOriginal,
                        region: regionOriginal,
                        comuna: comunaOriginal,
                        estado: estadoOriginal,
                        agente: agenteOriginal,
                        dormitorios: dormitoriosOriginal,
                        banos: banosOriginal
                    });
                    
                    // Cargar catálogos y preseleccionar
                    await cargarCatalogosEdicion(
                        tipoOriginal,
                        regionOriginal,
                        comunaOriginal,
                        estadoOriginal,
                        agenteOriginal,
                        dormitoriosOriginal,
                        banosOriginal
                    );
                },
                preConfirm: () => {
                    return validarYActualizarPropiedad();
                },
                allowOutsideClick: () => !Swal.isLoading()
            });
        },
        error: function (xhr, status, error) {
            console.error('❌ Error al cargar propiedad:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'No se pudieron cargar los datos de la propiedad',
                background: '#1D1D1D',
                color: '#fff'
            });
        }
    });
}

async function cargarCatalogosEdicion(tipoOriginal, regionOriginal, comunaOriginal, estadoOriginal, agenteOriginal, dormitoriosOriginal, banosOriginal) {
    console.log('📦 Cargando catálogos para edición...');
    console.log('Valores a preseleccionar:', { tipoOriginal, regionOriginal, comunaOriginal, estadoOriginal, agenteOriginal, dormitoriosOriginal, banosOriginal });
    
    try {
        // 1. Cargar regiones
        console.log('📍 Cargando regiones...');
        const selectRegion = $('#selRegion');
        selectRegion.empty();
        selectRegion.append('<option value="">Seleccione región...</option>');
        
        regionesChile.forEach(region => {
            selectRegion.append(`<option value="${region.nombre}">${region.nombre}</option>`);
        });
        
        // 2. Cargar tipos de propiedad
        console.log('🏠 Cargando tipos de propiedad...');
        const resTipos = await fetch(window.SiteRoot + 'Propiedades/ObtenerTiposPropiedad');
        const dataTipos = await resTipos.json();
        
        if (dataTipos.success) {
            const selectTipo = $('#selTipoPropiedad');
            selectTipo.find('option:not(:first)').remove();
            
            dataTipos.data.forEach(tipo => {
                selectTipo.append(`<option value="${tipo}">${tipo}</option>`);
            });
            
            // Preseleccionar tipo
            if (tipoOriginal) {
                selectTipo.val(tipoOriginal);
                console.log('✅ Tipo preseleccionado:', tipoOriginal);
            }
        }
        
        // 3. Cargar estados
        console.log('📊 Cargando estados...');
        const resEstados = await fetch(window.SiteRoot + 'Propiedades/ObtenerEstados');
        const dataEstados = await resEstados.json();
        
        if (dataEstados.success) {
            const selectEstado = $('#selEstado');
            selectEstado.find('option:not(:first)').remove();
            
            dataEstados.data.forEach(estado => {
                selectEstado.append(`<option value="${estado}">${estado}</option>`);
            });
            
            // Preseleccionar estado
            if (estadoOriginal) {
                selectEstado.val(estadoOriginal);
                console.log('✅ Estado preseleccionado:', estadoOriginal);
            }
        }
        
        // 4. Cargar agentes
        console.log('👥 Cargando agentes...');
        const resAgentes = await fetch(window.SiteRoot + 'Propiedades/ObtenerAgentes');
        const dataAgentes = await resAgentes.json();
        
        if (dataAgentes.success) {
            const selectAgente = $('#selAgenteResponsable');
            selectAgente.find('option:not(:first)').remove();
            
            dataAgentes.data.forEach(agente => {
                selectAgente.append(`<option value="${agente}">${agente}</option>`);
            });
            
            // Preseleccionar agente
            if (agenteOriginal) {
                selectAgente.val(agenteOriginal);
                console.log('✅ Agente preseleccionado:', agenteOriginal);
            }
        }
        
        // 5. Preseleccionar dormitorios y baños
        if (dormitoriosOriginal) {
            $('#selDormitorios').val(dormitoriosOriginal);
            console.log('✅ Dormitorios preseleccionados:', dormitoriosOriginal);
        }
        
        if (banosOriginal) {
            $('#selBanos').val(banosOriginal);
            console.log('✅ Baños preseleccionados:', banosOriginal);
        }
        
        // 6. Configurar evento de cambio de región
        $('#selRegion').off('change').on('change', function() {
            const regionNombre = $(this).val();
            console.log('🔄 Región cambiada a:', regionNombre);
            if (regionNombre) {
                cargarComunasEdicion(regionNombre);
            } else {
                $('#selComuna').empty().append('<option value="">Seleccione región primero...</option>').prop('disabled', true);
            }
        });
        
        // 7. Cargar comunas si hay región
        if (regionOriginal) {
            selectRegion.val(regionOriginal);
            console.log('✅ Región preseleccionada:', regionOriginal);
            
            await cargarComunasEdicion(regionOriginal, comunaOriginal);
        }
        
        console.log('✅ Catálogos cargados y valores preseleccionados');
        
    } catch (error) {
        console.error('❌ Error al cargar catálogos:', error);
    }
}

async function cargarComunasEdicion(regionNombre, comunaAPreseleccionar) {
    console.log('🏘️ Cargando comunas para edición:', regionNombre);
    const selectComuna = $('#selComuna');
    selectComuna.empty().append('<option value="">Cargando comunas...</option>').prop('disabled', true);
    
    try {
        const region = regionesChile.find(r => r.nombre === regionNombre);
        if (!region) {
            console.error('❌ Región no encontrada:', regionNombre);
            return;
        }
        
        const response = await fetch(`https://apis.digital.gob.cl/dpa/regiones/${region.codigo}/comunas`);
        const comunas = await response.json();
        
        console.log('✅ Comunas obtenidas:', comunas.length);
        
        selectComuna.empty();
        selectComuna.append('<option value="">Seleccione comuna...</option>');
        
        comunas.forEach(comuna => {
            selectComuna.append(`<option value="${comuna.nombre}">${comuna.nombre}</option>`);
        });
        
        selectComuna.prop('disabled', false);
        
        // Preseleccionar comuna si existe
        if (comunaAPreseleccionar) {
            selectComuna.val(comunaAPreseleccionar);
            console.log('✅ Comuna preseleccionada:', comunaAPreseleccionar);
        }
        
    } catch (error) {
        console.error('❌ Error al cargar comunas:', error);
        selectComuna.empty().append('<option value="">Error al cargar comunas</option>');
    }
}

function validarYActualizarPropiedad() {
    const idPropiedad = $('#txtIdPropiedad').val();
    const titulo = $('#txtTitulo').val();
    const tipoPropiedad = $('#selTipoPropiedad').val();
    const region = $('#selRegion').val();
    const estado = $('#selEstado').val();
    
    // Validaciones
    if (!titulo || titulo.trim() === '') {
        Swal.showValidationMessage('El título es obligatorio');
        return false;
    }
    
    if (!tipoPropiedad) {
        Swal.showValidationMessage('El tipo de propiedad es obligatorio');
        return false;
    }
    
    if (!region) {
        Swal.showValidationMessage('La región es obligatoria');
        return false;
    }
    
    if (!estado) {
        Swal.showValidationMessage('El estado es obligatorio');
        return false;
    }
    
    const propiedad = {
        ID_Propiedad: idPropiedad,
        Title: titulo,
        Tipo_elemento: tipoPropiedad,
        Direccion: $('#txtDireccion').val(),
        Comuna: $('#selComuna').val(),
        Region: region,
        Valor: $('#txtPrecio').val(),
        Dormitorios: $('#selDormitorios').val(),
        Banos: $('#selBanos').val(),
        Estado: estado,
        M2_Construidos: $('#txtMetrosConstruidos').val(),
        M2_Terreno: $('#txtMetrosTerreno').val(),
        Agente_Responsable: $('#selAgenteResponsable').val()
    };
    
    console.log('📝 Actualizando propiedad:', propiedad);
    
    return $.ajax({
        url: window.SiteRoot + "Propiedades/Actualizar",
        type: "POST",
        contentType: 'application/json',
        data: JSON.stringify(propiedad),
        success: function (response) {
            if (response.success) {
                console.log('✅ Propiedad actualizada exitosamente');
                Swal.fire({
                    icon: 'success',
                    title: 'Éxito',
                    text: response.message || 'Propiedad actualizada correctamente',
                    background: '#1D1D1D',
                    color: '#fff'
                });
                tablaPropiedad.ajax.reload();
            } else {
                console.error('❌ Error al actualizar:', response.message);
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message || 'Error al actualizar la propiedad',
                    background: '#1D1D1D',
                    color: '#fff'
                });
            }
        },
        error: function (xhr, status, error) {
            console.error('❌ Error AJAX al actualizar:', error);
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Error al comunicarse con el servidor',
                background: '#1D1D1D',
                color: '#fff'
            });
        }
    });
}

function eliminarPropiedad(idPropiedad) {
    console.log('🗑️ Eliminando propiedad:', idPropiedad);
    
    Swal.fire({
        title: '¿Está seguro?',
        text: `¿Desea eliminar la propiedad ${idPropiedad}? Esta acción no se puede deshacer.`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-trash-alt"></i> Sí, eliminar',
        cancelButtonText: '<i class="fas fa-times"></i> Cancelar',
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        background: '#1D1D1D',
        color: '#fff'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: window.SiteRoot + "Propiedades/Eliminar",
                type: "POST",
                data: { idPropiedad: idPropiedad },
                success: function (response) {
                    if (response.success) {
                        console.log('✅ Propiedad eliminada exitosamente');
                        Swal.fire({
                            icon: 'success',
                            title: 'Eliminada',
                            text: response.message || 'Propiedad eliminada correctamente',
                            background: '#1D1D1D',
                            color: '#fff'
                        });
                        tablaPropiedad.ajax.reload();
                    } else {
                        console.error('❌ Error al eliminar:', response.message);
                        Swal.fire({
                            icon: 'error',
                            title: 'Error',
                            text: response.message || 'Error al eliminar la propiedad',
                            background: '#1D1D1D',
                            color: '#fff'
                        });
                    }
                },
                error: function (xhr, status, error) {
                    console.error('❌ Error AJAX al eliminar:', error);
                    Swal.fire({
                        icon: 'error',
                        title: 'Error',
                        text: 'Error al comunicarse con el servidor',
                        background: '#1D1D1D',
                        color: '#fff'
                    });
                }
            });
        }
    });
}

