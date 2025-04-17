function initMap(sensors) {
    // Define approximate bounds for Sri Lanka.
    var southWest = L.latLng(5.9, 79.6);
    var northEast = L.latLng(9.8, 81.9);
    var bounds = L.latLngBounds(southWest, northEast);

    // Updated center for Colombo.
    var colomboCenter = [6.931, 79.847];

    var map = L.map('map', {
        center: colomboCenter,
        zoom: 12,
        minZoom: 12,
        maxZoom: 18,
        maxBounds: bounds,
        maxBoundsViscosity: 1.0
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18,
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    // Invalidate size and recenter after a short delay.
    setTimeout(function () {
        map.invalidateSize();
        map.setView(colomboCenter);
    }, 200);

    sensors.forEach(function (sensor) {
        var lat = sensor.latitude || sensor.Latitude;
        var lng = sensor.longitude || sensor.Longitude;
        var city = sensor.city || sensor.City;
        var aqi = sensor.aqi || sensor.AQI;
        var aqiColor = getAQIColor(aqi);

        // Use a unique id for the canvas (based on city name without spaces).
        var canvasId = "chart-" + city.replace(/\s+/g, '');
        var popupContent = `
            <div class="popup-content">
                <h5>${city}</h5>
                <p><strong>AQI:</strong> <span style="color:${aqiColor}">${aqi}</span></p>
                <p><strong>Total Readings:</strong> ${sensor.Readings.length}</p>
                <canvas id="${canvasId}" style="width:300px; height:200px;"></canvas>
            </div>
        `;

        // Create marker and bind the popup.
        var marker = L.marker([lat, lng]).addTo(map)
            .bindPopup(popupContent, { closeButton: false });

        // Store timer ID on marker.
        marker.closeTimeout = null;

        // Use mouseenter and mouseleave on marker.
        marker.on('mouseover', function () {
            clearTimeout(marker.closeTimeout);
            this.openPopup();
        });

        marker.on('mouseout', function () {
            marker.closeTimeout = setTimeout(() => {
                this.closePopup();
            }, 250);
        });

        // When popup opens, add listeners to the popup element.
        marker.on('popupopen', function (e) {
            var popupEl = e.popup.getElement();

            // Cancel timer if mouse enters the popup.
            popupEl.addEventListener('mouseenter', function () {
                clearTimeout(marker.closeTimeout);
            });

            // When mouse leaves the popup, start timer to close.
            popupEl.addEventListener('mouseleave', function () {
                marker.closeTimeout = setTimeout(() => {
                    marker.closePopup();
                }, 250);
            });

            // Render the chart inside the popup.
            renderChart(canvasId, sensor.Readings);
        });
    });
}

// Function to determine color code based on AQI.
function getAQIColor(aqi) {
    if (aqi == null) return "gray";
    if (aqi <= 50) return "green";
    if (aqi <= 100) return "yellow";
    if (aqi <= 150) return "orange";
    if (aqi <= 200) return "red";
    if (aqi <= 300) return "purple";
    return "maroon";
}

// Function to render a chart using Chart.js.
// This example plots AQI over time.
function renderChart(canvasId, readings) {
    if (!readings || readings.length === 0) return;

    var labels = readings.map(function (r) {
        return r.Timestamp; // Already formatted on server.
    });
    var aqiValues = readings.map(function (r) {
        return r.AQI;
    });

    var ctx = document.getElementById(canvasId);
    if (!ctx) return; // Safety check.

    // Destroy any previous chart instance if it exists.
    if (ctx.chartInstance) {
        ctx.chartInstance.destroy();
    }

    ctx.chartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'AQI over time',
                data: aqiValues,
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 2,
                fill: false,
                tension: 0.1
            }]
        },
        options: {
            responsive: false,
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'AQI'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Time'
                    }
                }
            }
        }
    });
}
