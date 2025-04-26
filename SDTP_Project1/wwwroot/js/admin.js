// wwwroot/js/admin.js

function getAntiForgeryToken() {
    return $('input[name="__RequestVerificationToken"]').val();
}

$(function () {
    // 1) Unbind any previous handlers to avoid duplicates
    $(document)
        .off('click', '.edit-btn')
        .off('click', '.sensor-toggle')
        .off('click', '.sensor-delete')
        .off('click', '#openThresholdSettings')
        .off('submit', '#editSensorForm')
        .off('submit', '#alertThresholdSettingsForm');

    // 2) Edit Sensor: open modal
    $(document).on("click", ".edit-btn", function (e) {
        e.preventDefault();
        const sensorId = $(this).data("id");
        $.get('/Admin/EditSensor', { id: sensorId }, function (data) {
            $("#editSensorContent").html(data);
            $("#editSensorModal").modal('show');
        }).fail(xhr => {
            console.error("Error loading sensor form:", xhr.responseText);
        });
    });

    // 3) Toggle Sensor Status
    $(document).on("click", ".sensor-toggle", function (e) {
        e.preventDefault();
        const btn = $(this);
        const id = btn.attr("data-id");
        const current = String(btn.attr("data-isactive")).toLowerCase() === "true";
        const newState = !current;

        $.ajax({
            url: "/Admin/ToggleSensorStatus",
            type: "POST",
            headers: { "RequestVerificationToken": getAntiForgeryToken() },
            data: { id, isActive: newState },
            success(resp) {
                if (!resp.success) {
                    return alert(resp.message || "Error toggling sensor.");
                }
                btn.attr("data-isactive", String(resp.newIsActive))
                    .text(resp.newIsActive ? "Disable" : "Enable");

                const badge = $("#status-" + id);
                badge.removeClass("bg-success bg-danger")
                    .addClass(resp.newIsActive ? "bg-success" : "bg-danger")
                    .text(resp.newIsActive ? "Active" : "Inactive");

                // Recount deactivated
                $("#deactivatedCount").text($(".badge.bg-danger").length);
            },
            error(xhr) {
                console.error("Toggle error:", xhr);
                alert("Failed to toggle sensor.");
            }
        });
    });

    // 4) Delete Sensor with a single confirm
    $(document).on("click", ".sensor-delete", function (e) {
        e.preventDefault();
        e.stopImmediatePropagation();              // prevent any other click handlers
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
            error() {
                alert("Failed to delete sensor.");
            }
        });
    });

    // 5) Open Threshold Settings modal
    $(document).on("click", "#openThresholdSettings", function (e) {
        e.preventDefault();
        $.get('/Admin/AlertThresholdSettings', function (data) {
            $("#alertThresholdSettingsContent").html(data);
            $("#alertThresholdSettingsModal").modal('show');
        }).fail(xhr => {
            console.error("Error loading thresholds:", xhr.responseText);
        });
    });

    // 6) Submit Edit Sensor Form
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
            error() {
                alert("Failed to update sensor.");
            }
        });
    });

    // 7) Submit Threshold Settings Form
    $(document).on("submit", "#alertThresholdSettingsForm", function (e) {
        e.preventDefault();
        const payload = $(this).find(".threshold-setting").map(function () {
            return {
                Parameter: $(this).data("parameter"),
                ThresholdValue: parseFloat($(this).find(".threshold-value").val()),
                IsActive: $(this).find(".threshold-active").prop("checked")
            };
        }).get();

        $.ajax({
            url: $(this).attr("action"),
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
            error() {
                alert("Failed to save thresholds.");
            }
        });
    });

    // 8) Real-time alerts via SignalR
    // ──────────────────────────────────
    // Ensure signalr.js is loaded first!
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/airQualityHub")
        .build();

    connection.on("ReceiveAlerts", function (alerts) {
        console.log("👂 Received alerts payload:", alerts);
        alerts.forEach(function (a) {
            const $card = $('<div>').addClass('card mb-2 shadow-sm border-0').append(
                $('<div>').addClass('card-body p-3').append(
                    $('<h6>')
                        .addClass('card-title mb-2 text-truncate')
                        .html(`<i class="bi bi-exclamation-triangle-fill text-warning me-1"></i> Sensor ${a.sensorId} breached ${a.parameter}`),
                    $('<p>')
                        .addClass('card-text mb-2 small')
                        .html(`Value: <span class="fw-bold text-primary">${a.currentValue}</span> <i class="bi bi-arrow-right me-1 ms-1"></i> Threshold: <span class="fw-bold text-danger">${a.thresholdValue}</span>`),
                    $('<div>').addClass('d-flex justify-content-between align-items-center').append(
                        $('<small>')
                            .addClass('text-muted')
                            .html(`<i class="bi bi-clock me-1"></i> ${new Date(a.timestamp).toLocaleTimeString()}`),
                        $('<span>')
                            .addClass('badge rounded-pill')
                            .addClass(a.currentValue >= a.thresholdValue ? 'bg-danger' : 'bg-success')
                            .text(a.currentValue >= a.thresholdValue ? 'Breached' : 'Normal')
                    )
                )
            );
            $('#alertsContainer').prepend($card);
        });
    });

    connection.start()
        .catch(function (err) {
        console.error("SignalR connection error:", err.toString());
    });
});
