// File: E:\Programming\Projects\ASPdotNET\SDTP_Project1\SDTP_Project1\wwwroot\js\home.js

// Utility must come first
function getAQIStyle(aqi) {
    const n = Number(aqi);
    let hex;
    if (isNaN(n)) hex = '#999999';
    else if (n <= 50) hex = '#00E400';
    else if (n <= 100) hex = '#FFFF00';
    else if (n <= 150) hex = '#FF7E00';
    else if (n <= 200) hex = '#FF0000';
    else if (n <= 300) hex = '#8F3F97';
    else hex = '#7E0023';

    const badgeClass = (() => {
        switch (hex.toLowerCase()) {
            case '#00e400': return 'bg-success';
            case '#ffff00': return 'bg-warning text-dark';
            case '#ff7e00': return 'bg-warning';
            case '#ff0000': return 'bg-danger';
            case '#8f3f97': return 'bg-purple';
            case '#7e0023': return 'bg-maroon';
            default: return 'bg-secondary';
        }
    })();

    const rgb = parseInt(hex.slice(1), 16);
    const luminance = 0.299 * (rgb >> 16)
        + 0.587 * ((rgb >> 8) & 0xff)
        + 0.114 * (rgb & 0xff);
    const textColor = luminance > 186 ? '#000000' : '#ffffff';

    return { hex, badgeClass, textColor };
}

function initMap(sensors) {
    const southWest = L.latLng(5.9, 79.6);
    const northEast = L.latLng(9.8, 81.9);
    const map = L.map('map', {
        center: [6.931, 79.847],
        zoom: 12,
        minZoom: 12,
        maxZoom: 18,
        maxBounds: L.latLngBounds(southWest, northEast),
        maxBoundsViscosity: 1.0
    });
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 18,
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);
    setTimeout(() => {
        map.invalidateSize();
        map.setView([6.931, 79.847]);
    }, 200);

    const customPopupOptions = {
        className: 'custom-popup',
        closeButton: false,
        maxWidth: 340,
        minWidth: 340,
        autoPan: true,
        offset: [0, -10]
    };

    sensors.forEach(sensor => {
        if (!sensor) return;

        const { hex, badgeClass, textColor } = getAQIStyle(sensor.AQI);
        const lat = sensor.Latitude;
        const lng = sensor.Longitude;
        const city = sensor.City || 'Unknown';
        const readings = sensor.Readings || [];
        const canvasId = `chart-${city.replace(/\s+/g, '')}-${Math.floor(Math.random() * 1000)}`;

        // Calculate 7 day average AQI
        const last7DaysReadings = readings.slice(-7);
        const avgAQI = last7DaysReadings.length > 0
            ? Math.round(last7DaysReadings.reduce((sum, r) => sum + Number(r.AQI), 0) / last7DaysReadings.length)
            : 'N/A';

        const avgStyle = getAQIStyle(avgAQI);
        // Get latest reading date and time
        let lastUpdateDate = 'N/A';
        let lastUpdateTime = 'N/A';

        if (readings.length > 0) {
            const latestReading = readings[readings.length - 1];
            const timestamp = new Date(latestReading.Timestamp);
            lastUpdateDate = timestamp.toLocaleDateString('en-US', { day: '2-digit', month: 'short' });
            lastUpdateTime = timestamp.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: true });
        }

        // Get latest air quality parameters
        const latestParams = readings.length > 0 ? readings[readings.length - 1] : {};

        // Modified popup content with better spacing and dimensions
        const popupContent = `
<div class="popup-content p-3 bg-custom-gray-light border-0 rounded shadow">
  <div class="d-flex justify-content-between align-items-center mb-3">
    <h5 class="m-0 text-custom-blue fw-bold">${city}</h5>
    <span class="text-muted" style="font-size: 0.75rem;">
      ${lastUpdateDate} ${lastUpdateTime}
    </span>
  </div>
  
  <div class="d-flex justify-content-between align-items-center mb-3">
    <div>
      <div class="d-flex align-items-center">
        <span class="text-muted" style="font-size: 0.8rem;">Current AQI</span>
      </div>
      <div class="d-flex align-items-baseline">
        
        <span
            class="badge rounded-lg fw-bold fs-5 ${badgeClass}"
            style="background-color: ${hex}; color: ${textColor};"
            >
            ${sensor.AQI ?? 'N/A'}
            </span>
      </div>
    </div>
    <div>
      <div class="d-flex align-items-center">
        <span class="text-muted" style="font-size: 0.8rem;">7-day Avg</span>
      </div>
      <div class="d-flex align-items-baseline">
        <span class="badge rounded-lg fw-bold fs-5 ${avgStyle.badgeClass}"
        style="background-color: ${avgStyle.hex}; color: ${avgStyle.textColor};"
        >
        ${avgAQI}
        </span>
      </div>
    </div>
  </div>
  
  <div class="row g-2 mb-4">
    <div class="col-3">
      <div class="bg-white p-2 rounded shadow-sm text-center">
        <div class="text-muted" style="font-size: 0.7rem;">PM2.5</div>
        <div class="fw-bold">${latestParams.PM2_5 ?? 'N/A'}</div>
      </div>
    </div>
    <div class="col-3">
      <div class="bg-white p-2 rounded shadow-sm text-center">
        <div class="text-muted" style="font-size: 0.7rem;">PM10</div>
        <div class="fw-bold">${latestParams.PM10 ?? 'N/A'}</div>
      </div>
    </div>
    <div class="col-3">
      <div class="bg-white p-2 rounded shadow-sm text-center">
        <div class="text-muted" style="font-size: 0.7rem;">O₃</div>
        <div class="fw-bold">${latestParams.O3 ?? 'N/A'}</div>
      </div>
    </div>
    <div class="col-3">
      <div class="bg-white p-2 rounded shadow-sm text-center">
        <div class="text-muted" style="font-size: 0.7rem;">NO₂</div>
        <div class="fw-bold">${latestParams.NO2 ?? 'N/A'}</div>
      </div>
    </div>
  </div>
  
  <div class="chart-container" style="height: 180px;">
    <canvas id="${canvasId}" width="320" height="180" class="w-100"></canvas>
  </div>
</div>
`;


        const svg = `
      <svg width="40" height="52" viewBox="0 0 40 52"
           xmlns="http://www.w3.org/2000/svg" class="aqi-pin-svg">
        <path d="M20 0 C30 0,40 10,40 20
                 C40 35,20 52,20 52
                 C20 52,0 35,0 20
                 C0 10,10 0,20 0 Z"
              fill="${hex}" stroke="#333" stroke-width="1"/>
        <text x="20" y="24" text-anchor="middle"
              fill="${textColor}" font-size="12" font-weight="bold">
          ${sensor.AQI ?? 'N/A'}
        </text>
      </svg>`;

        const aqiIcon = L.divIcon({
            html: svg,
            iconSize: [40, 52],
            iconAnchor: [20, 52],
            popupAnchor: [0, -52]
        });

        const marker = L.marker([lat, lng], { icon: aqiIcon, sensor: { readings, canvasId } })
            .addTo(map)
            .bindPopup(popupContent, customPopupOptions);

        marker._closeTimeout = null;
        marker.on('mouseover', () => { clearTimeout(marker._closeTimeout); marker.openPopup(); });
        marker.on('mouseout', () => { marker._closeTimeout = setTimeout(() => marker.closePopup(), 250); });
    });

    map.on('popupopen', e => {
        const marker = e.popup._source;
        clearTimeout(marker._closeTimeout);
        const popupEl = e.popup.getElement();
        popupEl.addEventListener('mouseenter', () => clearTimeout(marker._closeTimeout));
        popupEl.addEventListener('mouseleave', () => {
            marker._closeTimeout = setTimeout(() => marker.closePopup(), 250);
        });
        const canvas = popupEl.querySelector('canvas');
        if (canvas) setTimeout(() => renderChart(canvas.id, marker.options.sensor.readings), 50);
    });
}




// Modify the renderChart function to return the chart instance
function renderChart(canvasId, readings) {
    if (!readings || readings.length === 0) return null;

    const ctx = document.getElementById(canvasId);
    if (!ctx) return null;

    if (ctx.chartInstance) {
        ctx.chartInstance.destroy();
    }

    // Get last 14 days of readings to show a reasonable trend
    const recentReadings = readings.slice(-14);
    const labels = recentReadings.map(r => r.Timestamp);
    const aqiValues = recentReadings.map(r => r.AQI);

    const chartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'AQI',
                data: aqiValues,
                borderColor: 'rgba(54, 162, 235, 1)',
                backgroundColor: 'rgba(54, 162, 235, 0.1)',
                borderWidth: 2,
                fill: true,
                tension: 0.3,
                pointRadius: 2,
                pointHoverRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false  // Hide legend to save space
                },
                tooltip: {
                    mode: 'index',
                    intersect: true,
                    callbacks: {
                        title: function (tooltipItems) {
                            const date = new Date(tooltipItems[0].label);
                            return date.toLocaleDateString('en-US', {
                                month: 'short',
                                day: 'numeric',
                                hour: '2-digit',
                                minute: '2-digit'
                            });
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        text: 'AQI Values',
                        display: true  // Hide axis title to save space
                        
                    },
                    ticks: {
                        font: {
                            size: 10  // Smaller font for axis ticks
                        }
                    }
                },
                x: {
                    display: true,
                    title: {
                        display: true,
                        text: 'Past 30 Days'
                    },
                    ticks: {
                        maxRotation: 45,
                        minRotation: 45,
                        font: {
                            size: 8  // Smaller font for axis ticks
                        },
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


$(function () {
    $('.view-details-btn').on('click', function () {
        var sensorId = $(this).data('sensor-id');
        var sensorCity = $(this).data('sensor-city');
        var titleText = 'Readings for ' + sensorCity + '';

        $.get('/Home/SensorDetails', { sensorId: sensorId })
            .done(function (html) {
                // Update header
                $('#sensorDetailsLabel').text(titleText);
                // Inject and wrap in responsive container + striped table styling
                $('#sensorDetailsModal .modal-body').html(
                    '<div class="table-responsive">' + html + '</div>'
                );
                // Show modal
                var modalEl = document.getElementById('sensorDetailsModal');
                var bsModal = new bootstrap.Modal(modalEl);
                bsModal.show();
            })
            .fail(function () {
                alert('⚠️ Unable to load readings for ' + sensorCity + '.');
            });
    });
});

