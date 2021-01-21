import * as alt from 'alt-client';
import * as native from 'natives';

alt.onServer('CreatePictureNotification', createPictureNotification);

var activeNotification: any = undefined;

function createPictureNotification(notificationJson: string) {
    var notification = JSON.parse(notificationJson);
/*
    native.beginTextCommandThefeedPost("STRING");
    native.addTextComponentSubstringPlayerName(notification.Message);

    activeNotification = native.setNotificationMessage(notification.NotificationType,
        notification.NotificationType,
        notification.FlashNotification,
        4,
        0,
        notification.Title,
        notification.Description);
    native.endTextCommandThefeedPostTicker(notification.FlashNotification, false);*/
}

alt.onServer('createNotification', createNotification);

function createNotification(message: string, blink: boolean) {
    native.beginTextCommandThefeedPost('STRING');
    native.addTextComponentSubstringPlayerName(message);
    native.endTextCommandThefeedPostTicker(blink, true);
}

export async function ShowIconNotification(
    message:string,
    title:string,
    subtitle:string,
    icon:string,
    color:any = undefined,
    blink:boolean = false
) {
    icon = icon.toUpperCase();
    if (icon === 'PLAYER') {
        let pedHeadshot = native.registerPedheadshot(alt.Player.local.scriptID);
        icon = <string>await loadPlayerHead(pedHeadshot);
    }

    native.beginTextCommandThefeedPost('STRING');
    PushLongString(message, textblock => {
        native.addTextComponentSubstringPlayerName(textblock);
    });

    // Set the notification icon, title and subtitle.
    native.endTextCommandThefeedPostMessagetext(icon, icon, false, 0, title, subtitle);
    if (color) native.thefeedSetNextPostBackgroundColor(color);
    native.endTextCommandThefeedPostTicker(blink, false);

    native.unregisterPedheadshot(alt.Player.local.scriptID);
}

function loadPlayerHead(pedHeadshot:number) {
    return new Promise(resolve => {
        let interval = alt.setInterval(() => {
            if (native.isPedheadshotReady(pedHeadshot) && native.isPedheadshotValid(pedHeadshot)) {
                alt.clearInterval(interval);
                return resolve(native.getPedheadshotTxdString(pedHeadshot));
            }
        }, 0);
    });
}

function PushLongString(message: string, action: { (textBlock: string): void }) {
    message.match(/.{1,99}/g).forEach(textBlock => {
        action(textBlock);
    });
}