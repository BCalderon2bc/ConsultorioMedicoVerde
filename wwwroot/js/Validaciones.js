document.addEventListener("DOMContentLoaded", function () {

    /* ===============================
       SOLO LETRAS
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

            if (numeros.length > 8) {
                numeros = numeros.substring(0, 8);
            }

            if (numeros.length > 4) {
                this.value = numeros.substring(0, 4) + '-' + numeros.substring(4);
            } else {
                this.value = numeros;
            }

        });

    });

    /* ===============================
     MÁSCARA CÉDULA
     Formato: 000-000000-0000A
    =============================== */
    document.querySelectorAll(".cedula-ni").forEach(input => {

        input.addEventListener("input", function () {

            let valor = this.value.replace(/[^0-9a-zA-Z]/g, '').toUpperCase();

            // Máximo 14 caracteres reales (sin guiones)
            if (valor.length > 14) {
                valor = valor.substring(0, 14);
            }

            let formato = "";

            if (valor.length > 0) {
                formato += valor.substring(0, 3);
            }
            if (valor.length >= 4) {
                formato += "-" + valor.substring(3, 9);
            }
            if (valor.length >= 10) {
                formato += "-" + valor.substring(9, 13);
            }
            if (valor.length >= 14) {
                formato += valor.substring(13, 14);
            }

            this.value = formato;

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
       VALIDACIÓN VISUAL BOOTSTRAP
    =============================== */
    const forms = document.querySelectorAll("form");

    forms.forEach(form => {
        form.addEventListener("submit", function (event) {

            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }

            form.classList.add("was-validated");

        }, false);
    });

});