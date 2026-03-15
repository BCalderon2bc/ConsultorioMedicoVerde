const FiltroFechas = {
    init: function (idInicio, idFin) {
        const inputInicio = document.getElementById(idInicio);
        const inputFin = document.getElementById(idFin);

        if (!inputInicio || !inputFin) return;

        const sincronizar = () => {
            // Fecha Inicio no supere a Fecha Fin
            if (inputFin.value) {
                inputInicio.max = inputFin.value;
            } else {
                // Si no hay fecha fin, el inicio es libre
                inputInicio.removeAttribute("max");
            }

            // Y que Fin no sea menor que Inicio
            if (inputInicio.value) {
                inputFin.min = inputInicio.value;
            } else {
                inputFin.removeAttribute("min");
            }
        };

        inputFin.addEventListener("change", () => {
            sincronizar();
            if (inputInicio.value && inputInicio.value > inputFin.value) {
                inputInicio.value = inputFin.value;
            }
        });

        inputInicio.addEventListener("change", () => {
            sincronizar();
            if (inputFin.value && inputFin.value < inputInicio.value) {
                inputFin.value = inputInicio.value;
            }
        });

        sincronizar();
    }
};