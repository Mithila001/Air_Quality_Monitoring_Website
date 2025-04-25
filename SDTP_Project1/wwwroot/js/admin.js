function getAntiForgeryToken() {
    return $('input[name="__RequestVerificationToken"]').val();
}

$(function () {
    // Toggle On/Off
    $(document).on("click", ".sensor-toggle", function () {
        const btn = $(this),
            id = btn.data("id"),
            newStatus = !btn.data("isactive");

        $.ajax({
            url: "/Admin/ToggleSensorStatus",
            type: "POST",
            data: { id, isActive: newStatus },
            headers: { "RequestVerificationToken": getAntiForgeryToken() },
            success(resp) {
                if (!resp.success) return alert(resp.message || "Error");
                btn
                    .data("isactive", newStatus)
                    .text(newStatus ? "Disable" : "Enable");
                const badge = $("#status-" + id);
                badge
                    .toggleClass("bg-success", newStatus)
                    .toggleClass("bg-danger", !newStatus)
                    .text(newStatus ? "Active" : "Inactive");
                let cnt = parseInt($("#deactivatedCount").text());
                $("#deactivatedCount").text(newStatus ? Math.max(0, cnt - 1) : cnt + 1);
            },
            error() { alert("Failed to toggle sensor."); }
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
