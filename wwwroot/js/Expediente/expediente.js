document.addEventListener("DOMContentLoaded", function () {

    const btnBuscar = document.getElementById("btnBuscarPaciente");
    const txtBuscar = document.getElementById("txtBuscarPaciente");
    const btnReporte = document.getElementById("btnGenerarReporte");

    let pacienteActualId = null;

    /* ================================
       BUSCAR PACIENTE
    ================================== */
    btnBuscar.addEventListener("click", function () {
        const filtro = txtBuscar.value.trim();
        if (!filtro) return alert("Ingrese un criterio de búsqueda");

        const filtroRequest = { Filtro: filtro };

        $.ajax({
            url: "/Expedientes/ObtenerExpediente",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(filtroRequest),
            success: function (data) {
                if (!data || !data.paciente) {
                    message.success("Paciente no encontrado");
                    return;
                }

                pacienteActualId = data.paciente.idPaciente;
                mostrarPaciente(data.paciente);
                renderConsultas(data.consultas);
                renderCitas(data.citas);
                renderRecetas(data.recetas);
                renderHistorial(data.historial);

                $("#cardPaciente").removeClass("d-none");
                $("#cardExpediente").removeClass("d-none");
            },
            error: function (xhr, status, error) {
                console.error("Error AJAX:", status, error, xhr.responseText);
                message.err("Ocurrió un error al obtener el expediente");
            }
        });
    });

    /* ================================
       MOSTRAR DATOS GENERALES
    ================================== */
    function mostrarPaciente(p) {
        document.getElementById("lblNombre").innerText = p.nombre;
        document.getElementById("lblEdad").innerText = calcularEdad(p.fechaNacimiento) + " años";
        document.getElementById("lblGenero").innerText = p.genero;
        document.getElementById("lblTelefono").innerText = p.telefono;
        document.getElementById("lblCorreo").innerText = p.correo;
        document.getElementById("lblDireccion").innerText = p.direccion;
    }

    /* ================================
      CALCULAR EDAD 
        ================================== */
    function calcularEdad(fechaNacimiento) {
        const hoy = new Date();
        const nacimiento = new Date(fechaNacimiento);

        let edad = hoy.getFullYear() - nacimiento.getFullYear();
        const mes = hoy.getMonth() - nacimiento.getMonth();

        if (mes < 0 || (mes === 0 && hoy.getDate() < nacimiento.getDate())) {
            edad--;
        }

        return edad;
    }

    /* ================================
       CONSULTAS
    ================================== */
    function renderConsultas(consultas) {

        const contenedor = document.getElementById("contenedorConsultas");
        contenedor.innerHTML = "";

        if (!consultas || consultas.length === 0) {
            contenedor.innerHTML = "<p class='text-muted'>No hay consultas registradas.</p>";
            return;
        }

        consultas.forEach(c => {

            const card = `
                <div class="card shadow-sm border-0 mb-4 border-start border-success border-5">
                    <div class="card-header bg-white d-flex justify-content-between align-items-center py-3">
                        <span class="badge bg-success">
                            <i class="bi bi-calendar-event me-1"></i>
                            ${formatearFecha(c.fechaConsulta)}
     
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label class="fw-bold text-verde">
                                    <i class="bi bi-activity me-1"></i> Diagnóstico:
                                </label>
                                <p class="border-bottom pb-2">${c.diagnostico ?? "-"}</p>
                            </div>
                            <div class="col-md-6 mb-3">
                                <label class="fw-bold text-verde">
                                    <i class="bi bi-capsule me-1"></i> Tratamiento:
                                </label>
                                <p class="border-bottom pb-2">${c.tratamiento ?? "-"}</p>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            contenedor.innerHTML += card;
        });
    }


    /* ================================
       CITAS
    ================================== */
    function renderCitas(citas) {

        const tbody = document.getElementById("tblCitas");
        tbody.innerHTML = "";

        if (!citas || citas.length === 0) {
            tbody.innerHTML = "<tr><td colspan='4' class='text-center'>Sin registros</td></tr>";
            return;
        }

        citas.forEach(c => {
            tbody.innerHTML += `
                <tr>
                    <td>${formatearFecha(c.fechaCita)}</td>
                    <td>${c.motivo}</td>
                    <td>${c.estado}</td>
                    <td>${c.nombreMedico}</td>
                </tr>
            `;
        });
    }


    /* ================================
       RECETAS
    ================================== */
    function renderRecetas(recetas) {

        const tbody = document.getElementById("tblRecetas");
        tbody.innerHTML = "";

        if (!recetas || recetas.length === 0) {
            tbody.innerHTML = "<tr><td colspan='5' class='text-center'>Sin registros</td></tr>";
            return;
        }

        recetas.forEach(r => {
            tbody.innerHTML += `
                <tr>
                    <td>${r.medicamento}</td>
                    <td>${r.dosis}</td>
                    <td>${r.frecuencia}</td>
                    <td>${formatearFecha(r.fechaCreacion)}</td>
                </tr>
            `;
        });
    }

    /* ================================
       HISTORIAL MÉDICO
    ================================== */
    function renderHistorial(historial) {

        const tbody = document.getElementById("tblHistorial");
        tbody.innerHTML = "";

        if (!historial || historial.length === 0) {
            tbody.innerHTML = "<tr><td colspan='4' class='text-center'>Sin historial registrado</td></tr>";
            return;
        }

        historial.forEach(h => {
            tbody.innerHTML += `
                <tr>
                    <td>${h.alergias ?? "-"}</td>
                    <td>${h.enfermedadesPrevias ?? "-"}</td>
                    <td>${h.cirugiasPrevias ?? "-"}</td>
                    <td>${h.observaciones ?? "-"}</td>
                </tr>
            `;
        });
    }

    /* ================================
       GENERAR REPORTE (Corregido)
    ================================== */
    //btnReporte.addEventListener("click", function () {

    //    if (!pacienteActualId || pacienteActualId === "0") {
    //        // Usamos SweetAlert para un error elegante
    //        Swal.fire({
    //            title: "Paciente no seleccionado",
    //            text: "Por favor, busque y seleccione un paciente para generar su expediente.",
    //            icon: "warning",
    //            confirmButtonColor: "#208b3a"
    //        });
    //        return;
    //    }

    //    // Usamos el Diálogo Global para confirmar la descarga
    //    mostrarDialogoConfirmacion({
    //        titulo: '¿Generar Expediente PDF?',
    //        texto: 'Se abrirá el reporte clínico en una nueva pestaña.',
    //        icono: 'question',
    //        textoConfirmar: '<i class="bi bi-file-pdf"></i> Generar PDF',
    //        textoCancelar: 'Cancelar',
    //        callbackConfirmar: function () {
    //            // Construimos la URL correctamente
    //            const url = `/Expediente/GenerarReporte?idPaciente=${pacienteActualId}`;
    //            window.open(url, "_blank");
    //        }
    //    });
    //});

    /* ================================
       FORMATEAR FECHA
    ================================== */
    function formatearFecha(fecha) {
        if (!fecha) return "-";
        return new Date(fecha).toLocaleDateString();
    }

});