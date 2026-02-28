document.addEventListener("DOMContentLoaded", function () {
    
    /* ===============================
       TOGGLE PASSWORD
    =============================== */
    const toggleBtn = document.querySelector(".toggle-password");
    const passwordField = document.getElementById("password");
    const icon = document.getElementById("toggleIcon");

    if (toggleBtn && passwordField && icon) {
        toggleBtn.addEventListener("click", function () {

            if (passwordField.type === "password") {
                passwordField.type = "text";
                icon.classList.remove("bi-eye");
                icon.classList.add("bi-eye-slash");
            } else {
                passwordField.type = "password";
                icon.classList.remove("bi-eye-slash");
                icon.classList.add("bi-eye");
            }

        });
    }


    /* ===============================
       BOTÓN LOADING LOGIN
    =============================== */
    const form = document.querySelector("form");
    const btn = document.getElementById("btnLogin");
    const text = document.getElementById("btnText");

    if (form && btn && text) {
        form.addEventListener("submit", function () {
            btn.disabled = true;
            text.innerHTML = "Ingresando...";
        });
    }

});