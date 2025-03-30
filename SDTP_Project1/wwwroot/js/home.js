function initMap(locations) {
    // Define approximate bounds for Sri Lanka.
    var southWest = L.latLng(5.9, 79.6);
    var northEast = L.latLng(9.8, 81.9);
    var bounds = L.latLngBounds(southWest, northEast);

    // Updated center for Colombo so that the district is visible
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

    // Invalidate size and recenter after a short delay
    setTimeout(function () {
        map.invalidateSize();
        map.setView(colomboCenter);
    }, 200);

    // Add markers with updated popups showing current data
    locations.forEach(function (loc) {
        // Use camelCase if available, otherwise fall back to PascalCase.
        var lat = loc.latitude || loc.Latitude;
        var lng = loc.longitude || loc.Longitude;
        var district = loc.district || loc.District;
        var aqi = loc.aqi || loc.AQI;
        var timestamp = loc.timestamp || loc.Timestamp;
        var pm25 = loc.pm2_5 || loc.PM2_5;
        var pm10 = loc.pm10 || loc.PM10;

        var popupContent = "<b>" + district + "</b><br>" +
            "AQI: " + aqi + "<br>" +
            "Time: " + new Date(timestamp).toLocaleString() + "<br>" +
            "PM2.5: " + pm25 + "<br>" +
            "PM10: " + pm10;

        L.marker([lat, lng]).addTo(map)
            .bindPopup(popupContent);
    });
}
