// Validaciones y mejoras de UX para Inmobiliaria
(function () {
    'use strict';

    // Confirmación para eliminaciones
    document.addEventListener('DOMContentLoaded', function () {
        const deleteButtons = document.querySelectorAll('form[action*="Delete"] button[type="submit"]');
        
        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('¿Está seguro de que desea eliminar este registro? Esta acción no se puede deshacer.')) {
                    e.preventDefault();
                }
            });
        });

        // Mostrar mensajes temporales
        const alertMessages = document.querySelectorAll('.alert');
        alertMessages.forEach(alert => {
            setTimeout(() => {
                alert.style.transition = 'opacity 0.5s ease';
                alert.style.opacity = '0';
                setTimeout(() => alert.remove(), 500);
            }, 5000);
        });

        // Mejorar búsqueda con debounce
        const searchInput = document.querySelector('input[name="search"]');
        if (searchInput) {
            let timeout = null;
            searchInput.addEventListener('input', function () {
                clearTimeout(timeout);
                timeout = setTimeout(() => {
                    this.form.submit();
                }, 500);
            });
        }
    });

    // Validación de DNI
    function validarDNI(dni) {
        const dniRegex = /^\d{7,8}$/;
        return dniRegex.test(dni);
    }

    // Validación de email
    function validarEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    // Validación de teléfono
    function validarTelefono(telefono) {
        const telefonoRegex = /^[0-9+\-\s()]{10,20}$/;
        return telefonoRegex.test(telefono);
    }

    // Exponer funciones globalmente
    window.validaciones = {
        validarDNI,
        validarEmail,
        validarTelefono
    };
})();