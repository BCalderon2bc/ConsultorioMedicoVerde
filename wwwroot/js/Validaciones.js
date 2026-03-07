document.addEventListener("DOMContentLoaded", function () {

    /* ===============================
       SOLO LETRAS (Nombres, Apellidos)
    =============================== */
    document.querySelectorAll(".solo-letras").forEach(input => {
        input.addEventListener("input", function () {
            this.value = this.value.replace(/[^A-Za-zÁÉÍÓÚáéíóúÑñ ]/g, '');
        });
    });

    /* ===============================
       SOLO NÚMEROS
    =============================== */
    document.querySelectorAll(".solo-numeros").forEach(input => {
        input.addEventListener("input", function () {
            this.value = this.value.replace(/[^0-9]/g, '');
        });
    });

    /* ===============================
       MÁSCARA TELÉFONO 0000-0000
    =============================== */
    document.querySelectorAll(".telefono-mask").forEach(input => {
        input.addEventListener("input", function () {
            let numeros = this.value.replace(/\D/g, '');
            if (numeros.length > 8) numeros = numeros.substring(0, 8);

            if (numeros.length > 4) {
                this.value = numeros.substring(0, 4) + '-' + numeros.substring(4);
            } else {
                this.value = numeros;
            }
        });
    });

    /* ==========================================
     MÁSCARA CÉDULA NICARAGÜENSE ESTRICTA
     Formato: 000-000000-0000A
  ========================================== */
    document.querySelectorAll(".cedula-ni").forEach(input => {
        input.addEventListener("input", function () {
            // 1. Limpiamos el valor de guiones y convertimos a Mayúsculas
            let valorRaw = this.value.replace(/-/g, '').toUpperCase();

            // 2. Separamos la parte numérica (primeros 13) de la letra (carácter 14)
            let soloNumeros = valorRaw.substring(0, 13).replace(/\D/g, ''); // \D quita todo lo que NO sea número
            let soloLetra = valorRaw.substring(13, 14).replace(/[^A-Z]/g, ''); // Solo letras para el final

            let valorSaneado = soloNumeros + soloLetra;

            // 3. Aplicamos el formato con guiones según la longitud
            let formato = "";
            if (valorSaneado.length > 0) {
                formato += valorSaneado.substring(0, 3);
            }
            if (valorSaneado.length > 3) {
                formato += "-" + valorSaneado.substring(3, 9);
            }
            if (valorSaneado.length > 9) {
                formato += "-" + valorSaneado.substring(9, 13);
            }
            if (valorSaneado.length > 13) {
                formato += valorSaneado.substring(13, 14);
            }

            this.value = formato;
        });

        // Manejo de pegado (paste)
        input.addEventListener("paste", function () {
            setTimeout(() => { this.dispatchEvent(new Event('input')); }, 0);
        });
    });

    /* ===============================
       VALIDACIÓN CORREO EN VIVO
    =============================== */
    document.querySelectorAll(".correo-valido").forEach(input => {
        input.addEventListener("input", function () {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (this.value === "") {
                this.classList.remove("is-invalid", "is-valid");
                return;
            }
            if (emailRegex.test(this.value)) {
                this.classList.remove("is-invalid");
                this.classList.add("is-valid");
            } else {
                this.classList.remove("is-valid");
                this.classList.add("is-invalid");
            }
        });
    });

    /* ===============================
       VALIDACIÓN VISUAL BOOTSTRAP Y SUBMIT
    =============================== */

    // 1. Evitar que "Enter" envíe el formulario accidentalmente
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && e.target.tagName === 'INPUT') {
                e.preventDefault();
                return false;
            }
        });
    });

    // 2. Lógica de Envío (Submit)
    const forms = document.querySelectorAll("form");
    forms.forEach(form => {
        form.addEventListener("submit", function (event) {

            if (!form.checkValidity()) {
                // CASO: FORMULARIO INVÁLIDO
                event.preventDefault();
                event.stopPropagation();

                Swal.fire({
                    title: 'Campos incompletos',
                    text: 'Por favor, revise los campos marcados en rojo antes de continuar.',
                    icon: 'warning',
                    confirmButtonColor: '#208b3a'
                });
            } else {
                // CASO: FORMULARIO VÁLIDO (Aquí deshabilitamos el botón)
                const btn = form.querySelector('button[type="submit"]');
                if (btn) {
                    btn.disabled = true;
                    btn.innerHTML = '<span class="spinner-border spinner-border-sm"></span> Cargando...';
                }
            }

            form.classList.add("was-validated");
        }, false);
    });

    /* ===============================
       CONTROL DE CHECKS (is-valid)
    =============================== */
    const inputsManuales = document.querySelectorAll(".solo-letras, .solo-numeros, .cedula-ni, .telefono-mask, .form-control");

    inputsManuales.forEach(input => {
        input.addEventListener("input", function () {
            // Si está vacío, limpiamos todo
            if (this.value.trim() === "") {
                this.classList.remove("is-valid");
                this.classList.remove("is-invalid");
            }
            // Si cumple con los requisitos (required, minlength, etc.)
            else if (this.checkValidity()) {
                this.classList.remove("is-invalid");
                this.classList.add("is-valid");
            }
            // Si no cumple
            else {
                this.classList.remove("is-valid");
                this.classList.add("is-invalid");
            }
        });
    });
});