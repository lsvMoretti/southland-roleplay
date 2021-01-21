import * as alt from 'alt-client';
import * as native from 'natives';
import './extensions';
import './player/PlayerHandler';
import * as extensions from './extensions';
var textLabelJsonArray = new Array();
var textLabelArray = new Array();
alt.onServer('createTextLabel', createTextLabel);
alt.onServer('deleteTextLabel', deleteTextLabel);
alt.onServer('deleteAllTextLabels', () => {
    textLabelJsonArray = new Array();
    textLabelArray = new Array();
});
function createTextLabel(textLabelJson) {
    textLabelJsonArray.push(textLabelJson);
    var textLabel = JSON.parse(textLabelJson);
    textLabel.pos = new alt.Vector3(textLabel.PosX, textLabel.PosY, textLabel.PosZ);
    textLabelArray.push(textLabel);
}
function displayTextLabel(textLabel) {
    let playerPos = alt.Player.local.pos;
    var range = textLabel.Range;
    var dimension = alt.Player.local.getSyncedMeta("PlayerDimension");
    if (dimension != textLabel.Dimension)
        return;
    if (extensions.Distance(playerPos, textLabel.pos) < range) {
        const [bol, _x, _y] = native.getScreenCoordFromWorldCoord(textLabel.PosX, textLabel.PosY, textLabel.PosZ, null, null);
        const camCord = native.getFinalRenderedCamCoord();
        const dist = native.getDistanceBetweenCoords(camCord.x, camCord.y, camCord.z, textLabel.PosX, textLabel.PosY, textLabel.PosZ, true);
        let scale = (4.00001 / dist) * 0.3;
        if (scale > 0.2)
            scale = 0.2;
        const fov = (1 / native.getGameplayCamFov()) * 100;
        scale = scale * fov;
        if (bol) {
            native.setTextScale(scale, scale);
            native.setTextFont(textLabel.Font);
            native.setTextProportional(true);
            native.setTextColour(textLabel.Color.R, textLabel.Color.G, textLabel.Color.B, textLabel.Color.A);
            native.setTextDropshadow(0, 0, 0, 0, 255);
            native.setTextEdge(2, 0, 0, 0, 150);
            native.setTextDropShadow();
            native.setTextOutline();
            native.setTextCentre(true);
            native.beginTextCommandDisplayText("STRING");
            native.addTextComponentSubstringPlayerName(textLabel.Text);
            native.endTextCommandDisplayText(_x, _y + 0.025, 0);
        }
    }
}
function deleteTextLabel(textLabelJson) {
    for (let index = 0; index < textLabelJsonArray.length; index++) {
        const element = textLabelJsonArray[index];
        if (element === textLabelJson) {
            textLabelJsonArray.splice(index, 1);
        }
    }
    for (let index = 0; index < textLabelArray.length; index++) {
        const element = textLabelArray[index];
        if (element === JSON.parse(textLabelJson)) {
            textLabelArray.splice(index, 1);
        }
    }
}
alt.everyTick(() => {
    processTextLabels();
});
function processTextLabels() {
    if (textLabelArray.length === 0)
        return;
    textLabelArray.forEach(element => {
        displayTextLabel(element);
    });
}
