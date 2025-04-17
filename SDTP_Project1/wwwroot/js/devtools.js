$(function () {
    // show modal
    $('#devToolsBtn').click(() => $('#devToolsModal').modal('show'));

    // load status
    function refreshStatus() {
        // could call an endpoint; here we assume server returns status on toggle
    }

    $('#toggleDevMode').click(() => {
        $.post('/dev/toggle', null, data => {
            $('#devModeStatus').text(data.devMode ? 'ON' : 'OFF');
            $('#devToolsMessage').text(`DevMode is now ${data.devMode}`);
        });
    });

    $('#clearTodayData').click(() => {
        if (!confirm('Really delete all today’s data?')) return;
        $.post('/dev/clear-today', null, data => {
            $('#devToolsMessage').text(`Deleted ${data.deleted} records.`);
        });
    });
});
