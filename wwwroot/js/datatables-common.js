/**
 * Configuración común de DataTables (server-side).
 * Mismo patrón que Clientes Leads en todos los módulos.
 */
window.DataTablesCommon = {
    language: {
        url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json"
    },
    pageLength: 10,

    baseConfig: function (options) {
        return Object.assign({
            processing: true,
            serverSide: true,
            pageLength: this.pageLength,
            language: this.language
        }, options || {});
    },

    ajaxPost: function (url, extraDataFn) {
        var config = {
            url: url,
            type: "POST",
            error: function (xhr) {
                console.error("Error DataTables:", xhr.responseText || xhr.statusText);
                if (typeof Swal !== "undefined") {
                    Swal.fire({
                        icon: "error",
                        title: "Error",
                        text: "Error al cargar los datos de la tabla"
                    });
                }
            }
        };

        if (typeof extraDataFn === "function") {
            config.data = extraDataFn;
        }

        return config;
    }
};
