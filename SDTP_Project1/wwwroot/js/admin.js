function disableSensor(sensorId) {
    $.ajax({
        url: '/Admin/ToggleSensorStatus',
        type: 'POST',
        data: { id: sensorId },
        success: function (response) {
            if (response.success) {
                // Find the button and update the text based on response
                let btn = $(`button[onclick="disableSensor('${sensorId}')"]`);
                btn.text(response.isActive ? "Disable" : "Enable");

                // Update the row UI to reflect the new status
                let row = btn.closest(".sensor-row");
                row.toggleClass("disabled", !response.isActive);
            } else {
                alert("Error updating sensor status.");
            }
        },
        error: function () {
            alert("Error toggling sensor status.");
        }
    });
}


function confirmDelete(sensorId) {
    if (!confirm("⚠️ Are you sure you want to delete this sensor permanently?")) return;

    $.ajax({
        url: '/Admin/DeleteSensor',
        type: 'POST',
        data: { id: sensorId },
        success: function (response) {
            alert(response.message);
            location.reload();
        },
        error: function () {
            alert("Error deleting sensor.");
        }
    });
}


// Use delegated event handling for buttons with the sensor-toggle class
$(document).on("click", ".sensor-toggle", function () {
    var btn = $(this);
    var sensorId = btn.data("id");
    // Convert data-isactive to a boolean if needed
    var currentStatus = (btn.data("isactive") === true || btn.data("isactive") === "True");
    // Toggle the status: if true, set to false; if false, set to true.
    var newStatus = !currentStatus;

    // Send the AJAX request to toggle sensor status
    $.ajax({
        url: '/Admin/ToggleSensorStatus',
        type: 'POST',
        data: { id: sensorId, isActive: newStatus },
        success: function (response) {
            if (response.success) {
                // Update the button's data attribute and text
                btn.data("isactive", newStatus);
                btn.text(newStatus ? "Disable" : "Enable");
                // Update the status badge
                var statusBadge = $("#status-" + sensorId);
                if (newStatus) {
                    // Sensor is active, so status badge becomes "Active" with bg-success
                    statusBadge
                        .removeClass("bg-danger")
                        .addClass("bg-success")
                        .text("Active");
                } else {
                    // Sensor is inactive
                    statusBadge
                        .removeClass("bg-success")
                        .addClass("bg-danger")
                        .text("Inactive");
                }

                var countEl = $("#deactivatedCount");
                var currentCount = parseInt(countEl.text());
                if (newStatus) {
                    // When enabling, decrease the count
                    countEl.text(Math.max(0, currentCount - 1));
                } else {
                    // When disabling, increase the count
                    countEl.text(currentCount + 1);
                }

            } else {
                alert("Error updating sensor status: " + response.message);
            }
        },
        error: function () {
            alert("Error toggling sensor status.");
        }
    });
});
