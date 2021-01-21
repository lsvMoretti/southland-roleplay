import * as alt from 'alt-client';
import * as native from 'natives';

var totalBlips = 0;
var blips = new Array();

// Create a new blip.
alt.onServer('createLocalBlip', (blipJson: string) => {
    var blipInfo = JSON.parse(blipJson);
    alt.log("Creating Blip: " + blipInfo.Name);
    let blip = native.addBlipForCoord(blipInfo.PosX, blipInfo.PosY, blipInfo.PosZ);
    native.setBlipSprite(blip, blipInfo.Sprite);
    native.setBlipColour(blip, blipInfo.Color);
    native.setBlipScale(blip, blipInfo.Scale);
    native.setBlipAsShortRange(blip, blipInfo.ShortRange);
    native.beginTextCommandSetBlipName('STRING');
    native.addTextComponentSubstringPlayerName(blipInfo.Name);
    native.endTextCommandSetBlipName(blip);

    if (blipInfo.UniqueId == -1) {
        totalBlips += 1;
        blipInfo.UniqueId = `${totalBlips}`;
    }

    if (blips[blipInfo.UniqueId] !== undefined) {
        native.removeBlip(blips[blipInfo.UniqueId]);
    }

    blips[blipInfo.UniqueId] = blip;
});

// Delete a blip by uniqueID
alt.onServer('deleteLocalBlip', (uniqueId:number) => {
    if (blips[uniqueId] !== undefined) {
        native.removeBlip(blips[uniqueId]);
        delete blips[uniqueId];
    }
});

alt.onServer('deleteAllBlips', () => {
    blips.forEach((blip) => {
        native.removeBlip(blip);
    });

    blips = new Array();
});