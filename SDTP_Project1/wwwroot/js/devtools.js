$(function () {
    const fetchStatus = () => {
        $.get('/dev/status', data => {
            $('#devModeStatus').text(data.devMode ? 'ON' : 'OFF');
        });
    };

    // Trigger modal on button click
    $('#devToolsBtn').click(function () {
        $('#devToolsModal').modal('show');  // This will show the modal when the button is clicked
    });

    $('#devToolsModal').on('show.bs.modal', fetchStatus);

    $('#toggleDevMode').click(() => {
        $.post('/dev/toggle', null, data => {
            $('#devModeStatus').text(data.devMode ? 'ON' : 'OFF');
            $('#devToolsMessage')
                .removeClass('d-none')
                .text(`DevMode is now ${data.devMode ? 'ENABLED' : 'DISABLED'}`)
                .addClass('alert-info')
                .removeClass('alert-danger');
        });
    });

    $('#clearTodayData').click(() => {
        if (!confirm('Really delete all today’s data?')) return;
        $.post('/dev/clear-today', null, data => {
            $('#devToolsMessage')
                .removeClass('d-none')
                .text(`Deleted ${data.deleted} records.`)
                .addClass('alert-danger')
                .removeClass('alert-info');
        });
    });

    // Fetch status when modal is shown
    $('#devToolsModal').on('show.bs.modal', function () {
        $.get('/dev/status', function (data) {
            $('#devModeStatus').text(data.devMode ? 'ON' : 'OFF');
        });
    });
});
