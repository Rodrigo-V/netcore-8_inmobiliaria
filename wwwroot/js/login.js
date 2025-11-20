$(document).ready(function() {
    // Manejar envío del formulario
    $('#loginForm').on('submit', function(e) {
        e.preventDefault();
        realizarLogin();
    });

    // Enter key en los campos
    $('.form-control').on('keypress', function(e) {
        if (e.which === 13) {
            realizarLogin();
        }
    });
});

function realizarLogin() {
    var $btnLogin = $('#btnLogin');
    var $btnText = $('#btnText');
    var $loadingSpinner = $('#loadingSpinner');
    var $errorMessage = $('#errorMessage');
    var $successMessage = $('#successMessage');

    // Validar campos
    var email = $('input[name="Correo_Electronico"]').val().trim();
    var password = $('input[name="Clave"]').val();

    if (!email || !password) {
        mostrarError('Por favor, complete todos los campos.');
        return;
    }

    if (!validarEmail(email)) {
        mostrarError('Por favor, ingrese un correo electrónico válido.');
        return;
    }

    // Deshabilitar botón y mostrar loading
    $btnLogin.prop('disabled', true);
    $loadingSpinner.removeClass('d-none');
    $btnText.text('Iniciando...');
    
    // Ocultar mensajes previos
    $errorMessage.addClass('d-none');
    $successMessage.addClass('d-none');

    // Obtener el formulario y sus datos
    var formData = $('#loginForm').serialize();

    // Realizar petición AJAX
    $.ajax({
        url: '/Autenticacion/Login',
        type: 'POST',
        data: formData,
        success: function(response) {
            if (response.success) {
                mostrarExito(response.message);
                setTimeout(function() {
                    window.location.href = response.redirectUrl;
                }, 1500);
            } else {
                mostrarError(response.message);
                resetearBoton();
            }
        },
        error: function(xhr, status, error) {
            var message = 'Error de conexión. Por favor, intente nuevamente.';
            if (xhr.responseJSON && xhr.responseJSON.message) {
                message = xhr.responseJSON.message;
            }
            mostrarError(message);
            resetearBoton();
        }
    });
}

function resetearBoton() {
    var $btnLogin = $('#btnLogin');
    var $btnText = $('#btnText');
    var $loadingSpinner = $('#loadingSpinner');

    $btnLogin.prop('disabled', false);
    $loadingSpinner.addClass('d-none');
    $btnText.text('Iniciar Sesión');
}

function mostrarError(mensaje) {
    var $errorMessage = $('#errorMessage');
    $errorMessage.text(mensaje).removeClass('d-none');
    $('.login-container').addClass('shake');
    setTimeout(function() {
        $('.login-container').removeClass('shake');
    }, 600);
}

function mostrarExito(mensaje) {
    var $successMessage = $('#successMessage');
    $successMessage.text(mensaje).removeClass('d-none');
}

function validarEmail(email) {
    var re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

function mostrarMensajeRecuperacion() {
    alert('Para recuperar su contraseña, por favor contacte al administrador del sistema.');
}

