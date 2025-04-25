// wwwroot/js/form-validation.js
$(function () {
    // Automatically hook into ASP.NET Core's unobtrusive validation
    $("form").each(function () {
        $(this).validate(); // Force init in case of dynamic forms
    });

    // Custom handling: Add Bootstrap classes on validation fail
    $("form").on("submit", function (e) {
        let form = $(this);
        if (!form.valid()) {
            form.find("input, select").each(function () {
                const input = $(this);
                if (input.hasClass("input-validation-error")) {
                    input.addClass("is-invalid");
                } else {
                    input.removeClass("is-invalid").addClass("is-valid");
                }
            });
            e.preventDefault(); // Don't submit on client error
        }
    });

    // Reset validation classes on input change
    $("form input, form select").on("change keyup", function () {
        const input = $(this);
        if (input.hasClass("input-validation-error")) {
            input.addClass("is-invalid").removeClass("is-valid");
        } else {
            input.removeClass("is-invalid").addClass("is-valid");
        }
    });
});
