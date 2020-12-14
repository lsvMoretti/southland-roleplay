import * as alt from 'alt-client';
import * as native from 'natives';
var highlightColor = 0;
var hairColor = 0;
alt.onServer('HairStore:HighlightColor', (highlightIndex, colorIndex) => {
    highlightColor = highlightIndex;
    hairColor = colorIndex;
});
alt.onServer('HairStore:HairColorChange', onHairColorChange);
function onHairColorChange(type, index) {
    var player = alt.Player.local.scriptID;
    if (type === 0) {
        native.setPedHairColor(player, index, highlightColor);
        return;
    }
    if (type === 1) {
        native.setPedHairColor(player, hairColor, index);
        return;
    }
    if (type === 2) {
        native.setPedHeadOverlayColor(player, 1, 1, index, 0);
    }
    if (type === 3) {
        native.setPedHeadOverlayColor(player, 2, 1, index, 0);
    }
    if (type === 4) {
        native.setPedHeadOverlayColor(player, 10, 1, index, 0);
    }
}
