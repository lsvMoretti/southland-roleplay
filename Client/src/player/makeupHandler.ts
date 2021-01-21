import * as alt from 'alt-client';
import * as native from 'natives';

alt.onServer("Makeup:OnListChange", onListItemChange);

alt.onServer("Makeup:OnColorChange", onColorChange);

function onListItemChange(item:number, index:number, opacity:number){
    var localPlayer = alt.Player.local.scriptID;

    if(item <= 11){
        // Setting Ped Head Overlays

        native.setPedHeadOverlay(localPlayer, item, index, opacity);
    }
}

function onColorChange(overlay:number, colorType:number, colorId:number){
    var localPlayer = alt.Player.local.scriptID;

    native.setPedHeadOverlayColor(localPlayer, overlay, colorType, colorId, colorId);
}

