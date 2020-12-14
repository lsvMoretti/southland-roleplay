import * as alt from 'alt-client';
import * as native from 'natives';
class VehicleSounds {
    constructor(vehicle, soundId) {
        this.vehicle = vehicle;
        this.soundId = soundId;
    }
}
class HornTypes {
    constructor(vehicleModel, vehicleType) {
        this.vehicleModel = vehicleModel;
        this.vehicleType = vehicleType;
    }
}
var VehicleType;
(function (VehicleType) {
    VehicleType[VehicleType["EMS"] = 0] = "EMS";
    VehicleType[VehicleType["Fire"] = 1] = "Fire";
    VehicleType[VehicleType["FBI"] = 2] = "FBI";
    VehicleType[VehicleType["Police"] = 3] = "Police";
})(VehicleType || (VehicleType = {}));
let nativeUiMenuOpen = false;
let activeHorns = new Array();
let hornTypes = new Array(new HornTypes("AMBULANCE", VehicleType.EMS), new HornTypes("LGUARD", VehicleType.EMS), new HornTypes("FIRETRUK", VehicleType.Fire), new HornTypes("PBFD_E2", VehicleType.Fire), new HornTypes("FBI", VehicleType.FBI), new HornTypes("FBI2", VehicleType.FBI));
let hornKeyDown = false;
alt.on('keyup', (key) => {
});
alt.on('keydown', (key) => {
});
alt.everyTick(() => {
    let playerVehicle = alt.Player.local.vehicle;
    if (playerVehicle == undefined)
        return;
    if (native.getVehicleClass(playerVehicle.scriptID) != 18)
        return;
    let isDriver = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, -1, 0) == alt.Player.local.scriptID;
    let isFrontPass = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, 0, 0) == alt.Player.local.scriptID;
    if (!isDriver && !isFrontPass)
        return;
    if (nativeUiMenuOpen)
        return;
    native.disableControlAction(0, 86, true);
    if (native.isDisabledControlPressed(0, 86)) {
        hornKeyDown = true;
        alt.emitServer('newSirenHandler:HornPress', playerVehicle);
    }
    if (native.isDisabledControlJustReleased(0, 86)) {
        if (!hornKeyDown)
            return;
        hornKeyDown = false;
        alt.emitServer('newSirenHandler:HornRelease', playerVehicle);
    }
});
alt.onServer('newSirenHandler:HornActive', hornActive);
function hornActive(vehicle) {
    let hornAlreadyActive = false;
    activeHorns.forEach((vehicleSound) => {
        if (vehicleSound.vehicle == vehicle) {
            hornAlreadyActive = true;
            return;
        }
    });
    if (hornAlreadyActive)
        return;
    let soundId = native.getSoundId();
    let vehModel = native.getEntityModel(vehicle.scriptID);
    let vehicleType = VehicleType.Police;
    hornTypes.forEach((hornType) => {
        let hashKey = native.getHashKey(hornType.vehicleModel);
        if (hashKey == vehModel) {
            vehicleType = hornType.vehicleType;
        }
    });
    if (vehicleType == VehicleType.Fire) {
        native.playSoundFromEntity(soundId, "VEHICLES_HORNS_FIRETRUCK_WARNING", vehicle.scriptID, null, false, 0);
    }
    else {
        native.playSoundFromEntity(soundId, "SIRENS_AIRHORN", vehicle.scriptID, null, false, 0);
    }
    activeHorns.push(new VehicleSounds(vehicle, soundId));
}
alt.onServer('newSirenHandler:HornRelease', onHornRelease);
function onHornRelease(vehicle) {
    let activeHorn = undefined;
    activeHorns.forEach((vehicleSound) => {
        if (vehicleSound.vehicle == vehicle) {
            activeHorn = vehicleSound;
        }
    });
    if (activeHorn == undefined)
        return;
    native.stopSound(activeHorn.soundId);
    native.releaseSoundId(activeHorn.soundId);
    const index = activeHorns.indexOf(activeHorn, 0);
    if (index > -1) {
        activeHorns.splice(index, 1);
    }
}
export function SetNativeUiState(state) {
    nativeUiMenuOpen = state;
}
