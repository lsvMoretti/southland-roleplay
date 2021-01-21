if ('alt' in window) {
    alt.on('playMusicFromUrl', playMusicFromUrl);
    alt.on('StopPlayingMusic', stopMusic);
}

var radioObject = null;

function playMusicFromUrl(url) {
    radioObject = document.getElementById('radio');
    radioObject.setAttribute('src', url);
    radioObject.muted = false;
    radioObject.play();
}

function stopMusic() {
    radioObject.pause();
    radioObject.muted = true;
}