const FiltroFechas = {
    init: function (idInicio, idFin) {
        const inputInicio = document.getElementById(idInicio);
        const inputFin = document.getElementById(idFin);
        const hoy = new Date().toISOString().split('T')[0];

        if (!inputInicio || !inputFin) return;

        const sincronizar = () => {
            // La fecha inicio no puede superar a la fecha fin actual
            if (inputFin.value) {
                inputInicio.max = inputFin.value;
            } else {
                inputInicio.max = hoy;
            }

            // La fecha fin no puede ser anterior a la fecha inicio actual
            if (inputInicio.value) {
                inputFin.min = inputInicio.value;
            }

            // Ambas siempre limitadas por el día de hoy
            inputFin.max = hoy;
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