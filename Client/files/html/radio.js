var radioObject = undefined;

function radioLoaded()
{
    alt.emit('radioPageLoaded');
}

if('alt' in window){

    alt.on('PlayUrl', PlayUrl);
    alt.on('StopPlayingMusic', StopAudio);
}

function StopAudio()
{
    radioObject.pause = true;
    radioObject = undefined;
}


function PlayUrl(url)
{
    radioObject = new Audio(url);
    radioObject.autoplay = true;
    radioObject.volume = 0.2;
}