import * as alt from 'alt-client';
import * as native from 'natives';
alt.onServer('CreatePictureNotification', createPictureNotification);
var activeNotification = undefined;
function createPictureNotification(notificationJson) {
    var notification = JSON.parse(notificationJson);
}
alt.onServer('createNotification', createNotification);
function createNotification(message, blink) {
    native.beginTextCommandThefeedPost('STRING');
    native.addTextComponentSubstringPlayerName(message);
    native.endTextCommandThefeedPostTicker(blink, true);
}
export async function ShowIconNotification(message, title, subtitle, icon, color = undefined, blink = false) {
    icon = icon.toUpperCase();
    if (icon === 'PLAYER') {
        let pedHeadshot = native.registerPedheadshot(alt.Player.local.scriptID);
        icon = await loadPlayerHead(pedHeadshot);
    }
    native.beginTextCommandThefeedPost('STRING');
    PushLongString(message, textblock => {
        native.addTextComponentSubstringPlayerName(textblock);
    });
    native.endTextCommandThefeedPostMessagetext(icon, icon, false, 0, title, subtitle);
    if (color)
        native.thefeedSetNextPostBackgroundColor(color);
    native.endTextCommandThefeedPostTicker(blink, false);
    native.unregisterPedheadshot(alt.Player.local.scriptID);
}
function loadPlayerHead(pedHeadshot) {
    return new Promise(resolve => {
        let interval = alt.setInterval(() => {
            if (native.isPedheadshotReady(pedHeadshot) && native.isPedheadshotValid(pedHeadshot)) {
                alt.clearInterval(interval);
                return resolve(native.getPedheadshotTxdString(pedHeadshot));
            }
        }, 0);
    });
}
function PushLongString(message, action) {
    message.match(/.{1,99}/g).forEach(textBlock => {
        action(textBlock);
    });
}
