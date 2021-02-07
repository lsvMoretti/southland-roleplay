import * as alt from 'alt-client';
import * as native from 'natives';
import * as extension from "./extensions";
import * as chatHandler from './chat';
import * as sirenHandler from './vehicle/sirenHandler';
import * as cruiseControl from './vehicle/cruiseControl';
import * as vehicleHandler from './vehicle/vehicleHandler';
import Fingerpointing from './animation/fingerpointing';

import { getEditObjectStatus, onKeyDownEvent } from "./objects/objectPreview";
import * as Animation from "./animation";

var IsSpawned = false;
var IsChatOpen = false;
var leftIndicator = false;
var rightIndicator = false;
var handBrake = false;
var leftAltDown = false;
var leftCtrlDown = false;
var onlinePlayerWindow: alt.WebView;
var cursorState = false;

var nativeUiMenuOpen = false;

let pointing = new Fingerpointing();

export function SetNativeUiState(state: boolean) {
    nativeUiMenuOpen = state;
}

alt.on('ChatStatus',
    (status: boolean) => {
        IsChatOpen = status;
    });

alt.onServer('setPlayerSpawned', (toggle: boolean) => {
    IsSpawned = toggle;
});

var sirenMute: boolean = false;

alt.setInterval(() => {

    let isCuffed:boolean = alt.Player.local.getSyncedMeta('IsCuffed');

    if(isCuffed){
        native.disableControlAction(0, 21, true);
        native.disableControlAction(0, 22, true);
        native.disableControlAction(0, 23, true);
        native.disableControlAction(0, 24, true);
        native.disableControlAction(0, 140, true);
        native.disableControlAction(0, 141, true);
        native.disableControlAction(0, 142, true);
    }

    //Tab - INPUT_SELECT_WEAPON

    native.disableControlAction(0, 37, true);

    if(native.isDisabledControlJustReleased(0, 37)){
        alt.emitServer('WeaponChange:TabReleased');
    }

    // Weapon Switch
    native.disableControlAction(0, 99, true);
    native.disableControlAction(0, 100, true);
    native.disableControlAction(0, 157, true);
    native.disableControlAction(0, 158, true);
    native.disableControlAction(0, 159, true);
    native.disableControlAction(0, 160, true);
    native.disableControlAction(0, 161, true);
    native.disableControlAction(0, 162, true);
    native.disableControlAction(0, 163, true);
    native.disableControlAction(0, 164, true);
    native.disableControlAction(0, 165, true);
    native.disableControlAction(0, 261, true);
    native.disableControlAction(0, 262, true);

    // P Menu
    native.disableControlAction(0, 199, true);

    // Home Key
    native.disableControlAction(0, 212, true);
    native.disableControlAction(0, 213, true);
}, 0);

alt.on('keyup', async (key) => {
    if (chatHandler.IsChatOpen() || nativeUiMenuOpen || vehicleHandler.IsScrambleOpen()) return;

    if (getEditObjectStatus()) {
        return;
    }

    if(key == 66){
        // B Key
        pointing.stop();
    }

    if(key === 120){
        // F9
        let permissionState = alt.getPermissionState(alt.Permission.ScreenCapture);

        if(permissionState == alt.PermissionState.Allowed){
            alt.log('Allowed');
            let screenshot = await alt.takeScreenshot();
            
            alt.emitServer('Player:TakeScreenshot', screenshot);
        }
        else{
            alt.log(permissionState);
        }


    }

    if (key === 187) {
        // = key
        cruiseControl.OnCruiseControlPress();
        return;
    }

    if (key === 0x11) {
        leftCtrlDown = false;
    }

    if (key === 0x72) {
        alt.emitServer('KeyUpEvent', 'F3');
    }

    if (key === 78) { // Q = 0x51 - N = 78
        var isDriver: boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, -1, 0) == alt.Player.local.scriptID;
        var isFrontPass: boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, 0, 0) == alt.Player.local.scriptID;

        if (native.getVehicleClass(alt.Player.local.vehicle.scriptID) == 18 && isDriver || isFrontPass) {
            sirenHandler.toggleSirenLightState();
        }
    }

    /*if (key === 0x70) {
        // F1 Key
        if (!IsSpawned) return;

        if (!cursorState) {
            alt.showCursor(true);
            cursorState = true;
            return;
        }

        alt.showCursor(false);
        cursorState = false;
    }*/

    if (key === 0x71) {
        // F2
        if (!IsSpawned) return;

        if (onlinePlayerWindow !== undefined) {
            alt.showCursor(false);
            onlinePlayerWindow.destroy();
            onlinePlayerWindow = undefined;
            return;
        } else {
            alt.showCursor(true);
            onlinePlayerWindow = new alt.WebView("http://resource/files/html/onlinePlayers.html", false);
            onlinePlayerWindow.focus();
            onlinePlayerWindow.on('requestOnlinePlayerList', requestOnlinePlayers);
            onlinePlayerWindow.on('ClosePlayerList', ClosePlayerList);
        }
    }

    if (key === 0x25) {
        // Left Arrow

        if (!IsSpawned) return;

        if (alt.Player.local.vehicle !== null) {
            // In vehicle
            if (!leftIndicator) {
                leftIndicator = true;
                //native.setVehicleIndicatorLights(alt.Player.local.vehicle.scriptID, 1, true);
            } else {
                leftIndicator = false;
                //native.setVehicleIndicatorLights(alt.Player.local.vehicle.scriptID, 1, false);
            }
            alt.emitServer('IndicatorStateChange', 0, leftIndicator);
        }
    }

    if (key === 0x27) {
        // Right Arrow

        if (!IsSpawned) return;

        if (alt.Player.local.vehicle !== null) {
            // In vehicle
            if (!rightIndicator) {
                rightIndicator = true;
                //native.setVehicleIndicatorLights(alt.Player.local.vehicle.scriptID, 0, true);
            } else {
                rightIndicator = false;
                //native.setVehicleIndicatorLights(alt.Player.local.vehicle.scriptID, 0, false);
            }
            alt.emitServer('IndicatorStateChange', 1, rightIndicator);
        }
    }

    if (key === 0x12) {
        // Left Alt
        leftAltDown = false;
    }

    if (key === 0x20) {
        // Space
        if (!IsSpawned) return;

        /*if (ctrlDown) {
            if (!handBrake) {
                handBrake = true;
                native.setVehicleHandbrake(alt.Player.local.vehicle.scriptID, true);
            } else {
                handBrake = false;
                native.setVehicleHandbrake(alt.Player.local.vehicle.scriptID, false);
            }
        }*/
    }

    if (key === 0x46) {
        alt.emitServer('KeyUpEvent', 'f');
    }

    if (key === 0x01) {
        // Left Mouse Button
        alt.emitServer('KeyUpEvent', 'LMB');
    }

    if (key === 0x59) {
        alt.emitServer('KeyUpEvent', 'y');
    }

    if (key === 0x4C) {
        // L
        alt.emitServer('KeyUpEvent', 'l');
    }

    if (key === 0x45) {
        alt.log('E Key');
        // E Key
        if (leftAltDown) {
            alt.log("Alt+E");
            alt.emitServer('KeyUpEvent', 'ctrle');
        }
    }

    if (key === 0xBE) {
        // Period Key
        if (alt.Player.local.vehicle != undefined) {
            // In Vehicle

            var isDriver: boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, -1, 0) == alt.Player.local.scriptID;
            var isFrontPass: boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, 0, 0) == alt.Player.local.scriptID;

            if (native.getVehicleClass(alt.Player.local.vehicle.scriptID) == 18 && isDriver || isFrontPass) {
                sirenHandler.nextSirenStage();
            }
        }
    }

    if (key === 0x4D) {
        // M Key
        if (alt.Player.local.vehicle != undefined) {
            // In Vehicle

            var isDriver: boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, -1, 0) == alt.Player.local.scriptID;
            var isFrontPass: boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, 0, 0) == alt.Player.local.scriptID;

            if (native.getVehicleClass(alt.Player.local.vehicle.scriptID) == 18 && isDriver || isFrontPass) {
                sirenHandler.previousSirenStage();
            }
        }
    }

    if (alt.Player.local.vehicle != undefined) {
        // In Vehicle

        if (leftCtrlDown) {
            var pVeh: alt.Vehicle = alt.Player.local.vehicle;
            var pVehId: number = pVeh.scriptID;

            var isDriver: boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, -1, 0) == alt.Player.local.scriptID;
            var isFrontPass: boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, 0, 0) == alt.Player.local.scriptID;

            if (key === 0x31) {
                // 1
                if (native.isVehicleExtraTurnedOn(pVehId, 1)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 1, true);
                    alt.log('Extra1 True');
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 1, false);
                    alt.log('Extra1 False');
                }
            }
            if (key === 0x32) {
                // 2
                if (native.isVehicleExtraTurnedOn(pVehId, 2)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 2, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 2, false);
                }
            }
            if (key === 0x33) {
                // 3
                if (native.isVehicleExtraTurnedOn(pVehId, 3)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 3, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 3, false);
                }
            }
            if (key === 0x34) {
                // 4
                if (native.isVehicleExtraTurnedOn(pVehId, 4)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 4, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 4, false);
                }
            }
            if (key === 0x35) {
                // 5
                if (native.isVehicleExtraTurnedOn(pVehId, 5)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 5, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 5, false);
                }
            }
            if (key === 0x36) {
                // 6
                if (native.isVehicleExtraTurnedOn(pVehId, 6)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 6, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 6, false);
                }
            }
            if (key === 0x37) {
                // 7
                if (native.isVehicleExtraTurnedOn(pVehId, 7)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 7, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 7, false);
                }
            }
            if (key === 0x38) {
                // 8
                if (native.isVehicleExtraTurnedOn(pVehId, 8)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 8, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 8, false);
                }
            }
            if (key === 0x39) {
                // 9
                if (native.isVehicleExtraTurnedOn(pVehId, 9)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 9, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 9, false);
                }
            }
            if (key === 0x30) {
                // 0
                if (native.isVehicleExtraTurnedOn(pVehId, 10)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 10, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 10, false);
                }
            }
            if (key === 0x76) {
                // F7
                if (native.isVehicleExtraTurnedOn(pVehId, 11)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 11, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 11, false);
                }
            }
            if (key === 0x77) {
                // F8
                if (native.isVehicleExtraTurnedOn(pVehId, 12)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 12, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 12, false);
                }
            }
            if (key === 0x78) {
                // F9
                if (native.isVehicleExtraTurnedOn(pVehId, 13)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 13, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 13, false);
                }
            }
            if (key === 0x79) {
                // F10
                if (native.isVehicleExtraTurnedOn(pVehId, 14)) {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 14, true);
                }
                else {
                    alt.emitServer("VehicleHandler:ToggleExtra", pVeh, 14, false);
                }
            }
            if (key === 0x7A) {
                // F11
                if (native.areBombBayDoorsOpen(pVehId)) {
                    alt.emitServer("VehicleHandler:SetBombBayDoorState", pVeh, true);
                }
                else {
                    alt.emitServer("VehicleHandler:SetBombBayDoorState", pVeh, false);
                }
            }
        }
    }
});

alt.on('keydown', (key) => {
    if (getEditObjectStatus()) {
        onKeyDownEvent(key);
        return;
    }

    if (key === 0x12) {
        // Left Alt
        alt.log('Left Alt Down');
        leftAltDown = true;
    }

    if (key === 0x11) {
        leftCtrlDown = true;
    }

    if(key == 66){
        // B Key
        pointing.start();
    }
});

var crouchToggle: boolean;

alt.on('EnteredVehicle', () => {
    crouchToggle = false;
});

alt.setInterval(() => {
    if (!IsSpawned) return;

    if (native.isControlPressed(0, 48)) {
        // INPUT_HUD_SPECIAL
        native.setBigmapActive(true, false);
    }
    if (native.isControlJustReleased(0, 48)) {
        native.setBigmapActive(false, false);
    }

    // Vehicle

    if (alt.Player.local.vehicle != undefined) {
        // In Vehicle

        if (native.getVehicleClass(alt.Player.local.vehicle.scriptID) == 18) {
            // Disable Horn - Enable Down Arrow

            var isDriver: boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, -1, 0) == alt.Player.local.scriptID;
            var isFrontPass: boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, 0, 0) == alt.Player.local.scriptID;

            if (!nativeUiMenuOpen) {
                if (isDriver || isFrontPass) {
                    native.disableControlAction(0, 173, true);
                    native.disableControlAction(0, 85, true);
                    native.disableControlAction(0, 80, true);

                    if (native.isDisabledControlJustPressed(0, 173)) {
                        sirenHandler.toggleSirenState();
                    }

                    if (native.isDisabledControlJustPressed(0, 80)) {
                        sirenHandler.blipSiren();
                    }
                }
            }
        }
    }

    // Passenger
    native.disableControlAction(0, 58, true);
    if (native.isDisabledControlJustPressed(0, 58)) {
        enterVehicleAsPassenger();
        return;
    }
    // Driver
    native.disableControlAction(0, 23, true);
    if (native.isDisabledControlJustPressed(0, 23)) {
        enterVehicleAsDriver();
        return;
    }

    // Left Ctrl - Crouch
    native.disableControlAction(0, 36, true);
    if (native.isDisabledControlJustReleased(0, 36)) {
        if (!native.hasClipSetLoaded('move_ped_crouched')) {
            native.requestClipSet('move_ped_crouched');
        }

        var lastCrouch = crouchToggle;
        crouchToggle = !crouchToggle;

        var scriptId = alt.Player.local.scriptID;

        if (alt.Player.local.vehicle != null) return;

        if (!crouchToggle && lastCrouch) {
            alt.log('Cancel Crouch');
            alt.emitServer('Player:CancelCrouch');
            native.resetPedMovementClipset(scriptId, 0);
            return;
        } else {
            alt.log('Crouching');
            native.setPedMovementClipset(scriptId, "move_ped_crouched", 1.0);
        }
    }
}, 0);

function enterVehicleAsDriver() {
    if (alt.Player.local.vehicle == null) {
        const player = alt.Player.local;

        var vehicles: alt.Vehicle[] = alt.Vehicle.all;
        var closestVehicle: alt.Vehicle;
        var playerPos = alt.Player.local.pos;

        var lastDistance: number = 5;

        alt.log('Searching through ' + vehicles.length + ' vehicles.');

        vehicles.forEach(vehicle => {
            var vehiclePosition = vehicle.pos;
            var distance = Distance(playerPos, vehiclePosition);
            if (distance < lastDistance) {
                closestVehicle = vehicle;
                lastDistance = distance;
            }
        });

        if (closestVehicle == undefined) return;
        var vehicle = closestVehicle.scriptID;

        alt.log('Vehicle: ' + vehicle);

        if (native.getVehicleClass(vehicle) === 8 || native.getVehicleClass(vehicle) === 14) {
            if (native.isVehicleSeatFree(vehicle, -1, false)) {
                native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, -1, 2, 1, 0);
            } else {
                return;
            }
        }

        let boneFLDoor = native.getEntityBoneIndexByName(vehicle, 'door_dside_f');//Front Left
        const posFLDoor = native.getWorldPositionOfEntityBone(vehicle, boneFLDoor);
        let FlPos = new alt.Vector3(posFLDoor.x, posFLDoor.y, posFLDoor.z );
        const distFLDoor = Distance(FlPos, alt.Player.local.pos);

        let boneFRDoor = native.getEntityBoneIndexByName(vehicle, 'door_pside_f');//Front Right
        const posFRDoor = native.getWorldPositionOfEntityBone(vehicle, boneFRDoor);
        let FrPos = new alt.Vector3(posFRDoor.x, posFRDoor.y, posFRDoor.z );
        const distFRDoor = Distance(FrPos, alt.Player.local.pos);

        if (native.isVehicleSeatFree(vehicle, -1, false)) {
            let vehicleClass = native.getVehicleClass(vehicle);
            if (vehicleClass == 14) {
                // Boats
                native.setPedIntoVehicle(player.scriptID, vehicle, -1);
            } else {
                native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, -1, 2, 1, 0);
            }
        } else {
            if (distFRDoor < distFLDoor) return;

            native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, -1, 2, 1, 0);
        }
    }
}

function enterVehicleAsPassenger() {
    if (alt.Player.local.vehicle) return;

    var vehicles: alt.Vehicle[] = alt.Vehicle.all;
    var closestVehicle: alt.Vehicle = undefined;
    var playerPos = alt.Player.local.pos;

    var lastDistance: number = 5;

    alt.log('Searching through ' + vehicles.length + ' vehicles.');

    vehicles.forEach(vehicle => {
        var vehiclePosition = vehicle.pos;
        var distance = Distance(playerPos, vehiclePosition);
        if (distance < lastDistance) {
            closestVehicle = vehicle;
            lastDistance = distance;
        }
    });

    if (closestVehicle == undefined) return;
    var vehicle = closestVehicle.scriptID;

    alt.log('Vehicle: ' + vehicle);

    // New Entry

    let numberSeats = native.getVehicleModelNumberOfSeats(closestVehicle.model);

    alt.log(`Number of seats: ${numberSeats}.`);

    let seatCount: number = 0;

    for (let i = 0; i < numberSeats; i++) {
        let seatFree: boolean = native.isVehicleSeatFree(vehicle, i, false);

        if (!seatFree) {
            seatCount++;
        }
    }

    alt.log(`Number of seats taken: ${seatCount}.`);

    if (seatCount == numberSeats) return;

    //if (!native.isVehicleSeatFree(vehicle, 0, false) && !native.isVehicleSeatFree(vehicle, 1, false) && !native.isVehicleSeatFree(vehicle, 2, false)) return;

    if (native.getVehicleClass(vehicle) === 8 || native.getVehicleClass(vehicle) === 14 || native.getVehicleClass(vehicle) === 16) {
        let inSeat: boolean = false;

        for (let i = 0; i < numberSeats; i++) {
            if (inSeat) return;

            let seatFree: boolean = native.isVehicleSeatFree(vehicle, i, false);

            if (seatFree) {
                native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, i, 2, 1, 0);
                return;
            }
        }
        /*
        if (native.isVehicleSeatFree(vehicle, 0, false)) {
            native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, 0, 2, 1, 0);
        }
        else if (native.isVehicleSeatFree(vehicle, 1, false)) {
            native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, 1, 2, 1, 0);
        }
        else if (native.isVehicleSeatFree(vehicle, 2, false)) {
            native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, 2, 2, 1, 0);
        }
        else {
            return;
        }*/
    }

    let boneFRDoor = native.getEntityBoneIndexByName(vehicle, 'door_pside_f');//Front right
    const posFRDoor = native.getWorldPositionOfEntityBone(vehicle, boneFRDoor);
    let frDoorPos = new alt.Vector3(posFRDoor.x, posFRDoor.y, posFRDoor.z);
    const distFRDoor = Distance(frDoorPos, alt.Player.local.pos);

    let boneBLDoor = native.getEntityBoneIndexByName(vehicle, 'door_dside_r');//Back Left
    const posBLDoor = native.getWorldPositionOfEntityBone(vehicle, boneBLDoor);
    let blDoorPos = new alt.Vector3(posBLDoor.x, posBLDoor.y, posBLDoor.z);
    const distBLDoor = Distance(blDoorPos, alt.Player.local.pos);

    let boneBRDoor = native.getEntityBoneIndexByName(vehicle, 'door_pside_r');//Back Right
    const posBRDoor = native.getWorldPositionOfEntityBone(vehicle, boneBRDoor);
    let brDoorPos = new alt.Vector3(posBRDoor.x, posBRDoor.y, posBRDoor.z);
    const distBRDoor = Distance(brDoorPos, alt.Player.local.pos);

    let minDist = Math.min(distFRDoor, distBLDoor, distBRDoor);

    if (minDist == distFRDoor) {
        if (minDist > 1.8) return;

        if (native.isVehicleSeatFree(vehicle, 0, false)) {
            native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, 0, 2, 1, 0);
        } else if (native.isVehicleSeatFree(vehicle, 2, false)) {
            native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, 2, 2, 1, 0);
        }
        else {
            return;
        }
    }
    if (minDist == distBLDoor) {
        if (minDist > 1.8) return;

        if (native.isVehicleSeatFree(vehicle, 1, false)) {
            native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, 1, 2, 1, 0);
        } else {
            return;
        }
    }
    if (minDist == distBRDoor) {
        if (minDist > 1.8) return;

        if (native.isVehicleSeatFree(vehicle, 2, false)) {
            native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, 2, 2, 1, 0);
        } else if (native.isVehicleSeatFree(vehicle, 0, false)) {
            native.taskEnterVehicle(alt.Player.local.scriptID, vehicle, 5000, 0, 2, 1, 0);
        } else {
            return;
        }
    }
}

function Distance(vector1: alt.Vector3, vector2: alt.Vector3) {
    if (vector1 === undefined || vector2 === undefined) {
        throw new Error('AddVector => vector1 or vector2 is undefined');
    }

    return Math.sqrt(
        Math.pow(vector1.x - vector2.x, 2) +
        Math.pow(vector1.y - vector2.y, 2) +
        Math.pow(vector1.z - vector2.z, 2)
    );
}

alt.onServer('OnlinePlayerList', (json: string) => {
    if (onlinePlayerWindow === undefined) return;
    onlinePlayerWindow.emit('SendOnlinePlayers', json);
});

function requestOnlinePlayers() {
    alt.emitServer('SendRequestOnlinePlayers');
}

function ClosePlayerList() {
    alt.showCursor(false);
    onlinePlayerWindow.destroy();
    onlinePlayerWindow = undefined;
}