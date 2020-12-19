import * as alt from 'alt-client';
import * as native from 'natives';
var nameTagEnabled = true;
export function ToggleNameTags(state) {
    nameTagEnabled = state;
}
alt.everyTick(() => {
    if (!nameTagEnabled)
        return;
    let players = alt.Player.all;
    if (players.length === 0)
        return;
    let localPlayer = alt.Player.local;
    let playerPos = native.getEntityCoords(localPlayer.scriptID, true);
    for (var i = 0; i < players.length; i++) {
        var player = players[i];
        var playerPos2 = native.getEntityCoords(player.scriptID, true);
        let distance = native.getDistanceBetweenCoords(playerPos.x, playerPos.y, playerPos.z, playerPos2.x, playerPos2.y, playerPos2.z, true);
        let playerName = player.getSyncedMeta("playerNameTag");
        let playerId = player.getSyncedMeta("playerId");
        let isTyping = player.getSyncedMeta("TypeStatus");
        let stealthStatus = player.getSyncedMeta("StealthStatus");
        if (playerName == undefined)
            continue;
        let isSpectating = player.getSyncedMeta("IsSpectating");
        let isPdDuty = player.getSyncedMeta("PoliceDuty");
        let canSee = native.hasEntityClearLosToEntity(player.scriptID, localPlayer.scriptID, 17);
        let ameActive = player.getSyncedMeta("ChatCommand:AmeActive");
        let aMe;
        if (ameActive) {
            aMe = player.getSyncedMeta("ChatCommand:Ame");
        }
        let range = 15;
        if (stealthStatus !== undefined && stealthStatus == true) {
            range = 7.5;
        }
        if (distance <= range && player != localPlayer && canSee && !isSpectating) {
            let scale = distance / (range * range);
            if (scale < 0.3) {
                scale = 0.3;
            }
            let screenPos = native.getScreenCoordFromWorldCoord(playerPos2.x, playerPos2.y, playerPos2.z + 1, null, null);
            if (ameActive) {
                drawAme(aMe, screenPos[1], screenPos[2] - 0.070, scale, 194, 162, 218, 175, true);
            }
            if (isPdDuty) {
                drawText(playerName, playerId, screenPos[1], screenPos[2] - 0.030, scale, 0, 144, 255, 175, true, isTyping);
            }
            else {
                drawText(playerName, playerId, screenPos[1], screenPos[2] - 0.030, scale, 255, 255, 255, 175, true, isTyping);
            }
        }
    }
});
function drawText(name, id, x, y, scale, r, g, b, a, outline, typing) {
    native.setTextFont(0);
    native.setTextProportional(false);
    native.setTextScale(scale, scale);
    native.setTextColour(r, g, b, a);
    native.setTextDropshadow(0, 0, 0, 0, 255);
    native.setTextDropShadow();
    native.setTextEdge(2, 0, 0, 0, 255);
    native.setTextCentre(true);
    native.setTextDropShadow();
    if (outline)
        native.setTextOutline();
    native.beginTextCommandDisplayText("STRING");
    if (typing) {
        native.addTextComponentSubstringPlayerName("Typing..\n" + name + " (" + id + ")");
    }
    else {
        native.addTextComponentSubstringPlayerName(name + " (" + id + ")");
    }
    native.endTextCommandDisplayText(x, y, 0);
}
function drawAme(text, x, y, scale, r, g, b, a, outline) {
    native.setTextFont(0);
    native.setTextProportional(false);
    native.setTextScale(scale, scale);
    native.setTextColour(r, g, b, a);
    native.setTextDropshadow(0, 0, 0, 0, 255);
    native.setTextDropShadow();
    native.setTextEdge(2, 0, 0, 0, 255);
    native.setTextCentre(true);
    native.setTextDropShadow();
    if (outline)
        native.setTextOutline();
    native.beginTextCommandDisplayText("STRING");
    native.addTextComponentSubstringPlayerName("* " + text);
    native.endTextCommandDisplayText(x, y, 0);
}
