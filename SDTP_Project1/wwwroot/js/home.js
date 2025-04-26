function initMap(sensors) {
    // 1) Setup map
    const southWest = L.latLng(5.9, 79.6);
    const northEast = L.latLng(9.8, 81.9); // Fixed longitude value
    const bounds = L.latLngBounds(southWest, northEast);
    const center = [6.931, 79.847];
    const map = L.map('map', {
        center, zoom: 12, minZoom: 12, maxZoom: 18,
        maxBounds: bounds, maxBoundsViscosity: 1.0
    });
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18, attribution: '© OpenStreetMap contributors'
    }).addTo(map);
    setTimeout(() => { map.invalidateSize(); map.setView(center); }, 200);

    // 2) Create markers with attached sensor data
    sensors.forEach(sensor => {
        // Safety check for required data
        if (!sensor) return;

        const lat = sensor.Latitude;
        const lng = sensor.Longitude;
        const city = sensor.City || 'Unknown';

        // Safely handle AQI value
        let aqi = 'N/A';
        let color = 'gray';

        if (sensor.AQI !== undefined && sensor.AQI !== null) {
            aqi = sensor.AQI;
            // Use a try-catch block to handle any issues with getAQIColor
             try {
                 color = safeGetAQIColor(aqi);

             } catch (e) {
                    console.error('Error getting AQI color:', e);
                    color = 'gray';
             }
           
        }

        const awesomeIcon = L.AwesomeMarkers.icon({
            icon: 'info-sign', // or any other FA icon like 'leaf', 'cloud', etc.
            prefix: 'glyphicon', // change to 'fa' if using Font Awesome
            markerColor: mapAQIColorToMarkerColor(color),
            iconColor: 'white'
        });


        const readings = sensor.Readings || [];
        const canvasId = `chart-${city.replace(/\s+/g, '')}-${Math.floor(Math.random() * 1000)}`;

        const popupContent = `
<div class="popup-content p-3">
    <h5 class="mb-2 fw-bold text-primary">${city}</h5>
    <p class="mb-1"><strong class="me-1">AQI:</strong> <span class="badge rounded-pill" style="background-color:${color}; color: white;">${aqi}</span></p>
    <p class="mb-0"><strong class="me-1">Total Readings:</strong> <span class="badge bg-secondary rounded-pill">${readings.length}</span></p>
    <canvas id="${canvasId}" width="300" height="200" class="mt-2 border"></canvas>
</div>`;

        const marker = L.marker([lat, lng], {
            icon: awesomeIcon,
            sensor: {
                readings: readings,
                canvasId: canvasId
            }
        })
            .addTo(map)
            .bindPopup(popupContent, { closeButton: false });

        // store one timeout handle per marker
        marker._closeTimeout = null;

        // open on hover, close on leave
        marker.on('mouseover', () => {
            clearTimeout(marker._closeTimeout);
            marker.openPopup();
        });

        marker.on('mouseout', () => {
            marker._closeTimeout = setTimeout(() => marker.closePopup(), 250);
        });
    });

    // 3) Centralized popup event
    map.on('popupopen', e => {
        const marker = e.popup._source;
        clearTimeout(marker._closeTimeout);
        const popupEl = e.popup.getElement();

        // When user hovers over popup, cancel close; when leaves, schedule it.
        popupEl.addEventListener('mouseenter', () => clearTimeout(marker._closeTimeout));
        popupEl.addEventListener('mouseleave', () => {
            marker._closeTimeout = setTimeout(() => marker.closePopup(), 250);
        });

        // Render chart now that content is in the DOM
        if (marker.options && marker.options.sensor) {
            const canvas = popupEl.querySelector('canvas');
            if (canvas) {
                setTimeout(() => {
                    try {
                        renderChart(canvas.id, marker.options.sensor.readings);
                    } catch (e) {
                        console.error('Error rendering chart:', e);
                    }
                }, 50);
            }
        }
    });
}

function mapAQIColorToMarkerColor(hexColor) {
    switch (hexColor.toLowerCase()) {
        case 'green': return 'green';
        case 'yellow': return 'orange';  // AwesomeMarkers has 'orange', not yellow
        case 'orange': return 'orange';
        case 'red': return 'red';
        case 'purple': return 'purple';
        case 'maroon': return 'darkred';
        default: return 'gray';
    }
}


// Safer version of getAQIColor
function safeGetAQIColor(aqi) {
    // Make sure aqi is a number
    const aqiNum = Number(aqi);

    // If aqi is not a valid number, return gray
    if (isNaN(aqiNum)) return "gray";

    // Regular color logic
    if (aqiNum <= 50) return "green";
    if (aqiNum <= 100) return "yellow";
    if (aqiNum <= 150) return "orange";
    if (aqiNum <= 200) return "red";
    if (aqiNum <= 300) return "purple";
    return "maroon";
}

// Modify the renderChart function to return the chart instance
function renderChart(canvasId, readings) {
    if (!readings || readings.length === 0) return null;

    const ctx = document.getElementById(canvasId);
    if (!ctx) return null;

    if (ctx.chartInstance) {
        ctx.chartInstance.destroy();
    }

    const labels = readings.map(r => r.Timestamp);
    const aqiValues = readings.map(r => r.AQI);

    const chartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'AQI Trend',
                data: aqiValues,
                borderColor: 'rgba(54, 162, 235, 1)',
                backgroundColor: 'rgba(54, 162, 235, 0.1)',
                borderWidth: 2,
                fill: true,
                tension: 0.2,
                pointRadius: 3
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    title: { display: true, text: 'Air Quality Index' }
                },
                x: {
                    title: { display: true, text: 'Date' },
                    ticks: {
                        callback: function (value) {
                            const date = new Date(this.getLabelForValue(value));
                            return `${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getDate().toString().padStart(2, '0')}`;
                        }
                    }
                }
            }
        }
    });

    ctx.chartInstance = chartInstance;
    return chartInstance;
}
