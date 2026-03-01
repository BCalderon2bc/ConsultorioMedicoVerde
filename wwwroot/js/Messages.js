window.AppMessages = {

    success: function (message, title = "Éxito") {
        Swal.fire({
            icon: "success",
            title: title,
            text: message,
            confirmButtonColor: "#198754"
        });
    },

    error: function (message, title = "Error") {
        Swal.fire({
            icon: "error",
            title: title,
            text: message,
            confirmButtonColor: "#dc3545"
        });
    },

    warning: function (message, title = "Advertencia") {
        Swal.fire({
            icon: "warning",
            title: title,
            text: message,
            confirmButtonColor: "#ffc107"
        });
    },

    confirm: function (message, title = "Confirmación") {
        return Swal.fire({
            icon: "question",
            title: title,
            text: message,
            showCancelButton: true,
            confirmButtonText: "Sí",
            cancelButtonText: "Cancelar",
            confirmButtonColor: "#198754",
            cancelButtonColor: "#dc3545"
        });
    }
};