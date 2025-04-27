// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function showNotification(message, type = 'danger', duration = 5000) {
    const notificationArea = $('#notification-area');
    const notification = $(`
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `);

    notificationArea.append(notification);

    // Automatically remove the notification after a delay
    setTimeout(() => {
        notification.alert('close');
    }, duration);
}
