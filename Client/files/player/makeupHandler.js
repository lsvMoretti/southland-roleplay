import * as alt from 'alt-client';
import * as native from 'natives';
alt.onServer("Makeup:OnListChange", onListItemChange);
alt.onServer("Makeup:OnColorChange", onColorChange);
function onListItemChange(item, index, opacity) {
    var localPlayer = alt.Player.local.scriptID;
    if (item <= 11) {
        native.setPedHeadOverlay(localPlayer, item, index, opacity);
    }
}
function onColorChange(overlay, colorType, colorId) {
    var localPlayer = alt.Player.local.scriptID;
    native.setPedHeadOverlayColor(localPlayer, overlay, colorType, colorId, colorId);
}
