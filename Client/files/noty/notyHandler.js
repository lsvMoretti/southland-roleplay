import * as alt from 'alt-client';
var notyPage = undefined;
alt.onServer('connectionComplete', () => {
    loadNotyPage();
});
function loadNotyPage() {
    if (notyPage !== undefined) {
        alt.setTimeout(() => {
            notyPage.destroy();
            notyPage = undefined;
        }, 1000);
    }
    notyPage = new alt.WebView("http://resource/files/noty/noty.html", false);
}
alt.onServer('SendNotification', SendNotification);
export function SendNotification(message, time, type, layout) {
    if (notyPage === undefined) {
        loadNotyPage();
    }
    notyPage.emit('displayNotification', message, time, type, layout);
}
