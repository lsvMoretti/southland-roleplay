import * as alt from 'alt-client';
import * as native from 'natives';
alt.onServer('Blindfolded', (blindfolded) => {
    if (blindfolded) {
        native.doScreenFadeOut(1000);
    }
    else {
        native.doScreenFadeIn(1000);
    }
});
alt.onServer('freezePlayer', freezePlayer);
alt.onServer('OnTimeUpdate', () => {
    alt.setMsPerGameMinute(30000);
});
function freezePlayer(state) {
    native.freezeEntityPosition(alt.Player.local.scriptID, state);
}
var hudEnabled = false;
var freezeCam = false;
var freezeInput = false;
alt.onServer('toggleHud', toggleHud);
alt.onServer('toggleCursor', toggleCursorFunction);
alt.onServer('LoadDLC', () => {
    native.onEnterSp();
    native.onEnterMp();
});
function toggleCursorFunction(state) {
    alt.showCursor(state);
}
function toggleHud(state) {
    hudEnabled = state;
    alt.emit('hud:SetState', state);
}
export function GetHudState() {
    return hudEnabled;
}
alt.onServer('freezeCam', (state) => {
    freezeCam = state;
});
alt.onServer('freezeInput', (state) => {
    freezeInput = state;
});
alt.everyTick(() => {
    if (hudEnabled === false) {
    }
    if (freezeInput) {
        native.disableAllControlActions(0);
    }
    if (freezeCam) {
        native.disableAllControlActions(1);
    }
});
alt.onServer('loadIpl', (iplString) => {
    native.requestIpl(iplString);
});
alt.onServer('unloadIpl', (iplString) => {
    native.removeIpl(iplString);
});
alt.onServer('loadProp', (propName) => {
    alt.setTimeout(() => {
        let player = alt.Player.local;
        let interiorId = native.getInteriorAtCoords(player.pos.x, player.pos.y, player.pos.z);
        alt.log("Inside interior: " + interiorId);
        alt.log("Loading Prop: " + propName);
        native.activateInteriorEntitySet(interiorId, propName);
        native.setInteriorEntitySetColor(interiorId, propName, 1);
        native.refreshInterior(interiorId);
    }, 1);
});
alt.onServer('unloadProp', (propName) => {
    alt.setTimeout(() => {
        let player = alt.Player.local;
        let interiorId = native.getInteriorAtCoords(player.pos.x, player.pos.y, player.pos.z);
        alt.log("Inside interior: " + interiorId);
        alt.log("Unloading Prop: " + propName);
        native.deactivateInteriorEntitySet(interiorId, propName);
        native.refreshInterior(interiorId);
    }, 1);
});
alt.onServer('SetInteriorColor', (propName, colorId) => {
    let player = alt.Player.local;
    let interiorId = native.getInteriorAtCoords(player.pos.x, player.pos.y, player.pos.z);
    native.setInteriorEntitySetColor(interiorId, propName, colorId);
    native.refreshInterior(interiorId);
});
var characterLoaded = false;
alt.onServer("CharacterLoaded", CharacterLoaded);
function CharacterLoaded(state) {
    characterLoaded = state;
    alt.emit('hud:CharacterLoaded');
}
export function IsCharacterLoaded() {
    return characterLoaded;
}
alt.onServer('setWaypoint', (pos) => {
    native.setNewWaypoint(pos.x, pos.y);
});
alt.onServer('clearWaypoint', () => {
    native.setWaypointOff();
});
alt.onServer('fetchPlayerStreetArea', (eventName) => {
    var streetNameValue = 0;
    var crossingPoint = 0;
    var pos = alt.Player.local.pos;
    var zoneId = native.getNameOfZone(pos.x, pos.y, pos.z);
    var streetResult = native.getStreetNameAtCoord(pos.x, pos.y, pos.z, streetNameValue, crossingPoint);
    var streetName = native.getStreetNameFromHashKey(streetResult[1]);
    var zoneNameShort = ["AIRP", "ALAMO", "ALTA", "ARMYB", "BANHAMC", "BANNING", "BEACH", "BHAMCA", "BRADP", "BRADT", "BURTON", "CALAFB", "CANNY", "CCREAK", "CHAMH", "CHIL", "CHU", "CMSW", "CYPRE", "DAVIS", "DELBE", "DELPE", "DELSOL", "DESRT", "DOWNT", "DTVINE", "EAST_V", "EBURO", "ELGORL", "ELYSIAN", "GALFISH", "GOLF", "GRAPES", "GREATC", "HARMO", "HAWICK", "HORS", "HUMLAB", "JAIL", "KOREAT", "LACT", "LAGO", "LDAM", "LEGSQU", "LMESA", "LOSPUER", "MIRR", "MORN", "MOVIE", "MTCHIL", "MTGORDO", "MTJOSE", "MURRI", "NCHU", "NOOSE", "OCEANA", "PALCOV", "PALETO", "PALFOR", "PALHIGH", "PALMPOW", "PBLUFF", "PBOX", "PROCOB", "RANCHO", "RGLEN", "RICHM", "ROCKF", "RTRAK", "SANAND", "SANCHIA", "SANDY", "SKID", "SLAB", "STAD", "STRAW", "TATAMO", "TERMINA", "TEXTI", "TONGVAH", "TONGVAV", "VCANA", "VESP", "VINE", "WINDF", "WVINE", "ZANCUDO", "ZP_ORT", "ZQ_UAR"];
    var zoneListComp = ["Los Santos International Airport", "Alamo Sea", "Alta", "Fort Zancudo", "Banham Canyon Dr", "Banning", "Vespucci Beach", "Banham Canyon", "Braddock Pass", "Braddock Tunnel", "Burton", "Calafia Bridge", "Raton Canyon", "Cassidy Creek", "Chamberlain Hills", "Vinewood Hills", "Chumash", "Chiliad Mountain State Wilderness", "Cypress Flats", "Davis", "Del Perro Beach", "Del Perro", "La Puerta", "Grand Senora Desert", "Downtown", "Downtown Vinewood", "East Vinewood", "El Burro Heights", "El Gordo Lighthouse", "Elysian Island", "Galilee", "GWC and Golfing Society", "Grapeseed", "Great Chaparral", "Harmony", "Hawick", "Vinewood Racetrack", "Humane Labs and Research", "Bolingbroke Penitentiary", "Little Seoul", "Land Act Reservoir", "Lago Zancudo", "Land Act Dam", "Legion Square", "La Mesa", "La Puerta", "Mirror Park", "Morningwood", "Richards Majestic", "Mount Chiliad", "Mount Gordo", "Mount Josiah", "Murrieta Heights", "North Chumash", "N.O.O.S.E", "Pacific Ocean", "Paleto Cove", "Paleto Bay", "Paleto Forest", "Palomino Highlands", "Palmer - Taylor Power Station", "Pacific Bluffs", "Pillbox Hill", "Procopio Beach", "Rancho", "Richman Glen", "Richman", "Rockford Hills", "Redwood Lights Track", "San Andreas", "San Chianski Mountain Range", "Sandy Shores", "Mission Row", "Stab City", "Maze Bank Arena", "Strawberry", "Tataviam Mountains", "Terminal", "Textile City", "Tongva Hills", "Tongva Valley", "Vespucci Canals", "Vespucci", "Vinewood", "Ron Alternates Wind Farm", "West Vinewood", "Zancudo River", "Port of South Los Santos", "Davis Quartz"];
    var zoneIndex = zoneNameShort.indexOf(zoneId);
    var zoneName = zoneListComp[zoneIndex];
    alt.emitServer(eventName, streetName, zoneName);
});
alt.onServer('switchOutPlayer', switchOutPlayer);
function switchOutPlayer(timeout) {
    var player = alt.Player.local.scriptID;
    native.doScreenFadeOut(1000);
    alt.setTimeout(() => {
        native.doScreenFadeIn(1000);
    }, timeout);
}
alt.onServer('clearBlood', clearBlood);
function clearBlood(entity) {
    native.resetPedVisibleDamage(alt.Player.local.scriptID);
    native.clearPedBloodDamage(alt.Player.local.scriptID);
}
alt.setStat('stamina', 100);
alt.setStat('strength', 100);
alt.setStat('lung_capacity', 100);
alt.setStat('wheelie_ability', 100);
alt.setStat('flying_ability', 100);
alt.setStat('shooting_ability', 100);
alt.setStat('stealth_ability', 100);
alt.onServer('StartScreenEvent', (effectName, duration, looped) => {
    native.animpostfxPlay(effectName, duration, looped);
});
alt.onServer('StopScreenEvent', () => {
    native.animpostfxStopAll();
});
var specCam = undefined;
var specPlayer = undefined;
var specInterval = undefined;
alt.onServer('ToggleSpectate', (state, player) => {
    if (state) {
        if (specCam !== undefined) {
            closeSpectatorCam();
        }
        native.setEntityVisible(alt.Player.local.scriptID, false, false);
        alt.setTimeout(() => {
            specPlayer = player;
            native.freezeEntityPosition(alt.Player.local.scriptID, true);
            specCam = native.createCam("DEFAULT_SCRIPTED_CAMERA", true);
            native.setCamActive(specCam, true);
            native.renderScriptCams(true, false, 0, true, false, null);
            alt.log('Setting cam ' + specCam + ' to pos: ' + player.pos.x + ' ' + player.pos.y + ' ' + player.pos.z);
            native.setCamCoord(specCam, player.pos.x, player.pos.y, player.pos.z);
            native.setCamFov(specCam, 90);
            native.setFocusPosAndVel(player.pos.x, player.pos.y, player.pos.z, 0.0, 0.0, 0.0);
            native.setHdArea(player.pos.x, player.pos.y, player.pos.z, 30);
            native.attachCamToPedBone(specCam, player.scriptID, 0, -3, 0.1, 1.5, false);
            native.pointCamAtEntity(specCam, player.scriptID, 0, 0, 0, true);
            alt.emitServer('admin:spectate:setPlayerPos');
            specInterval = alt.setInterval(() => {
                alt.emitServer('admin:spectate:UpdatePosition');
            }, 5000);
        }, 2000);
        return;
    }
    closeSpectatorCam();
    native.setEntityVisible(alt.Player.local.scriptID, true, false);
});
alt.onServer('ToggleTransparency', (state) => {
    native.setEntityVisible(alt.Player.local.scriptID, state, false);
});
alt.everyTick(() => {
    if (specPlayer != undefined) {
        var players = alt.Player.all;
        var player;
        for (var i = 0; i < players.length; i++) {
            if (players[i] == specPlayer) {
                player = players[i];
            }
        }
        if (player == undefined && specPlayer != undefined) {
            closeSpectatorCam();
            return;
        }
        native.setHdArea(player.pos.x, player.pos.y, player.pos.z, 30);
        if (player.vehicle != null) {
            native.pointCamAtEntity(specCam, player.vehicle.scriptID, 0, 0, 0, true);
        }
        else {
            native.pointCamAtEntity(specCam, player.scriptID, 0, 0, 0, true);
        }
    }
});
function closeSpectatorCam() {
    native.renderScriptCams(false, false, 0, true, false, null);
    native.destroyCam(specCam, true);
    native.destroyAllCams(true);
    specCam = undefined;
    specPlayer = undefined;
    native.setFollowPedCamViewMode(1);
    native.clearFocus();
    native.freezeEntityPosition(alt.Player.local.scriptID, false);
    native.setEntityVisible(alt.Player.local.scriptID, true, false);
    alt.clearInterval(specInterval);
    specInterval = undefined;
    alt.emitServer('admin:OnSpectateFinish');
}
alt.onServer('player:FetchForwardPosition', (returnEvent, distance) => {
    let positionInFrontOfPlayer = PositionInFront(alt.Player.local.pos, native.getEntityRotation(alt.Player.local.scriptID, 2), distance);
    alt.log('Position:' +
        positionInFrontOfPlayer.x +
        ',' +
        positionInFrontOfPlayer.y +
        ',' +
        positionInFrontOfPlayer.z);
    alt.emitServer(returnEvent, positionInFrontOfPlayer.x, positionInFrontOfPlayer.y, positionInFrontOfPlayer.z);
});
function ForwardVectorFromRotation(rotation) {
    let z = rotation.z * (Math.PI / 180.0);
    let x = rotation.x * (Math.PI / 180.0);
    let num = Math.abs(Math.cos(x));
    return new alt.Vector3(-Math.sin(z) * num, Math.cos(z) * num, Math.sin(x));
}
function PositionInFront(position, rotation, distance) {
    let forwardVector = ForwardVectorFromRotation(rotation);
    let scaledForwardVector = new alt.Vector3(forwardVector.x * distance, forwardVector.y * distance, forwardVector.z * distance);
    return new alt.Vector3(position.x + scaledForwardVector.x, position.y + scaledForwardVector.y, position.z + scaledForwardVector.z);
}
alt.onServer('SetWalkStyle', (style) => {
    let playerId = alt.Player.local.scriptID;
    if (style === 0) {
        native.resetPedMovementClipset(playerId, 0.0);
        return;
    }
    var walkName = "";
    switch (style) {
        case 1:
            walkName = "ANIM_GROUP_MOVE_BALLISTIC";
            break;
        case 2:
            walkName = "ANIM_GROUP_MOVE_LEMAR_ALLEY";
            break;
        case 3:
            walkName = "clipset@move@trash_fast_turn";
            break;
        case 4:
            walkName = "FEMALE_FAST_RUNNER";
            break;
        case 5:
            walkName = "missfbi4prepp1_garbageman";
            break;
        case 6:
            walkName = "move_characters@franklin@fire";
            break;
        case 7:
            walkName = "move_characters@Jimmy@slow@";
            break;
        case 8:
            walkName = "move_characters@michael@fire";
            break;
        case 9:
            walkName = "move_f@flee@a";
            break;
        case 10:
            walkName = "move_f@scared";
            break;
        case 11:
            walkName = "move_f@sexy@a";
            break;
        case 12:
            walkName = "move_heist_lester";
            break;
        case 13:
            walkName = "move_injured_generic";
            break;
        case 14:
            walkName = "move_lester_CaneUp";
            break;
        case 15:
            walkName = "move_m@bag";
            break;
        case 16:
            walkName = "MOVE_M@BAIL_BOND_NOT_TAZERED";
            break;
        case 17:
            walkName = "MOVE_M@BAIL_BOND_TAZERED";
            break;
        case 18:
            walkName = "move_m@brave";
            break;
        case 19:
            walkName = "move_m@casual@d";
            break;
        case 20:
            walkName = "MOVE_M@DRUNK@MODERATEDRUNK";
            break;
        case 21:
            walkName = "MOVE_M@DRUNK@MODERATEDRUNK_HEAD_UP";
            break;
        case 22:
            walkName = "MOVE_M@DRUNK@SLIGHTLYDRUNK";
            break;
        case 23:
            walkName = "MOVE_M@DRUNK@VERYDRUNK";
            break;
        case 24:
            walkName = "move_m@fire";
            break;
        case 25:
            walkName = "move_m@gangster@var_e";
            break;
        case 26:
            walkName = "move_m@gangster@var_f";
            break;
        case 27:
            walkName = "move_m@gangster@var_i";
            break;
        case 28:
            walkName = "move_m@JOG@";
            break;
        case 29:
            walkName = "MOVE_M@PRISON_GAURD";
            break;
        case 30:
            walkName = "MOVE_P_M_ONE";
            break;
        case 31:
            walkName = "MOVE_P_M_ONE_BRIEFCASE";
            break;
        case 32:
            walkName = "move_p_m_zero_janitor";
            break;
        case 33:
            walkName = "move_p_m_zero_slow";
            break;
        case 34:
            walkName = "move_ped_bucket";
            break;
        case 35:
            walkName = "move_ped_mop";
            break;
        case 36:
            walkName = "MOVE_M@FEMME@";
            break;
        case 37:
            walkName = "MOVE_M@GANGSTER@NG";
            break;
        case 38:
            walkName = "MOVE_F@GANGSTER@NG";
            break;
        case 39:
            walkName = "MOVE_M@POSH@";
            break;
        case 40:
            walkName = "MOVE_F@POSH@";
            break;
        case 41:
            walkName = "MOVE_M@TOUGH_GUY@";
            break;
        case 42:
            walkName = "MOVE_F@TOUGH_GUY@";
            break;
        default:
            walkName = "ANIM_GROUP_MOVE_BALLISTIC";
            break;
    }
    if (!native.hasClipSetLoaded(walkName)) {
        native.requestClipSet(walkName);
        var timeout = alt.setTimeout(() => {
            alt.clearTimeout(timeout);
            if (native.hasClipSetLoaded(walkName)) {
                native.setPedMovementClipset(playerId, walkName, 1);
                return;
            }
        }, 1000);
    }
    native.setPedMovementClipset(playerId, walkName, 1);
});
export var playerDimension = 0;
alt.setInterval(() => {
    alt.emitServer('FetchPlayerDimension');
}, 5000);
alt.onServer('SendPlayerDimension', (dimension) => {
    playerDimension = dimension;
});
alt.onServer('SetCuffState', (state) => {
    var scriptId = alt.Player.local.scriptID;
    var dict = "mp_arresting";
    var name = "idle";
    var flag = 49;
    var duration = -1;
    if (state) {
        native.setEnableHandcuffs(scriptId, true);
        if (native.hasAnimDictLoaded(dict)) {
            native.taskPlayAnim(scriptId, dict, name, 1, -1, duration, flag, 1.0, false, false, false);
            return;
        }
        let result = loadAnimation(dict);
        result.then(() => {
            native.taskPlayAnim(scriptId, dict, name, 1, -1, duration, flag, 1.0, false, false, false);
        });
    }
    else {
        native.setEnableHandcuffs(scriptId, false);
        native.uncuffPed(scriptId);
        native.clearPedTasks(scriptId);
    }
});
function loadAnimation(dict) {
    return new Promise(resolve => {
        native.requestAnimDict(dict);
        let count = 0;
        let inter = alt.setInterval(() => {
            if (count > 255) {
                alt.clearInterval(inter);
                return;
            }
            if (native.hasAnimDictLoaded(dict)) {
                resolve(true);
                alt.clearInterval(inter);
                return;
            }
            count += 1;
        }, 5);
    });
}
alt.onServer('SendPlayerLogout', () => {
    native.restartGame();
});
