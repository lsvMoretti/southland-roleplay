import * as alt from 'alt-client';

var musicView: alt.WebView = undefined;
var currentUrl: string = undefined;
var audioControlView: alt.WebView = undefined;
var stationJson: string = undefined;

alt.onServer('showAudioControl', showAudioControl);
alt.onServer('PlayMusicUrl', playMusicFromUrl);
alt.onServer('StopMusic', stopMusic);

function showAudioControl(json: string) {
    CloseAudioControlPanel();

    stationJson = json;
    alt.showCursor(true);
    audioControlView = new alt.WebView("http://resource/files/html/radioControl.html", false);

    audioControlView.focus();

    audioControlView.on('radioControlPageLoaded', radioControlPageLoaded);

    audioControlView.on('SelectedMusicStream', SelectedMusicStream);

    audioControlView.on('StopMusicStream', SelectStopMusicStream)
}

function radioControlPageLoaded() {
    if (audioControlView === undefined) return;

    audioControlView.emit('loadStationTable', stationJson);
}

function SelectedMusicStream(stationName: string) {
    CloseAudioControlPanel();
    alt.emitServer('PlayerSelectedMusicStream', stationName);
}

function SelectStopMusicStream() {
    CloseAudioControlPanel();
    alt.emitServer('StopSelectedMusicStream');
}

function CloseAudioControlPanel() {
    if (audioControlView !== undefined) {
        alt.setTimeout(() => {
            audioControlView.destroy();
            audioControlView = undefined;
        }, 1000);
        alt.showCursor(false);
    }
}

function playMusicFromUrl(url: string) {
    alt.log('Loading Webview');
    currentUrl = url;
    if (musicView !== undefined) {
        stopMusic();
    }
    musicView = new alt.WebView("http://resource/files/html/radio.html", false);
    musicView.focus();
    musicView.on('radioPageLoaded', radioPageLoaded);
}

function radioPageLoaded() {
    if (musicView === undefined) return;

    if (currentUrl === undefined) return;

    musicView.emit('PlayUrl', currentUrl);
}

function stopMusic() {
    musicView.emit('StopPlayingMusic');
    alt.setTimeout(() => {
        musicView.destroy();
        musicView = undefined;
    },
        1000);
}