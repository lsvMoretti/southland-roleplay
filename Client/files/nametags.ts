import * as alt from 'alt-client';
import * as native from 'natives';
import Screen from "./NativeUi/utils/Screen";

let nameTagEnabled: boolean = true;
let interval: number;
let drawDistance: number = 20;

export function ToggleNameTags(state: boolean) {
    nameTagEnabled = state;
}

export function StartNameTagDraw() {
    interval = alt.setInterval(drawNameTags, 0);
}

function distance2d(vector1: alt.Vector3, vector2: alt.Vector3) {
    return Math.sqrt(Math.pow(vector1.x - vector2.x, 2) + Math.pow(vector1.y - vector2.y, 2));
}

function drawAme(text: string, pos: native.Vector3, vector: native.Vector3, frameTime: number, scale: number, r: number, g: number, b: number, a: number, outline: boolean) {
    // pos.z already += 0.75
    pos.z += 0.5;

    native.setDrawOrigin(
        pos.x + vector.x * frameTime,
        pos.y + vector.y * frameTime,
        pos.z + vector.z * frameTime,
        0
    );

    native.setTextFont(0);
    native.setTextProportional(false);
    native.setTextScale(scale, scale);
    native.setTextColour(r, g, b, a);
    native.setTextDropshadow(0, 0, 0, 0, 255);
    native.setTextDropShadow();
    native.setTextEdge(2, 0, 0, 0, 255);
    native.setTextCentre(true);
    native.setTextDropShadow();

    if (outline) native.setTextOutline();

    native.beginTextCommandDisplayText("STRING");

    native.addTextComponentSubstringPlayerName("* " + text);
    native.endTextCommandDisplayText(0, 0, 0);

    native.clearDrawOrigin();
}

function drawNameTags() {
    if (!nameTagEnabled) return;

    for (let i = 0, n = alt.Player.all.length; i < n; i++) {
        const player = alt.Player.all[i];

        if (!player.valid) continue;

        if (player.scriptID === alt.Player.local.scriptID) continue;

        const name = player.getSyncedMeta("playerNameTag");
        const playerId = player.getSyncedMeta("playerId");

        if (!name) continue;

        if (!native.hasEntityClearLosToEntity(alt.Player.local.scriptID, player.scriptID, 17)) continue;

        const dist = distance2d(player.pos, alt.Player.local.pos);
        const stealthStatus = player.getSyncedMeta("StealthStatus");

        if (stealthStatus) {
            drawDistance = 10;
        }

        if (dist > drawDistance) continue;

        const isTyping: boolean = player.getSyncedMeta("TypeStatus");
        const isDown: boolean = player.getSyncedMeta("ISDOWNED");
        const pos: native.Vector3 = { ...native.getPedBoneCoords(player.scriptID, 12844, 0, 0, 0) };
        pos.z += 0.5;

        const scale = 1 - (0.8 * dist) / drawDistance;
        const fontSize = 0.4 * scale;

        const lineHeight = native.getTextScaleHeight(fontSize, 4);
        const entity = player.vehicle ? player.vehicle.scriptID : player.scriptID;
        const vector = native.getEntityVelocity(entity);
        const frameTime = native.getFrameTime();

        native.setDrawOrigin(
            pos.x + vector.x * frameTime,
            pos.y + vector.y * frameTime,
            pos.z + vector.z * frameTime,
            0
        );

        native.beginTextCommandDisplayText('STRING');
        native.setTextFont(4);
        native.setTextScale(fontSize, fontSize);
        native.setTextProportional(true);
        native.setTextCentre(true);
        native.setTextColour(255, 255, 255, 255);
        native.setTextOutline();
        if (isDown) {
            native.addTextComponentSubstringPlayerName(`~r~Downed\n~w~${name} (${playerId})`);
        } else {
            native.addTextComponentSubstringPlayerName(isTyping ? `Typing..\n${name} (${playerId})` : `${name} (${playerId})`);
        }
        native.endTextCommandDisplayText(0, 0, 0);

        // aMe
        let ameActive: boolean = player.getSyncedMeta("ChatCommand:AmeActive");

        if (ameActive) {
            let aMe = player.getSyncedMeta("ChatCommand:Ame");
            drawAme(aMe, pos, vector, frameTime, fontSize, 194, 162, 218, 175, true);
        }

        native.clearDrawOrigin();
    }
}

/*
alt.everyTick(() => {
    if (!nameTagEnabled) return;

    let players = alt.Player.all;

    if (players.length === 0) return;

    let localPlayer = alt.Player.local;
    let playerPos = native.getEntityCoords(localPlayer.scriptID, true);

    for (var i = 0; i < players.length; i++) {
        var player = players[i];

        var playerPos2 = native.getEntityCoords(player.scriptID, true);
        let distance = native.getDistanceBetweenCoords(playerPos.x, playerPos.y, playerPos.z, playerPos2.x, playerPos2.y, playerPos2.z, true);

        let playerName = player.getSyncedMeta("playerNameTag");
        let playerId = player.getSyncedMeta("playerId");
        let isTyping: boolean = player.getSyncedMeta("TypeStatus");
        let stealthStatus = player.getSyncedMeta("StealthStatus");

        if (playerName == undefined) continue;

        let isSpectating: boolean = player.getSyncedMeta("IsSpectating");

        let isPdDuty: boolean = player.getSyncedMeta("PoliceDuty");

        let isHelperDuty: boolean = player.getSyncedMeta("HELPERONDUTY");

        let canSee = native.hasEntityClearLosToEntity(player.scriptID, localPlayer.scriptID, 17);

        let ameActive: boolean = player.getSyncedMeta("ChatCommand:AmeActive");

        let aMe: string;

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

            if(player.vehicle != null){
                // In a vehicle
                let seatId = null;
                let maxPassengers = native.getVehicleMaxNumberOfPassengers(player.vehicle.scriptID);
                for(let vi = -1; vi < maxPassengers; vi++){
                    let pedInSeat = native.getPedInVehicleSeat(player.vehicle.scriptID, vi, null);
                    if(pedInSeat == player.scriptID){
                        seatId = vi;
                        break;
                    }
                }
                if(seatId == null) return;

                for (let seatPos = -1; seatPos < seatId; seatPos++){
                    // 0.05
                    screenPos[2] = screenPos[2] - 0.04;
                }
            }

            if (ameActive) {
                drawAme(aMe, screenPos[1], screenPos[2] - 0.070, scale, 194, 162, 218, 175, true);
            }

            if (isPdDuty) {
                drawText(playerName, playerId, screenPos[1], screenPos[2] - 0.030, scale, 0, 144, 255, 175, true, isTyping);
            } if(isHelperDuty) {
                drawText(playerName, playerId, screenPos[1], screenPos[2] - 0.030, scale, 242, 33, 33, 175, true, isTyping);
            }
            else {
                drawText(playerName, playerId, screenPos[1], screenPos[2] - 0.030, scale, 255, 255, 255, 175, true, isTyping);
            }
        }
    }
});*/

function drawText(name: string, id: string, x: number, y: number, scale: number, r: number, g: number, b: number, a: number, outline: boolean, typing: boolean) {
    native.setTextFont(0);
    native.setTextProportional(false);
    native.setTextScale(scale, scale);
    native.setTextColour(r, g, b, a);
    native.setTextDropshadow(0, 0, 0, 0, 255);
    native.setTextDropShadow();
    native.setTextEdge(2, 0, 0, 0, 255);
    native.setTextCentre(true);
    native.setTextDropShadow();

    if (outline) native.setTextOutline();

    native.beginTextCommandDisplayText("STRING");
    if (typing) {
        native.addTextComponentSubstringPlayerName("Typing..\n" + name + " (" + id + ")");
    } else {
        native.addTextComponentSubstringPlayerName(name + " (" + id + ")");
    }

    native.endTextCommandDisplayText(x, y, 0);
}