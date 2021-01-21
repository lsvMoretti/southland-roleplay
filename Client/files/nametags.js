import * as alt from 'alt-client';
import * as native from 'natives';
let nameTagEnabled = true;
let interval;
let drawDistance = 20;
export function ToggleNameTags(state) {
    nameTagEnabled = state;
}
export function StartNameTagDraw() {
    interval = alt.setInterval(drawNameTags, 0);
}
function distance2d(vector1, vector2) {
    return Math.sqrt(Math.pow(vector1.x - vector2.x, 2) + Math.pow(vector1.y - vector2.y, 2));
}
function drawAme(text, pos, vector, frameTime, scale, r, g, b, a, outline) {
    pos.z += 0.5;
    native.setDrawOrigin(pos.x + vector.x * frameTime, pos.y + vector.y * frameTime, pos.z + vector.z * frameTime, 0);
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
    native.endTextCommandDisplayText(0, 0, 0);
    native.clearDrawOrigin();
}
function drawNameTags() {
    if (!nameTagEnabled)
        return;
    for (let i = 0, n = alt.Player.all.length; i < n; i++) {
        const player = alt.Player.all[i];
        if (!player.valid)
            continue;
        if (player.scriptID === alt.Player.local.scriptID)
            continue;
        const name = player.getSyncedMeta("playerNameTag");
        const playerId = player.getSyncedMeta("playerId");
        if (!name)
            continue;
        if (!native.hasEntityClearLosToEntity(alt.Player.local.scriptID, player.scriptID, 17))
            continue;
        const dist = distance2d(player.pos, alt.Player.local.pos);
        const stealthStatus = player.getSyncedMeta("StealthStatus");
        if (stealthStatus) {
            drawDistance = 10;
        }
        if (dist > drawDistance)
            continue;
        const isTyping = player.getSyncedMeta("TypeStatus");
        const isDown = player.getSyncedMeta("ISDOWNED");
        const pos = { ...native.getPedBoneCoords(player.scriptID, 12844, 0, 0, 0) };
        pos.z += 0.5;
        const scale = 1 - (0.8 * dist) / drawDistance;
        const fontSize = 0.4 * scale;
        const lineHeight = native.getTextScaleHeight(fontSize, 4);
        const entity = player.vehicle ? player.vehicle.scriptID : player.scriptID;
        const vector = native.getEntityVelocity(entity);
        const frameTime = native.getFrameTime();
        native.setDrawOrigin(pos.x + vector.x * frameTime, pos.y + vector.y * frameTime, pos.z + vector.z * frameTime, 0);
        native.beginTextCommandDisplayText('STRING');
        native.setTextFont(4);
        native.setTextScale(fontSize, fontSize);
        native.setTextProportional(true);
        native.setTextCentre(true);
        native.setTextColour(255, 255, 255, 255);
        native.setTextOutline();
        if (isDown) {
            native.addTextComponentSubstringPlayerName(`~r~Downed\n~w~${name} (${playerId})`);
        }
        else {
            native.addTextComponentSubstringPlayerName(isTyping ? `Typing..\n${name} (${playerId})` : `${name} (${playerId})`);
        }
        native.endTextCommandDisplayText(0, 0, 0);
        let ameActive = player.getSyncedMeta("ChatCommand:AmeActive");
        if (ameActive) {
            let aMe = player.getSyncedMeta("ChatCommand:Ame");
            drawAme(aMe, pos, vector, frameTime, fontSize, 194, 162, 218, 175, true);
        }
        native.clearDrawOrigin();
    }
}
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
