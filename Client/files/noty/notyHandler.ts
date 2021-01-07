import * as alt from 'alt-client';
import * as native from 'natives';

var notyPage: alt.WebView = undefined;

alt.onServer('connectionComplete', () => {
    loadNotyPage();
});

function loadNotyPage() {
    if (notyPage !== undefined) {
        notyPage.destroy();
        notyPage = undefined;
    }

    notyPage = new alt.WebView("http://resource/files/noty/noty.html", false);
}

alt.onServer('SendNotification', SendNotification);

export function SendNotification(message: string, time: number, type: string, layout: string) {
    if (notyPage === undefined) {
        loadNotyPage();
    }
    notyPage.emit('displayNotification', message, time, type, layout);
}