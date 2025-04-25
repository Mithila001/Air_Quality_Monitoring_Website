function getAntiForgeryToken() {
    return $('input[name="__RequestVerificationToken"]').val();
}

$(function () {
    // Toggle On/Off
    $(document).on("click", ".sensor-toggle", function () {
        const btn = $(this);
        const id = btn.attr("data-id");
        // ALWAYS read the raw attribute and compare to lowercase "true"
        const currentState = String(btn.attr("data-isactive")).toLowerCase() === "true";
        const newState = !currentState;

        console.log(`[Toggle] Sensor ${id}: ${currentState} → ${newState}`);

        $.ajax({
            url: "/Admin/ToggleSensorStatus",
            type: "POST",
            headers: { "RequestVerificationToken": getAntiForgeryToken() },
            data: { id: id, isActive: newState },
            success(resp) {
                console.log("[Toggle] Response:", resp);
                if (!resp.success) {
                    alert(resp.message || "Error toggling sensor.");
                    return;
                }

                // 1) Update the button attribute + text
                btn
                    .attr("data-isactive", String(resp.newIsActive))
                    .text(resp.newIsActive ? "Disable" : "Enable");

                // 2) Update the badge classes & text
                const badge = $("#status-" + id);
                badge
                    .removeClass("bg-success bg-danger")
                    .addClass(resp.newIsActive ? "bg-success" : "bg-danger")
                    .text(resp.newIsActive ? "Active" : "Inactive");

                // 3) Recount deactivated sensors from DOM
                const totalDeactivated = $(".badge.bg-danger").length;
                $("#deactivatedCount").text(totalDeactivated);
            },
            error(xhr) {
                console.error("[Toggle] AJAX error:", xhr);
                alert("Failed to toggle sensor. See console for details.");
            }
        });
    });

    // Delete
    $(document).on("click", ".sensor-delete", function () {
        const id = $(this).data("id");
        if (!confirm("⚠️ Permanently delete this sensor?")) return;
        $.ajax({
            url: "/Admin/DeleteSensor",
            type: "POST",
            data: { id },
            headers: { "RequestVerificationToken": getAntiForgeryToken() },
            success(resp) {
                if (!resp.success) return alert(resp.message);
                location.reload();
            },
            error() { alert("Failed to delete sensor."); }
        });
    });

    // Edit Sensor Form
    $(document).on("submit", "#editSensorForm", function (e) {
        e.preventDefault();
        const form = $(this);
        $.ajax({
            url: form.attr("action"),
            type: "POST",
            data: form.serialize(),
            headers: { "RequestVerificationToken": getAntiForgeryToken() },
            success(resp) {
                if (resp.success) {
                    $("#editSensorModal").modal("hide");
                    location.reload();
                } else {
                    $("#editSensorContent").html(resp);
                }
            },
            error() { alert("Failed to update sensor."); }
        });
    });

    // Alert Thresholds Form
    $(document).on("submit", "#alertThresholdSettingsForm", function (e) {
        e.preventDefault();
        const form = $(this), payload = [];
        form.find(".threshold-setting").each(function () {
            const block = $(this),
                parameter = block.data("parameter"),
                value = parseFloat(block.find(".threshold-value").val()),
                isActive = block.find(".threshold-active").prop("checked");
            payload.push({ Parameter: parameter, ThresholdValue: value, IsActive: isActive });
        });

        $.ajax({
            url: form.attr("action"),
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(payload),
            headers: { "RequestVerificationToken": getAntiForgeryToken() },
            success(resp) {
                if (resp.success) {
                    $("#alertThresholdSettingsModal").modal("hide");
                    location.reload();
                } else {
                    alert(resp.message || "Error saving settings.");
                }
            },
            error() { alert("Failed to save thresholds."); }
        });
    });
});
