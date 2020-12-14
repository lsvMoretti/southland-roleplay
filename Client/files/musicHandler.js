import * as alt from 'alt-client';
var musicView = undefined;
var currentUrl = undefined;
var audioControlView = undefined;
var stationJson = undefined;
alt.onServer('showAudioControl', showAudioControl);
alt.onServer('PlayMusicUrl', playMusicFromUrl);
alt.onServer('StopMusic', stopMusic);
function showAudioControl(json) {
    CloseAudioControlPanel();
    stationJson = json;
    alt.showCursor(true);
    audioControlView = new alt.WebView("http://resource/files/html/radioControl.html", false);
    audioControlView.focus();
    audioControlView.on('radioControlPageLoaded', radioControlPageLoaded);
    audioControlView.on('SelectedMusicStream', SelectedMusicStream);
    audioControlView.on('StopMusicStream', SelectStopMusicStream);
}
function radioControlPageLoaded() {
    if (audioControlView === undefined)
        return;
    audioControlView.emit('loadStationTable', stationJson);
}
function SelectedMusicStream(stationName) {
    CloseAudioControlPanel();
    alt.emitServer('PlayerSelectedMusicStream', stationName);
}
function SelectStopMusicStream() {
    CloseAudioControlPanel();
    alt.emitServer('StopSelectedMusicStream');
}
function CloseAudioControlPanel() {
    if (audioControlView !== undefined) {
        audioControlView.destroy();
        audioControlView = undefined;
        alt.showCursor(false);
    }
}
function playMusicFromUrl(url) {
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
    if (musicView === undefined)
        return;
    if (currentUrl === undefined)
        return;
    musicView.emit('PlayUrl', currentUrl);
}
function stopMusic() {
    musicView.emit('StopPlayingMusic');
    musicView.destroy();
    musicView = undefined;
}
